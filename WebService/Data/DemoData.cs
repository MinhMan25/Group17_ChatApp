using WebService.Infrastructure;

namespace WebService.Data;

/// <summary>
/// Lần chạy đầu: tạo 3 tài khoản mẫu alice / bob / carol (mật khẩu 12345678),
/// cho vào phòng GLOBAL, kết bạn với nhau và tạo sẵn một nhóm chat.
/// Tắt bằng "SeedDemoData": false trong appsettings.json.
/// </summary>
public static class DemoData
{
    public static void Seed(Db db, IConfiguration config, ILogger logger)
    {
        // Câu lệnh đầu tiên cũng là bước kiểm tra kết nối SQL Server
        var userCount = Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM Users"));
        logger.LogInformation("Đã kết nối SQL Server, có {Count} tài khoản.", userCount);

        if (!db.Exists("SELECT COUNT(*) FROM ChatRooms WHERE RoomId = 'GLOBAL'"))
            db.Execute("INSERT INTO ChatRooms (RoomId, RoomName, RoomType) VALUES ('GLOBAL', N'Nhóm chat tổng', 'Global')");

        if (!config.GetValue("SeedDemoData", true) || userCount > 0) return;

        var users = new[] { ("alice", "Alice Nguyễn"), ("bob", "Bob Trần"), ("carol", "Carol Lê") };
        foreach (var (username, displayName) in users)
        {
            var (salt, hash) = PasswordHasher.Hash("12345678");
            db.Execute(
                "INSERT INTO Users (Username, Email, DisplayName, PasswordSalt, PasswordHash) VALUES (@username, @email, @displayName, @salt, @hash)",
                new { username, email = username + "@demo.local", displayName, salt, hash });
            db.Execute("INSERT INTO RoomMembers (RoomId, Username) VALUES ('GLOBAL', @username)", new { username });
        }

        db.Execute("INSERT INTO Friendships (FromUser, ToUser, Status) VALUES ('alice', 'bob', 'ACCEPTED'), ('alice', 'carol', 'ACCEPTED'), ('bob', 'carol', 'ACCEPTED')");

        db.Execute("INSERT INTO ChatRooms (RoomId, RoomName, RoomType, CreatedBy) VALUES ('GROUP_DEMO', N'Nhóm đồ án NT106', 'Group', 'alice')");
        db.Execute("INSERT INTO RoomMembers (RoomId, Username, IsAdmin) VALUES ('GROUP_DEMO', 'alice', 1), ('GROUP_DEMO', 'bob', 0), ('GROUP_DEMO', 'carol', 0)");
        db.Execute("INSERT INTO Messages (RoomId, Sender, Content) VALUES ('GROUP_DEMO', 'alice', N'Chào cả nhóm 👋'), ('GLOBAL', 'bob', N'Xin chào mọi người!')");

        logger.LogInformation("Đã tạo tài khoản mẫu: alice / bob / carol, mật khẩu 12345678");
    }
}
