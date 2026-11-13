using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using WebService.Data;

namespace WebService.Infrastructure;

/// <summary>
/// Lỗi nghiệp vụ. Trả về client dạng JSON: {"code":"USERNAME_EXISTS","message":"Tên đăng nhập đã tồn tại"}.
/// Mã lỗi giữ giống AccountService của server TCP (INVALID_USERNAME, INVALID_CREDENTIALS...).
/// </summary>
public class ApiError : Exception
{
    public int Status { get; }
    public string Code { get; }

    public ApiError(int status, string code, string message) : base(message)
    {
        Status = status;
        Code = code;
    }

    public static ApiError BadRequest(string code, string message) => new(400, code, message);
    public static ApiError Unauthorized(string code, string message) => new(401, code, message);
    public static ApiError Forbidden(string code, string message) => new(403, code, message);
    public static ApiError NotFound(string code, string message) => new(404, code, message);
    public static ApiError Conflict(string code, string message) => new(409, code, message);
}

/// <summary>Bắt mọi lỗi, trả JSON {code, message} với mã HTTP tương ứng.</summary>
public class ErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorMiddleware> _logger;

    public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiError ex) when (!context.Response.HasStarted)
        {
            context.Response.StatusCode = ex.Status;
            await context.Response.WriteAsJsonAsync(new { code = ex.Code, message = ex.Message });
        }
        catch (SqlException ex) when (!context.Response.HasStarted)
        {
            _logger.LogError(ex, "Lỗi SQL Server");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { code = "DATABASE_ERROR", message = "Lỗi cơ sở dữ liệu." });
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            _logger.LogError(ex, "Lỗi không xử lý được");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { code = "SERVER_ERROR", message = "Lỗi máy chủ." });
        }
    }
}

/// <summary>
/// Kiểm tra đăng nhập cho mọi đường dẫn /api/... (trừ đăng ký, đăng nhập, quên mật khẩu).
/// Client gửi header: Authorization: Bearer &lt;token nhận được khi đăng nhập&gt;
/// Hợp lệ thì lưu Username vào HttpContext.Items và cập nhật LastSeen (dùng cho Online/Offline).
/// </summary>
public class TokenMiddleware
{
    private static readonly string[] PublicPaths =
    {
        "/api/auth/register", "/api/auth/login", "/api/auth/forgot-password", "/api/auth/reset-password"
    };

    private readonly RequestDelegate _next;

    public TokenMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, Db db)
    {
        var path = context.Request.Path.Value ?? "";
        var isApi = path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
        var isPublic = PublicPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase));
        if (!isApi || isPublic)
        {
            await _next(context);
            return;
        }

        var header = context.Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : "";
        var username = token.Length == 0
            ? null
            : db.Scalar("SELECT Username FROM Sessions WHERE Token = @token AND ExpiresAt > SYSDATETIME()", new { token }) as string;

        if (username == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { code = "NOT_AUTHENTICATED", message = "Chưa đăng nhập hoặc phiên đã hết hạn." });
            return;
        }

        db.Execute("UPDATE Users SET LastSeen = SYSDATETIME() WHERE Username = @username", new { username });
        context.Items["Username"] = username;
        context.Items["Token"] = token;
        await _next(context);
    }
}

/// <summary>
/// Băm mật khẩu GIỐNG HỆT AccountService của server TCP:
/// salt ngẫu nhiên 16 byte, PBKDF2 100.000 vòng (HMAC-SHA1 - mặc định của Rfc2898DeriveBytes), hash 32 byte.
/// </summary>
public static class PasswordHasher
{
    private const int Iterations = 100_000;

    public static (byte[] Salt, byte[] Hash) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return (salt, Derive(password, salt));
    }

    public static bool Verify(string password, byte[] salt, byte[] expectedHash) =>
        // So sánh thời gian cố định (không dừng ở byte khác đầu tiên), giống vòng lặp XOR trong AccountService
        CryptographicOperations.FixedTimeEquals(Derive(password, salt), expectedHash);

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA1, 32);
}

/// <summary>Quy tắc kiểm tra dữ liệu, chép từ AccountService.Register.</summary>
public static class Rules
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private static bool HasBadChars(string s) => s.Contains('|') || s.Contains('\r') || s.Contains('\n');

    public static string Username(string? username)
    {
        var u = (username ?? "").Trim();
        if (u.Length < 3 || u.Length > 32 || !u.All(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '.'))
            throw ApiError.BadRequest("INVALID_USERNAME", "Tên đăng nhập 3-32 ký tự, chỉ gồm chữ không dấu, số, dấu _ và dấu chấm.");
        return u;
    }

    public static string Email(string? email)
    {
        var e = (email ?? "").Trim();
        if (e.Length == 0 || e.Length > 254 || HasBadChars(e) || !EmailPattern.IsMatch(e))
            throw ApiError.BadRequest("INVALID_EMAIL", "Email không hợp lệ.");
        return e;
    }

    public static string Password(string? password)
    {
        var p = password ?? "";
        if (p.Length < 8 || p.Length > 128 || HasBadChars(p))
            throw ApiError.BadRequest("INVALID_PASSWORD", "Mật khẩu 8-128 ký tự.");
        return p;
    }

    public static string DisplayName(string? displayName)
    {
        var d = (displayName ?? "").Trim();
        if (d.Length == 0 || d.Length > 32 || HasBadChars(d))
            throw ApiError.BadRequest("INVALID_DISPLAY_NAME", "Tên hiển thị 1-32 ký tự.");
        return d;
    }

    public static string Message(string? content)
    {
        var c = (content ?? "").Trim();
        if (c.Length == 0 || c.Length > 2000)
            throw ApiError.BadRequest("INVALID_MESSAGE", "Tin nhắn 1-2000 ký tự.");
        return c;
    }

    public static string RoomName(string? name)
    {
        var n = (name ?? "").Trim();
        if (n.Length == 0 || n.Length > 100)
            throw ApiError.BadRequest("INVALID_ROOM_NAME", "Tên nhóm 1-100 ký tự.");
        return n;
    }
}

/// <summary>
/// Gửi email bằng SmtpClient (môn học chương Email).
/// Chưa điền Smtp trong appsettings.json thì in nội dung ra cửa sổ console để thử.
/// </summary>
public class EmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Send(string to, string subject, string body)
    {
        var host = _config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("[SMTP chưa cấu hình] Gửi tới {To}: {Subject}\n{Body}", to, subject, body);
            return;
        }

        using var client = new SmtpClient(host, int.Parse(_config["Smtp:Port"] ?? "587"))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"])
        };
        client.Send(new MailMessage(_config["Smtp:From"]!, to, subject, body));
    }
}
