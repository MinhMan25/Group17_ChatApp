using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebService.Data;
using WebService.Infrastructure;
using WebService.Models;

namespace WebService.Controllers;

/// <summary>Nhóm chức năng 1 - Quản lý tài khoản: đăng ký, đăng nhập, đăng xuất, mật khẩu, phiên đăng nhập.</summary>
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly EmailSender _email;
    private readonly IConfiguration _config;

    public AuthController(Db db, EmailSender email, IConfiguration config) : base(db)
    {
        _email = email;
        _config = config;
    }

    private record Account(string Username, string DisplayName, string Email, byte[] Salt, byte[] Hash);

    private Account? FindAccount(string where, object args) => Db.QueryFirst(
        "SELECT Username, DisplayName, Email, PasswordSalt, PasswordHash FROM Users WHERE " + where, args,
        r => new Account(r.Str("Username"), r.Str("DisplayName"), r.Str("Email"), (byte[])r["PasswordSalt"], (byte[])r["PasswordHash"]));

    /// <summary>POST /api/auth/register - tạo tài khoản, tự vào phòng GLOBAL.</summary>
    [HttpPost("register")]
    public ActionResult<ResultDto> Register(RegisterRequest req)
    {
        var username = Rules.Username(req.Username);
        var email = Rules.Email(req.Email);
        var password = Rules.Password(req.Password);
        var displayName = Rules.DisplayName(string.IsNullOrWhiteSpace(req.DisplayName) ? username : req.DisplayName);

        if (UserExists(username)) throw ApiError.Conflict("USERNAME_EXISTS", "Tên đăng nhập đã tồn tại.");
        if (Db.Exists("SELECT COUNT(*) FROM Users WHERE Email = @email", new { email }))
            throw ApiError.Conflict("EMAIL_EXISTS", "Email đã được sử dụng.");

        var (salt, hash) = PasswordHasher.Hash(password);
        try
        {
            Db.Execute(
                "INSERT INTO Users (Username, Email, DisplayName, PasswordSalt, PasswordHash) VALUES (@username, @email, @displayName, @salt, @hash)",
                new { username, email, displayName, salt, hash });
        }
        catch (SqlException ex) when (Db.IsDuplicateKey(ex))
        {
            throw ApiError.Conflict("USERNAME_EXISTS", "Tên đăng nhập hoặc email đã tồn tại.");
        }
        Db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES ('GLOBAL', @username)", new { username });

        return new ResultDto("REGISTERED", "Đăng ký thành công.");
    }

    /// <summary>POST /api/auth/login - trả về token để gửi kèm các yêu cầu sau.</summary>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest req)
    {
        var account = FindAccount("Username = @username", new { username = (req.Username ?? "").Trim() });
        if (account == null || !PasswordHasher.Verify(req.Password ?? "", account.Salt, account.Hash))
            throw ApiError.Unauthorized("INVALID_CREDENTIALS", "Sai tên đăng nhập hoặc mật khẩu.");

        var token = Guid.NewGuid().ToString("N");
        var days = _config.GetValue("SessionDays", 7);
        var device = string.IsNullOrWhiteSpace(req.Device) ? "ClientForm" : req.Device.Trim();
        if (device.Length > 100) device = device[..100];

        Db.Execute(
            "INSERT INTO Sessions (Token, Username, Device, ExpiresAt) VALUES (@token, @username, @device, DATEADD(DAY, @days, SYSDATETIME()))",
            new { token, username = account.Username, device, days });
        Db.Execute("UPDATE Users SET LastSeen = SYSDATETIME() WHERE Username = @username", new { username = account.Username });

        // Giống server TCP: đăng nhập là có mặt trong phòng GLOBAL
        if (!Db.Exists("SELECT COUNT(*) FROM RoomMembers WHERE RoomId = 'GLOBAL' AND Username = @username", new { username = account.Username }))
            Db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES ('GLOBAL', @username)", new { username = account.Username });

        return new LoginResponse(token, account.Username, account.DisplayName, account.Email, DateTime.Now.AddDays(days));
    }

    /// <summary>POST /api/auth/logout - xoá phiên hiện tại (thiết bị khác vẫn đăng nhập).</summary>
    [HttpPost("logout")]
    public ActionResult<ResultDto> Logout()
    {
        Db.Execute("DELETE FROM Sessions WHERE Token = @token", new { token = CurrentToken });
        return new ResultDto("LOGGED_OUT", "Đã đăng xuất.");
    }

    /// <summary>PUT /api/auth/password - đổi mật khẩu; các thiết bị khác bị đăng xuất.</summary>
    [HttpPut("password")]
    public ActionResult<ResultDto> ChangePassword(ChangePasswordRequest req)
    {
        var account = FindAccount("Username = @me", new { me = Me })!;
        if (!PasswordHasher.Verify(req.OldPassword ?? "", account.Salt, account.Hash))
            throw ApiError.BadRequest("WRONG_PASSWORD", "Mật khẩu cũ không đúng.");

        var (salt, hash) = PasswordHasher.Hash(Rules.Password(req.NewPassword));
        Db.Execute("UPDATE Users SET PasswordSalt = @salt, PasswordHash = @hash WHERE Username = @me", new { salt, hash, me = Me });
        Db.Execute("DELETE FROM Sessions WHERE Username = @me AND Token <> @token", new { me = Me, token = CurrentToken });
        return new ResultDto("PASSWORD_CHANGED", "Đã đổi mật khẩu.");
    }

    /// <summary>POST /api/auth/forgot-password - gửi mã 6 số qua email (hiệu lực 10 phút).</summary>
    [HttpPost("forgot-password")]
    public ActionResult<ResultDto> ForgotPassword(ForgotPasswordRequest req)
    {
        var email = (req.Email ?? "").Trim();
        var account = FindAccount("Email = @email", new { email });
        if (account != null)
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            Db.Execute("UPDATE Users SET ResetCode = @code, ResetCodeExpires = DATEADD(MINUTE, 10, SYSDATETIME()) WHERE Username = @username",
                new { code, username = account.Username });
            _email.Send(account.Email, "Mã đặt lại mật khẩu Zalu",
                $"Xin chào {account.DisplayName},\n\nMã đặt lại mật khẩu của bạn là: {code}\nMã có hiệu lực trong 10 phút.");
        }
        // Trả cùng một câu dù email có tồn tại hay không, để không lộ email nào đã đăng ký
        return new ResultDto("CODE_SENT", "Nếu email đã đăng ký, mã xác nhận đã được gửi.");
    }

    /// <summary>POST /api/auth/reset-password - đặt mật khẩu mới bằng mã trong email.</summary>
    [HttpPost("reset-password")]
    public ActionResult<ResultDto> ResetPassword(ResetPasswordRequest req)
    {
        var email = (req.Email ?? "").Trim();
        var code = (req.Code ?? "").Trim();
        var username = Db.Scalar(
            "SELECT Username FROM Users WHERE Email = @email AND ResetCode = @code AND ResetCodeExpires > SYSDATETIME()",
            new { email, code }) as string;
        if (username == null) throw ApiError.BadRequest("INVALID_CODE", "Mã không đúng hoặc đã hết hạn.");

        var (salt, hash) = PasswordHasher.Hash(Rules.Password(req.NewPassword));
        Db.Execute("UPDATE Users SET PasswordSalt = @salt, PasswordHash = @hash, ResetCode = NULL, ResetCodeExpires = NULL WHERE Username = @username",
            new { salt, hash, username });
        Db.Execute("DELETE FROM Sessions WHERE Username = @username", new { username });
        return new ResultDto("PASSWORD_RESET", "Đã đặt lại mật khẩu, hãy đăng nhập lại.");
    }

    /// <summary>GET /api/auth/sessions - các thiết bị đang đăng nhập (Cài đặt → Bảo mật).</summary>
    [HttpGet("sessions")]
    public ActionResult<List<SessionDto>> Sessions()
    {
        var current = CurrentToken;
        return Db.Query(
            "SELECT Token, Device, LoginAt, ExpiresAt FROM Sessions WHERE Username = @me AND ExpiresAt > SYSDATETIME() ORDER BY LoginAt DESC",
            new { me = Me },
            r => new SessionDto(r.Str("Token")[..6] + "…", r.StrOrNull("Device"), r.Date("LoginAt"), r.Date("ExpiresAt"), r.Str("Token") == current));
    }

    /// <summary>DELETE /api/auth/sessions/others - đăng xuất mọi thiết bị khác.</summary>
    [HttpDelete("sessions/others")]
    public ActionResult<ResultDto> LogoutOthers()
    {
        var count = Db.Execute("DELETE FROM Sessions WHERE Username = @me AND Token <> @token", new { me = Me, token = CurrentToken });
        return new ResultDto("SESSIONS_REMOVED", $"Đã đăng xuất {count} thiết bị khác.");
    }
}
