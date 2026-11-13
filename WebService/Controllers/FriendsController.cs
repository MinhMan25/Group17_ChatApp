using Microsoft.AspNetCore.Mvc;
using WebService.Data;
using WebService.Infrastructure;
using WebService.Models;

namespace WebService.Controllers;

/// <summary>Nhóm chức năng 2 - Quản lý bạn bè.</summary>
[Route("api/friends")]
public class FriendsController : ApiControllerBase
{
    public FriendsController(Db db) : base(db) { }

    /// <summary>GET /api/friends - danh sách bạn bè, người Online lên đầu.</summary>
    [HttpGet]
    public ActionResult<List<UserDto>> List() => Db.Query(
        $"SELECT {Sql.UserColumns} FROM Users u WHERE u.Username IN (" +
        "  SELECT ToUser FROM Friendships WHERE FromUser = @me AND Status = 'ACCEPTED' " +
        "  UNION SELECT FromUser FROM Friendships WHERE ToUser = @me AND Status = 'ACCEPTED') " +
        "ORDER BY IsOnline DESC, u.DisplayName",
        new { me = Me }, UserDto.Map);

    /// <summary>GET /api/friends/requests - lời mời đã nhận và đã gửi (đang chờ).</summary>
    [HttpGet("requests")]
    public ActionResult<FriendRequestsDto> Requests()
    {
        var received = Db.Query(
            $"SELECT {Sql.UserColumns} FROM Friendships f JOIN Users u ON u.Username = f.FromUser " +
            "WHERE f.ToUser = @me AND f.Status = 'PENDING' ORDER BY f.CreatedAt DESC",
            new { me = Me }, UserDto.Map);
        var sent = Db.Query(
            $"SELECT {Sql.UserColumns} FROM Friendships f JOIN Users u ON u.Username = f.ToUser " +
            "WHERE f.FromUser = @me AND f.Status = 'PENDING' ORDER BY f.CreatedAt DESC",
            new { me = Me }, UserDto.Map);
        return new FriendRequestsDto(received, sent);
    }

    /// <summary>POST /api/friends/requests/{username} - gửi lời mời. Nếu người kia đã mời mình thì thành bạn luôn.</summary>
    [HttpPost("requests/{username}")]
    public ActionResult<ResultDto> SendRequest(string username)
    {
        if (string.Equals(username, Me, StringComparison.OrdinalIgnoreCase))
            throw ApiError.BadRequest("CANNOT_ADD_SELF", "Không thể kết bạn với chính mình.");
        if (!UserExists(username)) throw ApiError.NotFound("USER_NOT_FOUND", "Không tìm thấy người dùng.");
        if (AreFriends(Me, username)) throw ApiError.Conflict("ALREADY_FRIENDS", "Hai người đã là bạn bè.");

        // Người kia đã gửi lời mời cho mình trước => chấp nhận luôn
        var accepted = Db.Execute(
            "UPDATE Friendships SET Status = 'ACCEPTED' WHERE FromUser = @username AND ToUser = @me AND Status = 'PENDING'",
            new { username, me = Me });
        if (accepted > 0) return new ResultDto("FRIEND_ADDED", "Người này đã mời bạn trước, hai người đã thành bạn bè.");

        if (Db.Exists("SELECT COUNT(*) FROM Friendships WHERE FromUser = @me AND ToUser = @username", new { me = Me, username }))
            throw ApiError.Conflict("REQUEST_EXISTS", "Bạn đã gửi lời mời cho người này rồi.");

        Db.Execute("INSERT INTO Friendships (FromUser, ToUser, Status) VALUES (@me, @username, 'PENDING')", new { me = Me, username });
        return new ResultDto("REQUEST_SENT", "Đã gửi lời mời kết bạn.");
    }

    /// <summary>POST /api/friends/requests/{username}/accept - chấp nhận lời mời của username.</summary>
    [HttpPost("requests/{username}/accept")]
    public ActionResult<ResultDto> Accept(string username)
    {
        var count = Db.Execute(
            "UPDATE Friendships SET Status = 'ACCEPTED' WHERE FromUser = @username AND ToUser = @me AND Status = 'PENDING'",
            new { username, me = Me });
        if (count == 0) throw ApiError.NotFound("REQUEST_NOT_FOUND", "Không có lời mời nào từ người này.");
        return new ResultDto("FRIEND_ADDED", "Đã chấp nhận lời mời kết bạn.");
    }

    /// <summary>DELETE /api/friends/requests/{username} - từ chối lời mời nhận được, hoặc huỷ lời mời đã gửi.</summary>
    [HttpDelete("requests/{username}")]
    public ActionResult<ResultDto> RejectOrCancel(string username)
    {
        var count = Db.Execute(
            "DELETE FROM Friendships WHERE Status = 'PENDING' AND " +
            "((FromUser = @username AND ToUser = @me) OR (FromUser = @me AND ToUser = @username))",
            new { username, me = Me });
        if (count == 0) throw ApiError.NotFound("REQUEST_NOT_FOUND", "Không có lời mời nào giữa hai người.");
        return new ResultDto("REQUEST_REMOVED", "Đã xoá lời mời kết bạn.");
    }

    /// <summary>DELETE /api/friends/{username} - xoá bạn.</summary>
    [HttpDelete("{username}")]
    public ActionResult<ResultDto> Unfriend(string username)
    {
        var count = Db.Execute(
            "DELETE FROM Friendships WHERE Status = 'ACCEPTED' AND " +
            "((FromUser = @username AND ToUser = @me) OR (FromUser = @me AND ToUser = @username))",
            new { username, me = Me });
        if (count == 0) throw ApiError.NotFound("NOT_FRIENDS", "Hai người chưa là bạn bè.");
        return new ResultDto("FRIEND_REMOVED", "Đã xoá bạn.");
    }
}
