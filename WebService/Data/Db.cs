using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace WebService.Data;

/// <summary>
/// Lớp hỗ trợ ADO.NET: mỗi lần gọi mở một SqlConnection, chạy SqlCommand rồi đóng.
/// Tham số truyền bằng đối tượng ẩn danh: new { username = "alice" } tương ứng @username trong câu SQL.
/// Luôn dùng tham số, không cộng chuỗi, để tránh SQL Injection.
/// </summary>
public class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ZaluChat")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:ZaluChat trong appsettings.json");
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string sql, object? args)
    {
        var command = new SqlCommand(sql, connection);
        if (args != null)
        {
            foreach (var property in args.GetType().GetProperties())
                command.Parameters.AddWithValue("@" + property.Name, property.GetValue(args) ?? DBNull.Value);
        }
        return command;
    }

    /// <summary>SELECT nhiều dòng; map đổi mỗi dòng thành một đối tượng.</summary>
    public List<T> Query<T>(string sql, object? args, Func<SqlDataReader, T> map)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = CreateCommand(connection, sql, args);
        using var reader = command.ExecuteReader();
        var list = new List<T>();
        while (reader.Read()) list.Add(map(reader));
        return list;
    }

    /// <summary>SELECT một dòng, không có thì trả null.</summary>
    public T? QueryFirst<T>(string sql, object? args, Func<SqlDataReader, T> map) where T : class =>
        Query(sql, args, map).FirstOrDefault();

    /// <summary>INSERT / UPDATE / DELETE, trả về số dòng bị ảnh hưởng.</summary>
    public int Execute(string sql, object? args = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = CreateCommand(connection, sql, args);
        return command.ExecuteNonQuery();
    }

    /// <summary>Lấy một giá trị (COUNT(*), SCOPE_IDENTITY()...).</summary>
    public object? Scalar(string sql, object? args = null)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = CreateCommand(connection, sql, args);
        var value = command.ExecuteScalar();
        return value is DBNull ? null : value;
    }

    public bool Exists(string sql, object? args = null) => Convert.ToInt32(Scalar(sql, args) ?? 0) > 0;

    /// <summary>Lỗi trùng khoá chính / UNIQUE (2627, 2601).</summary>
    public static bool IsDuplicateKey(SqlException ex) => ex.Number == 2627 || ex.Number == 2601;
}

/// <summary>Đọc cột theo tên, tự xử lý NULL.</summary>
public static class ReaderExtensions
{
    public static string Str(this DbDataReader r, string column) => r[column] is DBNull ? "" : (string)r[column];
    public static string? StrOrNull(this DbDataReader r, string column) => r[column] is DBNull ? null : (string)r[column];
    public static long Long(this DbDataReader r, string column) => Convert.ToInt64(r[column]);
    public static int Int(this DbDataReader r, string column) => Convert.ToInt32(r[column]);
    public static bool Bool(this DbDataReader r, string column) => r[column] is not DBNull && Convert.ToBoolean(r[column]);
    public static DateTime Date(this DbDataReader r, string column) => (DateTime)r[column];
    public static DateTime? DateOrNull(this DbDataReader r, string column) => r[column] is DBNull ? null : (DateTime)r[column];
}

/// <summary>Đoạn SQL dùng chung.</summary>
public static class Sql
{
    /// <summary>
    /// Online = còn phiên đăng nhập VÀ đã gọi API trong 60 giây gần nhất
    /// (client gọi POST /api/users/me/heartbeat khoảng 30 giây một lần).
    /// Dùng với bảng Users đặt bí danh u.
    /// </summary>
    public const string IsOnline =
        "CAST(CASE WHEN u.LastSeen > DATEADD(SECOND, -60, SYSDATETIME()) " +
        "AND EXISTS (SELECT 1 FROM Sessions s WHERE s.Username = u.Username AND s.ExpiresAt > SYSDATETIME()) " +
        "THEN 1 ELSE 0 END AS BIT) AS IsOnline";

    public const string UserColumns = "u.Username, u.DisplayName, u.Bio, u.AvatarPath, u.LastSeen, " + IsOnline;
}
