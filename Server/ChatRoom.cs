
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.Chat_room_test_TCP
{

    public enum ChatRoomType //loại phòng
    {
        Global,
        Group,
        Private
    }

    public class ChatRoom
    {

        public ChatRoomType RoomType { get; }
        public string AvatarPath { get; set; }

        // Mã định danh phòng chat.
        public string RoomId { get; }

        // Tên hiển thị của phòng chat.
        public string RoomName { get; }

        // Danh sách các kết nối TCP đang tham gia phòng.
        private readonly HashSet<TcpClient> members
            = new HashSet<TcpClient>();

        // Khởi tạo một phòng chat.
        public ChatRoom(string roomId, string roomName, ChatRoomType roomType)
        {
            RoomId = roomId;
            RoomName = roomName;
            RoomType = roomType;
        }

        // Thêm một kết nối vào phòng.
        public bool AddMember(TcpClient client)
        {
            return members.Add(client);
        }

        // Xóa một kết nối khỏi phòng.
        public bool RemoveMember(TcpClient client)
        {
            return members.Remove(client);
        }

        // Kiểm tra kết nối có trong phòng hay không.
        public bool ContainsMember(TcpClient client)
        {
            return members.Contains(client);
        }

        // Lấy danh sách thành viên hiện tại.
        public TcpClient[] GetMembers()
        {
            TcpClient[] result = new TcpClient[members.Count];
            members.CopyTo(result);
            return result;
        }
    }
}
