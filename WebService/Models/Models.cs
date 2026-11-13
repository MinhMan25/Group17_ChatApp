using WebService.Data;
using Microsoft.Data.SqlClient;

namespace WebService.Models;

// ===================== Dữ liệu client gửi lên (JSON) =====================

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    /// <summary>Bỏ trống thì lấy Username (giống server TCP hiện tại).</summary>
    public string? DisplayName { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Device { get; set; }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = "";
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class UpdateProfileRequest
{
    public string DisplayName { get; set; } = "";
    public string? Bio { get; set; }
}

public class ChangeEmailRequest
{
    public string NewEmail { get; set; } = "";
    public string Password { get; set; } = "";
}

public class CreateGroupRequest
{
    public string RoomName { get; set; } = "";
    public List<string> Members { get; set; } = new();
}

public class RenameRoomRequest
{
    public string RoomName { get; set; } = "";
}

public class AddMemberRequest
{
    public string Username { get; set; } = "";
}

public class SendMessageRequest
{
    public string Content { get; set; } = "";
}

// ===================== Dữ liệu trả về (JSON) =====================

public record UserDto(string Username, string DisplayName, string? Bio, string? AvatarPath, bool IsOnline, DateTime? LastSeen)
{
    /// <summary>Đọc từ câu SELECT có Sql.UserColumns.</summary>
    public static UserDto Map(SqlDataReader r) => new(
        r.Str("Username"), r.Str("DisplayName"), r.StrOrNull("Bio"), r.StrOrNull("AvatarPath"),
        r.Bool("IsOnline"), r.DateOrNull("LastSeen"));
}

public record LoginResponse(string Token, string Username, string DisplayName, string Email, DateTime ExpiresAt);

public record SessionDto(string TokenPreview, string? Device, DateTime LoginAt, DateTime ExpiresAt, bool IsCurrent);

public record SearchResultDto(UserDto User, string Relation);   // FRIEND | SENT | RECEIVED | NONE

public record MeDto(UserDto User, string Email, DateTime CreatedAt);

public record FriendRequestsDto(List<UserDto> Received, List<UserDto> Sent);

public record RoomDto(string RoomId, string RoomName, string RoomType, string? AvatarPath, int MemberCount,
    string? LastMessage, string? LastSender, DateTime? LastMessageAt, bool? OtherIsOnline);

public record MemberDto(string Username, string DisplayName, string? AvatarPath, bool IsAdmin, bool IsOnline);

public record RoomDetailDto(string RoomId, string RoomName, string RoomType, string? AvatarPath, bool IAmAdmin, List<MemberDto> Members);

public record MessageDto(long MessageId, string RoomId, string Sender, string SenderName, string? Content,
    string? FilePath, string? FileName, bool IsDeleted, DateTime SentAt)
{
    /// <summary>Phần sau SELECT (để chèn TOP (@take) phía trước).</summary>
    public const string ColumnsFrom =
        "m.MessageId, m.RoomId, m.Sender, u.DisplayName AS SenderName, m.Content, m.FilePath, m.FileName, m.IsDeleted, m.SentAt " +
        "FROM Messages m JOIN Users u ON u.Username = m.Sender ";

    public static MessageDto Map(SqlDataReader r)
    {
        var deleted = r.Bool("IsDeleted");
        return new MessageDto(r.Long("MessageId"), r.Str("RoomId"), r.Str("Sender"), r.Str("SenderName"),
            deleted ? null : r.StrOrNull("Content"),
            deleted ? null : r.StrOrNull("FilePath"),
            deleted ? null : r.StrOrNull("FileName"),
            deleted, r.Date("SentAt"));
    }
}

/// <summary>Kết quả đơn giản: {"code":"REGISTERED","message":"..."}.</summary>
public record ResultDto(string Code, string Message);
