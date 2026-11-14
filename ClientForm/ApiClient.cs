using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatappZalu
{
    // =====================================================================
    //  Gọi WebService (ASP.NET Core Web API) từ WinForms bằng HttpClient.
    //  Dùng chung một đối tượng qua ApiClientManager.Api (giống ChatClientManager.Client).
    //
    //  Ví dụ trong Login.cs:
    //      try
    //      {
    //          var me = await ApiClientManager.Api.LoginAsync(username, password);
    //          // đăng nhập thành công: me.DisplayName ...
    //      }
    //      catch (ApiException ex) when (ex.Code == "INVALID_CREDENTIALS")
    //      {
    //          MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.");
    //      }
    // =====================================================================

    /// <summary>Lỗi server trả về: Code giống mã lỗi cũ (USERNAME_EXISTS, INVALID_PASSWORD...).</summary>
    public class ApiException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public ApiException(int statusCode, string code, string message) : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }

    // ---------- Dữ liệu nhận về (trùng tên trường với WebService/Models) ----------
    public record UserDto(string Username, string DisplayName, string? Bio, string? AvatarPath, bool IsOnline, DateTime? LastSeen);
    public record LoginResponse(string Token, string Username, string DisplayName, string Email, DateTime ExpiresAt);
    public record ResultDto(string Code, string Message);
    public record MeDto(UserDto User, string Email, DateTime CreatedAt);
    public record SearchResultDto(UserDto User, string Relation);
    public record FriendRequestsDto(List<UserDto> Received, List<UserDto> Sent);
    public record SessionDto(string TokenPreview, string? Device, DateTime LoginAt, DateTime ExpiresAt, bool IsCurrent);
    public record RoomDto(string RoomId, string RoomName, string RoomType, string? AvatarPath, int MemberCount,
        string? LastMessage, string? LastSender, DateTime? LastMessageAt, bool? OtherIsOnline);
    public record MemberDto(string Username, string DisplayName, string? AvatarPath, bool IsAdmin, bool IsOnline);
    public record RoomDetailDto(string RoomId, string RoomName, string RoomType, string? AvatarPath, bool IAmAdmin, List<MemberDto> Members);
    public record MessageDto(long MessageId, string RoomId, string Sender, string SenderName, string? Content,
        string? FilePath, string? FileName, bool IsDeleted, DateTime SentAt);

    public class ApiClient : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient http;

        /// <summary>Địa chỉ WebService, ví dụ http://127.0.0.1:5000 (đổi thành IP máy chạy WebService khi demo LAN).</summary>
        public string BaseUrl { get; }
        public string? Token { get; private set; }
        public string? Username { get; private set; }
        public string? DisplayName { get; private set; }

        public ApiClient(string baseUrl = "http://127.0.0.1:5000")
        {
            BaseUrl = baseUrl.TrimEnd('/');
            http = new HttpClient { BaseAddress = new Uri(BaseUrl + "/"), Timeout = TimeSpan.FromSeconds(15) };
        }

        /// <summary>Đường dẫn đầy đủ của ảnh/tệp (AvatarPath, FilePath là đường dẫn tương đối /uploads/...).</summary>
        public string FileUrl(string relativePath) => BaseUrl + relativePath;

        public async Task<Image> DownloadImageAsync(string relativePath)
        {
            byte[] bytes = await http.GetByteArrayAsync(FileUrl(relativePath));
            using var stream = new MemoryStream(bytes);
            using var source = Image.FromStream(stream);
            return new Bitmap(source);
        }

        // ================================================================ tài khoản

        public Task<ResultDto> RegisterAsync(string username, string email, string password, string? displayName = null) =>
            SendAsync<ResultDto>(HttpMethod.Post, "api/auth/register", new { username, email, password, displayName });

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var result = await SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login",
                new { username, password, device = Environment.MachineName });
            Token = result.Token;
            Username = result.Username;
            DisplayName = result.DisplayName;
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            return result;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await SendAsync<ResultDto>(HttpMethod.Post, "api/auth/logout");
            }
            finally
            {
                Token = null;
                http.DefaultRequestHeaders.Authorization = null;
            }
        }

        public Task<ResultDto> ChangePasswordAsync(string oldPassword, string newPassword) =>
            SendAsync<ResultDto>(HttpMethod.Put, "api/auth/password", new { oldPassword, newPassword });

        public Task<ResultDto> ForgotPasswordAsync(string email) =>
            SendAsync<ResultDto>(HttpMethod.Post, "api/auth/forgot-password", new { email });

        public Task<ResultDto> ResetPasswordAsync(string email, string code, string newPassword) =>
            SendAsync<ResultDto>(HttpMethod.Post, "api/auth/reset-password", new { email, code, newPassword });

        public Task<List<SessionDto>> GetSessionsAsync() => SendAsync<List<SessionDto>>(HttpMethod.Get, "api/auth/sessions");

        public Task<ResultDto> LogoutOtherDevicesAsync() => SendAsync<ResultDto>(HttpMethod.Delete, "api/auth/sessions/others");

        // ================================================================ cá nhân

        public Task<MeDto> GetMeAsync() => SendAsync<MeDto>(HttpMethod.Get, "api/users/me");

        public Task<UserDto> UpdateProfileAsync(string displayName, string? bio) =>
            SendAsync<UserDto>(HttpMethod.Put, "api/users/me", new { displayName, bio });

        public Task<ResultDto> ChangeEmailAsync(string newEmail, string password) =>
            SendAsync<ResultDto>(HttpMethod.Put, "api/users/me/email", new { newEmail, password });

        public Task<UserDto> UploadAvatarAsync(string imagePath) => UploadAsync<UserDto>("api/users/me/avatar", imagePath, null);

        /// <summary>Gọi khoảng 30 giây/lần (Timer) để giữ trạng thái Online.</summary>
        public Task HeartbeatAsync() => SendAsync<object?>(HttpMethod.Post, "api/users/me/heartbeat");

        public Task<List<SearchResultDto>> SearchUsersAsync(string keyword) =>
            SendAsync<List<SearchResultDto>>(HttpMethod.Get, "api/users/search?q=" + Uri.EscapeDataString(keyword));

        // ================================================================ bạn bè

        public Task<List<UserDto>> GetFriendsAsync() => SendAsync<List<UserDto>>(HttpMethod.Get, "api/friends");

        public Task<FriendRequestsDto> GetFriendRequestsAsync() => SendAsync<FriendRequestsDto>(HttpMethod.Get, "api/friends/requests");

        public Task<ResultDto> SendFriendRequestAsync(string username) =>
            SendAsync<ResultDto>(HttpMethod.Post, "api/friends/requests/" + Uri.EscapeDataString(username));

        public Task<ResultDto> AcceptFriendAsync(string username) =>
            SendAsync<ResultDto>(HttpMethod.Post, $"api/friends/requests/{Uri.EscapeDataString(username)}/accept");

        /// <summary>Từ chối lời mời nhận được, hoặc huỷ lời mời mình đã gửi.</summary>
        public Task<ResultDto> RejectFriendAsync(string username) =>
            SendAsync<ResultDto>(HttpMethod.Delete, "api/friends/requests/" + Uri.EscapeDataString(username));

        public Task<ResultDto> RemoveFriendAsync(string username) =>
            SendAsync<ResultDto>(HttpMethod.Delete, "api/friends/" + Uri.EscapeDataString(username));

        // ================================================================ phòng chat

        public Task<List<RoomDto>> GetRoomsAsync() => SendAsync<List<RoomDto>>(HttpMethod.Get, "api/rooms");

        public Task<RoomDetailDto> GetRoomAsync(string roomId) => SendAsync<RoomDetailDto>(HttpMethod.Get, "api/rooms/" + roomId);

        public Task<RoomDetailDto> OpenPrivateRoomAsync(string username) =>
            SendAsync<RoomDetailDto>(HttpMethod.Post, "api/rooms/private/" + Uri.EscapeDataString(username));

        public Task<RoomDetailDto> CreateGroupAsync(string roomName, List<string> members) =>
            SendAsync<RoomDetailDto>(HttpMethod.Post, "api/rooms/group", new { roomName, members });

        public Task<RoomDetailDto> RenameGroupAsync(string roomId, string roomName) =>
            SendAsync<RoomDetailDto>(HttpMethod.Put, $"api/rooms/{roomId}/name", new { roomName });

        public Task<RoomDetailDto> AddMemberAsync(string roomId, string username) =>
            SendAsync<RoomDetailDto>(HttpMethod.Post, $"api/rooms/{roomId}/members", new { username });

        public Task<RoomDetailDto> RemoveMemberAsync(string roomId, string username) =>
            SendAsync<RoomDetailDto>(HttpMethod.Delete, $"api/rooms/{roomId}/members/{Uri.EscapeDataString(username)}");

        public Task<ResultDto> LeaveGroupAsync(string roomId) => SendAsync<ResultDto>(HttpMethod.Post, $"api/rooms/{roomId}/leave");

        // ================================================================ tin nhắn

        /// <summary>50 tin gần nhất (mở phòng).</summary>
        public Task<List<MessageDto>> GetLatestMessagesAsync(string roomId, int take = 50) =>
            SendAsync<List<MessageDto>>(HttpMethod.Get, $"api/rooms/{roomId}/messages?take={take}");

        /// <summary>Tin mới hơn afterId - gọi định kỳ bằng Timer để nhận tin (polling).</summary>
        public Task<List<MessageDto>> GetNewMessagesAsync(string roomId, long afterId) =>
            SendAsync<List<MessageDto>>(HttpMethod.Get, $"api/rooms/{roomId}/messages?afterId={afterId}");

        /// <summary>Tin cũ hơn beforeId - khi cuộn lên xem lịch sử.</summary>
        public Task<List<MessageDto>> GetOlderMessagesAsync(string roomId, long beforeId, int take = 50) =>
            SendAsync<List<MessageDto>>(HttpMethod.Get, $"api/rooms/{roomId}/messages?beforeId={beforeId}&take={take}");

        public Task<MessageDto> SendMessageAsync(string roomId, string content) =>
            SendAsync<MessageDto>(HttpMethod.Post, $"api/rooms/{roomId}/messages", new { content });

        public Task<MessageDto> SendFileAsync(string roomId, string filePath, string? caption = null) =>
            UploadAsync<MessageDto>($"api/rooms/{roomId}/files", filePath, caption);

        public Task<MessageDto> DeleteMessageAsync(long messageId) =>
            SendAsync<MessageDto>(HttpMethod.Delete, "api/messages/" + messageId);

        // ================================================================ hỗ trợ

        private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
        {
            using var request = new HttpRequestMessage(method, url);
            if (body != null) request.Content = JsonContent.Create(body, options: JsonOptions);
            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(0, "CANNOT_CONNECT", "Không kết nối được WebService: " + ex.Message);
            }
            catch (TaskCanceledException)
            {
                throw new ApiException(0, "TIMEOUT", "WebService không phản hồi.");
            }
            return await ReadAsync<T>(response);
        }

        private async Task<T> UploadAsync<T>(string url, string filePath, string? content)
        {
            using var form = new MultipartFormDataContent();
            using var file = new StreamContent(File.OpenRead(filePath));
            form.Add(file, "file", Path.GetFileName(filePath));
            if (!string.IsNullOrWhiteSpace(content)) form.Add(new StringContent(content), "content");

            HttpResponseMessage response;
            try
            {
                response = await http.PostAsync(url, form);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(0, "CANNOT_CONNECT", "Không kết nối được WebService: " + ex.Message);
            }
            return await ReadAsync<T>(response);
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return default!;
                    return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
                }

                // Lỗi: {"code":"USERNAME_EXISTS","message":"..."}
                string code = "HTTP_" + (int)response.StatusCode, message = response.ReasonPhrase ?? "Lỗi";
                try
                {
                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (doc.RootElement.TryGetProperty("code", out var c)) code = c.GetString() ?? code;
                    if (doc.RootElement.TryGetProperty("message", out var m)) message = m.GetString() ?? message;
                }
                catch (JsonException)
                {
                    // thân lỗi không phải JSON
                }
                throw new ApiException((int)response.StatusCode, code, message);
            }
        }

        public void Dispose() => http.Dispose();
    }

    /// <summary>Một ApiClient dùng chung cho mọi Form (giống ChatClientManager).</summary>
    internal static class ApiClientManager
    {
        public static ApiClient Api { get; } = new ApiClient("http://127.0.0.1:5000");
    }
}
