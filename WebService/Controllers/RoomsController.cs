using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebService.Data;
using WebService.Infrastructure;
using WebService.Models;

namespace WebService.Controllers;

/// <summary>
/// Nhóm chức năng 3, 4 - Phòng chat và tin nhắn.
/// Phòng chat giống lớp ChatRoom của server TCP: Global (GLOBAL), Group (nhóm), Private (chat riêng).
/// Nhận tin mới: client hỏi định kỳ GET /api/rooms/{roomId}/messages?afterId=&lt;id tin cuối đang có&gt; (polling).
/// </summary>
[Route("api")]
public class RoomsController : ApiControllerBase
{
    private const long MaxFileBytes = 20 * 1024 * 1024;

    public RoomsController(Db db) : base(db) { }

    // ================================================================ PHÒNG CHAT

    /// <summary>GET /api/rooms - các phòng của tôi, kèm tin nhắn cuối, mới nhất lên đầu.</summary>
    [HttpGet("rooms")]
    public ActionResult<List<RoomDto>> ListRooms() => Db.Query(
        "SELECT r.RoomId, r.RoomType, r.CreatedAt, " +
        "  CASE WHEN r.RoomType = 'Private' THEN o.DisplayName ELSE r.RoomName END AS Title, " +
        "  CASE WHEN r.RoomType = 'Private' THEN o.AvatarPath ELSE r.AvatarPath END AS Avatar, " +
        "  (SELECT COUNT(*) FROM RoomMembers x WHERE x.RoomId = r.RoomId) AS MemberCount, " +
        "  lm.Content, lm.FileName, lm.IsDeleted, lm.SentAt, lm.SenderName, o.IsOnline AS OtherIsOnline " +
        "FROM RoomMembers me " +
        "JOIN ChatRooms r ON r.RoomId = me.RoomId " +
        "OUTER APPLY (SELECT TOP 1 m.Content, m.FileName, m.IsDeleted, m.SentAt, su.DisplayName AS SenderName " +
        "             FROM Messages m JOIN Users su ON su.Username = m.Sender " +
        "             WHERE m.RoomId = r.RoomId ORDER BY m.MessageId DESC) lm " +
        $"OUTER APPLY (SELECT TOP 1 u.DisplayName, u.AvatarPath, {Sql.IsOnline} " +
        "             FROM RoomMembers om JOIN Users u ON u.Username = om.Username " +
        "             WHERE r.RoomType = 'Private' AND om.RoomId = r.RoomId AND om.Username <> @me) o " +
        "WHERE me.Username = @me " +
        "ORDER BY COALESCE(lm.SentAt, r.CreatedAt) DESC",
        new { me = Me },
        r =>
        {
            string? last = null;
            if (r["SentAt"] is not DBNull)
                last = r.Bool("IsDeleted") ? "Tin nhắn đã bị xoá"
                    : r.StrOrNull("Content") ?? "[Tệp] " + r.StrOrNull("FileName");
            return new RoomDto(r.Str("RoomId"), r.Str("Title"), r.Str("RoomType"), r.StrOrNull("Avatar"), r.Int("MemberCount"),
                last, r.StrOrNull("SenderName"), r.DateOrNull("SentAt"),
                r["OtherIsOnline"] is DBNull ? null : r.Bool("OtherIsOnline"));
        });

    /// <summary>GET /api/rooms/{roomId} - thông tin phòng và danh sách thành viên.</summary>
    [HttpGet("rooms/{roomId}")]
    public ActionResult<RoomDetailDto> GetRoom(string roomId) => LoadRoom(roomId);

    private RoomDetailDto LoadRoom(string roomId)
    {
        var membership = RequireMember(roomId);
        var members = Db.Query(
            $"SELECT u.Username, u.DisplayName, u.AvatarPath, rm.IsAdmin, {Sql.IsOnline} " +
            "FROM RoomMembers rm JOIN Users u ON u.Username = rm.Username WHERE rm.RoomId = @roomId ORDER BY rm.JoinedAt",
            new { roomId },
            r => new MemberDto(r.Str("Username"), r.Str("DisplayName"), r.StrOrNull("AvatarPath"), r.Bool("IsAdmin"), r.Bool("IsOnline")));
        var room = Db.QueryFirst("SELECT RoomName, AvatarPath FROM ChatRooms WHERE RoomId = @roomId", new { roomId },
            r => new { Name = r.Str("RoomName"), Avatar = r.StrOrNull("AvatarPath") })!;

        var name = room.Name;
        var avatar = room.Avatar;
        if (membership.RoomType == "Private")   // chat riêng: hiện tên, ảnh của người kia
        {
            var other = members.FirstOrDefault(m => !string.Equals(m.Username, Me, StringComparison.OrdinalIgnoreCase));
            name = other?.DisplayName ?? name;
            avatar = other?.AvatarPath;
        }
        return new RoomDetailDto(roomId, name, membership.RoomType, avatar, membership.IsAdmin, members);
    }

    /// <summary>POST /api/rooms/private/{username} - mở (hoặc tạo) phòng chat riêng với một người bạn.</summary>
    [HttpPost("rooms/private/{username}")]
    public ActionResult<RoomDetailDto> OpenPrivate(string username)
    {
        if (string.Equals(username, Me, StringComparison.OrdinalIgnoreCase))
            throw ApiError.BadRequest("CANNOT_CHAT_SELF", "Không thể tự nhắn tin với chính mình.");
        if (!UserExists(username)) throw ApiError.NotFound("USER_NOT_FOUND", "Không tìm thấy người dùng.");
        if (!AreFriends(Me, username)) throw ApiError.Forbidden("NOT_FRIENDS", "Hãy kết bạn trước khi nhắn tin.");

        // Mã phòng cố định cho mỗi cặp: PRIVATE_alice_bob (tên xếp theo thứ tự chữ cái)
        var pair = new[] { Me.ToLowerInvariant(), username.ToLowerInvariant() }.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var roomId = $"PRIVATE_{pair[0]}_{pair[1]}";

        if (!Db.Exists("SELECT COUNT(*) FROM ChatRooms WHERE RoomId = @roomId", new { roomId }))
        {
            try
            {
                Db.Execute("INSERT INTO ChatRooms (RoomId, RoomName, RoomType, CreatedBy) VALUES (@roomId, N'Chat riêng', 'Private', @me)",
                    new { roomId, me = Me });
                Db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES (@roomId, @me), (@roomId, @username)",
                    new { roomId, me = Me, username });
            }
            catch (SqlException ex) when (Db.IsDuplicateKey(ex))
            {
                // Người kia vừa tạo phòng cùng lúc - dùng luôn phòng đó
            }
        }
        return LoadRoom(roomId);
    }

    /// <summary>POST /api/rooms/group - tạo nhóm chat; người tạo là trưởng nhóm (IsAdmin).</summary>
    [HttpPost("rooms/group")]
    public ActionResult<RoomDetailDto> CreateGroup(CreateGroupRequest req)
    {
        var roomName = Rules.RoomName(req.RoomName);
        var members = (req.Members ?? new List<string>())
            .Select(m => m.Trim()).Where(m => m.Length > 0 && !string.Equals(m, Me, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (members.Count == 0) throw ApiError.BadRequest("NEED_MEMBERS", "Chọn ít nhất 1 người bạn để tạo nhóm.");
        foreach (var m in members)
            if (!AreFriends(Me, m)) throw ApiError.BadRequest("NOT_FRIENDS", $"{m} chưa là bạn của bạn.");

        var roomId = "GROUP_" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        Db.Execute("INSERT INTO ChatRooms (RoomId, RoomName, RoomType, CreatedBy) VALUES (@roomId, @roomName, 'Group', @me)",
            new { roomId, roomName, me = Me });
        Db.Execute("INSERT INTO RoomMembers (RoomId, Username, IsAdmin) VALUES (@roomId, @me, 1)", new { roomId, me = Me });
        foreach (var username in members)
            Db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES (@roomId, @username)", new { roomId, username });

        return LoadRoom(roomId);
    }

    /// <summary>PUT /api/rooms/{roomId}/name - đổi tên nhóm (trưởng nhóm).</summary>
    [HttpPut("rooms/{roomId}/name")]
    public ActionResult<RoomDetailDto> Rename(string roomId, RenameRoomRequest req)
    {
        RequireGroupAdmin(roomId);
        Db.Execute("UPDATE ChatRooms SET RoomName = @roomName WHERE RoomId = @roomId", new { roomName = Rules.RoomName(req.RoomName), roomId });
        return LoadRoom(roomId);
    }

    /// <summary>POST /api/rooms/{roomId}/avatar - đổi ảnh nhóm (form-data "file", trưởng nhóm).</summary>
    [HttpPost("rooms/{roomId}/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public ActionResult<RoomDetailDto> UploadRoomAvatar(string roomId, IFormFile? file)
    {
        RequireGroupAdmin(roomId);
        var path = SaveUpload(file, "rooms", imagesOnly: true, maxBytes: 5 * 1024 * 1024);
        Db.Execute("UPDATE ChatRooms SET AvatarPath = @path WHERE RoomId = @roomId", new { path, roomId });
        return LoadRoom(roomId);
    }

    /// <summary>POST /api/rooms/{roomId}/members - thêm một người bạn vào nhóm (trưởng nhóm).</summary>
    [HttpPost("rooms/{roomId}/members")]
    public ActionResult<RoomDetailDto> AddMember(string roomId, AddMemberRequest req)
    {
        RequireGroupAdmin(roomId);
        var username = (req.Username ?? "").Trim();
        if (!UserExists(username)) throw ApiError.NotFound("USER_NOT_FOUND", "Không tìm thấy người dùng.");
        if (!AreFriends(Me, username)) throw ApiError.BadRequest("NOT_FRIENDS", "Chỉ thêm được bạn bè vào nhóm.");
        if (Db.Exists("SELECT COUNT(*) FROM RoomMembers WHERE RoomId = @roomId AND Username = @username", new { roomId, username }))
            throw ApiError.Conflict("ALREADY_MEMBER", "Người này đã ở trong nhóm.");

        Db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES (@roomId, @username)", new { roomId, username });
        return LoadRoom(roomId);
    }

    /// <summary>DELETE /api/rooms/{roomId}/members/{username} - xoá thành viên khỏi nhóm (trưởng nhóm).</summary>
    [HttpDelete("rooms/{roomId}/members/{username}")]
    public ActionResult<RoomDetailDto> RemoveMember(string roomId, string username)
    {
        RequireGroupAdmin(roomId);
        if (string.Equals(username, Me, StringComparison.OrdinalIgnoreCase))
            throw ApiError.BadRequest("USE_LEAVE", "Dùng chức năng rời nhóm để tự rời.");
        var count = Db.Execute("DELETE FROM RoomMembers WHERE RoomId = @roomId AND Username = @username", new { roomId, username });
        if (count == 0) throw ApiError.NotFound("NOT_MEMBER", "Người này không ở trong nhóm.");
        return LoadRoom(roomId);
    }

    /// <summary>POST /api/rooms/{roomId}/leave - rời nhóm. Trưởng nhóm rời thì người vào sớm nhất lên thay.</summary>
    [HttpPost("rooms/{roomId}/leave")]
    public ActionResult<ResultDto> Leave(string roomId)
    {
        var membership = RequireMember(roomId);
        if (membership.RoomType != "Group") throw ApiError.BadRequest("NOT_GROUP", "Chỉ rời được nhóm chat.");

        Db.Execute("DELETE FROM RoomMembers WHERE RoomId = @roomId AND Username = @me", new { roomId, me = Me });

        if (!Db.Exists("SELECT COUNT(*) FROM RoomMembers WHERE RoomId = @roomId", new { roomId }))
            Db.Execute("DELETE FROM ChatRooms WHERE RoomId = @roomId", new { roomId });   // không còn ai: xoá nhóm
        else if (!Db.Exists("SELECT COUNT(*) FROM RoomMembers WHERE RoomId = @roomId AND IsAdmin = 1", new { roomId }))
            Db.Execute(
                "UPDATE RoomMembers SET IsAdmin = 1 WHERE RoomId = @roomId AND Username = " +
                "(SELECT TOP 1 Username FROM RoomMembers WHERE RoomId = @roomId ORDER BY JoinedAt)",
                new { roomId });

        return new ResultDto("LEFT", "Đã rời nhóm.");
    }

    // ================================================================ TIN NHẮN

    /// <summary>
    /// GET /api/rooms/{roomId}/messages
    ///   ?afterId=123  : tin MỚI hơn 123 (client gọi 1-2 giây/lần để nhận tin mới)
    ///   ?beforeId=50  : tin CŨ hơn 50 (cuộn lên xem lịch sử)
    ///   không truyền  : 50 tin gần nhất (khi mở phòng)
    /// </summary>
    [HttpGet("rooms/{roomId}/messages")]
    public ActionResult<List<MessageDto>> GetMessages(string roomId, long afterId = 0, long beforeId = 0, int take = 50)
    {
        RequireMember(roomId);
        take = Math.Clamp(take, 1, 100);

        if (afterId > 0)
            return Db.Query(
                $"SELECT TOP (@take) {MessageDto.ColumnsFrom} WHERE m.RoomId = @roomId AND m.MessageId > @afterId ORDER BY m.MessageId",
                new { take, roomId, afterId }, MessageDto.Map);

        var latest = Db.Query(
            $"SELECT TOP (@take) {MessageDto.ColumnsFrom} WHERE m.RoomId = @roomId" +
            (beforeId > 0 ? " AND m.MessageId < @beforeId" : "") + " ORDER BY m.MessageId DESC",
            new { take, roomId, beforeId }, MessageDto.Map);
        latest.Reverse();   // trả về theo thứ tự cũ -> mới để hiển thị
        return latest;
    }

    /// <summary>POST /api/rooms/{roomId}/messages - gửi tin văn bản (có emoji).</summary>
    [HttpPost("rooms/{roomId}/messages")]
    public ActionResult<MessageDto> Send(string roomId, SendMessageRequest req)
    {
        RequireMember(roomId);
        var content = Rules.Message(req.Content);
        var id = Convert.ToInt64(Db.Scalar(
            "INSERT INTO Messages (RoomId, Sender, Content) OUTPUT INSERTED.MessageId VALUES (@roomId, @me, @content)",
            new { roomId, me = Me, content }));
        return LoadMessage(id);
    }

    /// <summary>POST /api/rooms/{roomId}/files - gửi hình ảnh / tệp (form-data: "file", "content" tuỳ chọn).</summary>
    [HttpPost("rooms/{roomId}/files")]
    [RequestSizeLimit(MaxFileBytes)]
    public ActionResult<MessageDto> SendFile(string roomId, IFormFile? file, [FromForm] string? content)
    {
        RequireMember(roomId);
        var path = SaveUpload(file, "files", imagesOnly: false, maxBytes: MaxFileBytes);
        var fileName = Path.GetFileName(file!.FileName);
        if (fileName.Length > 255) fileName = fileName[^255..];
        var caption = string.IsNullOrWhiteSpace(content) ? null : Rules.Message(content);

        var id = Convert.ToInt64(Db.Scalar(
            "INSERT INTO Messages (RoomId, Sender, Content, FilePath, FileName) OUTPUT INSERTED.MessageId " +
            "VALUES (@roomId, @me, @caption, @path, @fileName)",
            new { roomId, me = Me, caption, path, fileName }));
        return LoadMessage(id);
    }

    /// <summary>DELETE /api/messages/{messageId} - xoá tin nhắn của mình (vẫn giữ dòng, hiện "đã bị xoá").</summary>
    [HttpDelete("messages/{messageId:long}")]
    public ActionResult<MessageDto> DeleteMessage(long messageId)
    {
        var sender = Db.Scalar("SELECT Sender FROM Messages WHERE MessageId = @messageId", new { messageId }) as string
                     ?? throw ApiError.NotFound("MESSAGE_NOT_FOUND", "Không tìm thấy tin nhắn.");
        if (!string.Equals(sender, Me, StringComparison.OrdinalIgnoreCase))
            throw ApiError.Forbidden("NOT_OWNER", "Chỉ xoá được tin nhắn của mình.");

        Db.Execute("UPDATE Messages SET IsDeleted = 1 WHERE MessageId = @messageId", new { messageId });
        return LoadMessage(messageId);
    }

    private MessageDto LoadMessage(long messageId) =>
        Db.QueryFirst($"SELECT {MessageDto.ColumnsFrom} WHERE m.MessageId = @messageId", new { messageId }, MessageDto.Map)!;
}
