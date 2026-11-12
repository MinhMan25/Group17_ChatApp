using WebService.Data;
using WebService.Infrastructure;

// =====================================================================
//  ZaluChat Web API (Nhóm 17)
//  WinForms (ClientForm)  ──HTTP + JSON──▶  WebService  ──ADO.NET──▶  SQL Server
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

// Tự kiểm tra dữ liệu trong controller để trả mã lỗi giống AccountService (INVALID_USERNAME...)
builder.Services.AddControllers(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<EmailSender>();

var app = builder.Build();

// Thư mục lưu ảnh đại diện và tệp gửi trong chat: wwwroot/uploads
var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));

// Kiểm tra kết nối SQL Server và tạo tài khoản mẫu
try
{
    DemoData.Seed(app.Services.GetRequiredService<Db>(), app.Configuration, app.Logger);
}
catch (Exception ex)
{
    app.Logger.LogError("Không kết nối được SQL Server: {Message}", ex.Message);
    app.Logger.LogError("Kiểm tra: đã chạy Database/ZaluChat.sql chưa, và chuỗi ConnectionStrings:ZaluChat trong appsettings.json.");
    return;
}

app.UseMiddleware<ErrorMiddleware>();   // đổi lỗi thành JSON {code, message}
app.UseStaticFiles();                   // phục vụ /uploads/... (ảnh, tệp)
app.UseMiddleware<TokenMiddleware>();   // kiểm tra "Authorization: Bearer <token>"
app.MapControllers();
app.MapGet("/", () => "ZaluChat Web API đang chạy. Thử bằng Postman: POST /api/auth/login");

app.Run();
