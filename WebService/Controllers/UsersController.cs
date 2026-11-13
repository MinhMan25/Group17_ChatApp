using Microsoft.AspNetCore.Mvc;
using WebService.Data;
using WebService.Infrastructure;
using WebService.Models;

namespace WebService.Controllers;

/// <summary>Thông tin cá nhân, ảnh đại diện, tìm kiếm người dùng, trạng thái Online (nhóm 5, 6).</summary>
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    public UsersController(Db db) : base(db) { }

    private UserDto GetUser(string username) =>
        Db.QueryFirst($"SELECT {Sql.UserColumns} FROM Users u WHERE u.Username = @username", new { username }, UserDto.Map)
        ?? throw ApiError.NotFound("USER_NOT_FOUND", "Không tìm thấy người dùng.");

    /// <summary>GET /api/users/me - thông tin tài khoản đang đăng nhập (màn hình Cài đặt → Tài khoản).</summary>
    [HttpGet("me")]
    public ActionResult<MeDto> GetMe()
    {
        var user = GetUser(Me);
        var info = Db.QueryFirst("SELECT Email, CreatedAt FROM Users WHERE Username = @me", new { me = Me },
            r => new { Email = r.Str("Email"), CreatedAt = r.Date("CreatedAt") })!;
        return new MeDto(user, info.Email, info.CreatedAt);
    }

    /// <summary>PUT /api/users/me - đổi tên hiển thị, thông tin giới thiệu.</summary>
    [HttpPut("me")]
    public ActionResult<UserDto> UpdateMe(UpdateProfileRequest req)
    {
        var displayName = Rules.DisplayName(req.DisplayName);
        var bio = string.IsNullOrWhiteSpace(req.Bio) ? null : req.Bio.Trim();
        if (bio != null && bio.Length > 200) throw ApiError.BadRequest("INVALID_BIO", "Thông tin giới thiệu tối đa 200 ký tự.");

        Db.Execute("UPDATE Users SET DisplayName = @displayName, Bio = @bio WHERE Username = @me", new { displayName, bio, me = Me });
        return GetUser(Me);
    }

    /// <summary>PUT /api/users/me/email - đổi email (cần nhập mật khẩu).</summary>
    [HttpPut("me/email")]
    public ActionResult<ResultDto> ChangeEmail(ChangeEmailRequest req)
    {
        var email = Rules.Email(req.NewEmail);
        var row = Db.QueryFirst("SELECT PasswordSalt, PasswordHash FROM Users WHERE Username = @me", new { me = Me },
            r => new { Salt = (byte[])r["PasswordSalt"], Hash = (byte[])r["PasswordHash"] })!;
        if (!PasswordHasher.Verify(req.Password ?? "", row.Salt, row.Hash))
            throw ApiError.BadRequest("WRONG_PASSWORD", "Mật khẩu không đúng.");
        if (Db.Exists("SELECT COUNT(*) FROM Users WHERE Email = @email AND Username <> @me", new { email, me = Me }))
            throw ApiError.Conflict("EMAIL_EXISTS", "Email đã được sử dụng.");

        Db.Execute("UPDATE Users SET Email = @email WHERE Username = @me", new { email, me = Me });
        return new ResultDto("EMAIL_CHANGED", "Đã đổi email.");
    }

    /// <summary>POST /api/users/me/avatar - đổi ảnh đại diện (form-data, trường "file").</summary>
    [HttpPost("me/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public ActionResult<UserDto> UploadAvatar(IFormFile? file)
    {
        var path = SaveUpload(file, "avatars", imagesOnly: true, maxBytes: 5 * 1024 * 1024);
        Db.Execute("UPDATE Users SET AvatarPath = @path WHERE Username = @me", new { path, me = Me });
        return GetUser(Me);
    }

    /// <summary>
    /// POST /api/users/me/heartbeat - client gọi khoảng 30 giây/lần để giữ trạng thái Online.
    /// (TokenMiddleware đã cập nhật LastSeen nên ở đây không cần làm gì thêm.)
    /// </summary>
    [HttpPost("me/heartbeat")]
    public IActionResult Heartbeat() => NoContent();

    /// <summary>GET /api/users/search?q=... - tìm theo tên đăng nhập / tên hiển thị, kèm quan hệ bạn bè.</summary>
    [HttpGet("search")]
    public ActionResult<List<SearchResultDto>> Search(string? q)
    {
        var keyword = (q ?? "").Trim().Replace("%", "").Replace("_", "").Replace("[", "");
        if (keyword.Length == 0) return new List<SearchResultDto>();

        return Db.Query(
            $"SELECT TOP 20 {Sql.UserColumns}, " +
            "CASE WHEN EXISTS (SELECT 1 FROM Friendships f WHERE f.Status = 'ACCEPTED' AND " +
            "       ((f.FromUser = @me AND f.ToUser = u.Username) OR (f.FromUser = u.Username AND f.ToUser = @me))) THEN 'FRIEND' " +
            "     WHEN EXISTS (SELECT 1 FROM Friendships f WHERE f.Status = 'PENDING' AND f.FromUser = @me AND f.ToUser = u.Username) THEN 'SENT' " +
            "     WHEN EXISTS (SELECT 1 FROM Friendships f WHERE f.Status = 'PENDING' AND f.FromUser = u.Username AND f.ToUser = @me) THEN 'RECEIVED' " +
            "     ELSE 'NONE' END AS Relation " +
            "FROM Users u WHERE u.Username <> @me AND (u.Username LIKE @pattern OR u.DisplayName LIKE @pattern) " +
            "ORDER BY u.DisplayName",
            new { me = Me, pattern = "%" + keyword + "%" },
            r => new SearchResultDto(UserDto.Map(r), r.Str("Relation")));
    }

    /// <summary>GET /api/users/{username} - xem hồ sơ một người.</summary>
    [HttpGet("{username}")]
    public ActionResult<UserDto> Get(string username) => GetUser(username);
}
