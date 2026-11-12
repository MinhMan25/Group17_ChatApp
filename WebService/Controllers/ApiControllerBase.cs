using Microsoft.AspNetCore.Mvc;
using WebService.Data;
using WebService.Infrastructure;

namespace WebService.Controllers;

/// <summary>Lớp cha của các controller: lấy người đang đăng nhập, kiểm tra quyền phòng chat, lưu tệp.</summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };
    private static readonly string[] BlockedExtensions = { ".exe", ".bat", ".cmd", ".msi", ".ps1", ".vbs", ".scr", ".sh" };

    protected Db Db { get; }

    protected ApiControllerBase(Db db) => Db = db;

    /// <summary>Username của người đang gọi API (TokenMiddleware đã kiểm tra token).</summary>
    protected string Me => HttpContext.Items["Username"] as string
                           ?? throw ApiError.Unauthorized("NOT_AUTHENTICATED", "Chưa đăng nhập.");

    protected string CurrentToken => HttpContext.Items["Token"] as string ?? "";

    protected record Membership(string RoomType, bool IsAdmin);

    /// <summary>Người dùng phải đang ở trong phòng; trả về loại phòng và quyền admin.</summary>
    protected Membership RequireMember(string roomId)
    {
        var membership = Db.QueryFirst(
            "SELECT r.RoomType, m.IsAdmin FROM RoomMembers m JOIN ChatRooms r ON r.RoomId = m.RoomId " +
            "WHERE m.RoomId = @roomId AND m.Username = @me",
            new { roomId, me = Me },
            r => new Membership(r.Str("RoomType"), r.Bool("IsAdmin")));
        if (membership != null) return membership;

        if (Db.Exists("SELECT COUNT(*) FROM ChatRooms WHERE RoomId = @roomId", new { roomId }))
            throw ApiError.Forbidden("NOT_MEMBER", "Bạn không ở trong phòng chat này.");
        throw ApiError.NotFound("ROOM_NOT_FOUND", "Không tìm thấy phòng chat.");
    }

    /// <summary>Chỉ admin của nhóm (RoomType = Group) mới được làm.</summary>
    protected void RequireGroupAdmin(string roomId)
    {
        var membership = RequireMember(roomId);
        if (membership.RoomType != "Group") throw ApiError.BadRequest("NOT_GROUP", "Chức năng này chỉ dành cho nhóm chat.");
        if (!membership.IsAdmin) throw ApiError.Forbidden("NOT_ADMIN", "Chỉ trưởng nhóm mới làm được.");
    }

    protected bool AreFriends(string a, string b) => Db.Exists(
        "SELECT COUNT(*) FROM Friendships WHERE Status = 'ACCEPTED' AND " +
        "((FromUser = @a AND ToUser = @b) OR (FromUser = @b AND ToUser = @a))", new { a, b });

    protected bool UserExists(string username) =>
        Db.Exists("SELECT COUNT(*) FROM Users WHERE Username = @username", new { username });

    /// <summary>Lưu tệp vào wwwroot/uploads/{folder}/, trả về đường dẫn dạng /uploads/files/abc.png.</summary>
    protected string SaveUpload(IFormFile? file, string folder, bool imagesOnly, long maxBytes)
    {
        if (file == null || file.Length == 0) throw ApiError.BadRequest("NO_FILE", "Chưa chọn tệp.");
        if (file.Length > maxBytes) throw ApiError.BadRequest("FILE_TOO_LARGE", $"Tệp tối đa {maxBytes / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (imagesOnly && !ImageExtensions.Contains(ext)) throw ApiError.BadRequest("NOT_IMAGE", "Chỉ nhận ảnh PNG, JPG, GIF, BMP, WEBP.");
        if (BlockedExtensions.Contains(ext)) throw ApiError.BadRequest("FILE_NOT_ALLOWED", "Loại tệp này không được gửi.");

        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var directory = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(directory);

        var name = Guid.NewGuid().ToString("N") + ext;   // tên ngẫu nhiên, không dùng tên client gửi
        using (var stream = System.IO.File.Create(Path.Combine(directory, name)))
            file.CopyTo(stream);
        return $"/uploads/{folder}/{name}";
    }
}
