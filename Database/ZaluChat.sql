/* =====================================================================
   CƠ SỞ DỮ LIỆU ZALU CHAT (SQL Server) - Nhóm 17
   Tên bảng/cột bám theo các lớp đang có trong repo:
     UserAccount  -> Users          (Username, Email, DisplayName, PasswordSalt, PasswordHash)
     UserSession  -> Sessions       (Username, LoginAt)
     ChatRoom     -> ChatRooms      (RoomId, RoomName, RoomType, AvatarPath)
     thành viên   -> RoomMembers
   Thêm: Friendships (bạn bè), Messages (lưu tin nhắn, lịch sử).
   Chạy toàn bộ file này trong SSMS (F5).
   ===================================================================== */

IF DB_ID('ZaluChat') IS NULL
    CREATE DATABASE ZaluChat;
GO
USE ZaluChat;
GO

-- Xoá bảng cũ (chạy lại script từ đầu). Thứ tự: bảng con trước, bảng cha sau.
DROP TABLE IF EXISTS Messages;
DROP TABLE IF EXISTS RoomMembers;
DROP TABLE IF EXISTS ChatRooms;
DROP TABLE IF EXISTS Friendships;
DROP TABLE IF EXISTS Sessions;
DROP TABLE IF EXISTS Users;
GO

-- ---------------------------------------------------------------------
-- 1. TÀI KHOẢN (tương ứng lớp UserAccount)
--    Username là khoá chính; collation mặc định của SQL Server không phân
--    biệt hoa thường nên "Maanx" và "maanx" là một (giống OrdinalIgnoreCase).
-- ---------------------------------------------------------------------
CREATE TABLE Users (
    Username          VARCHAR(32)    NOT NULL PRIMARY KEY,
    Email             VARCHAR(254)   NOT NULL UNIQUE,
    DisplayName       NVARCHAR(32)   NOT NULL,
    PasswordSalt      VARBINARY(16)  NOT NULL,        -- salt ngẫu nhiên 16 byte
    PasswordHash      VARBINARY(32)  NOT NULL,        -- PBKDF2 100.000 vòng, 32 byte
    Bio               NVARCHAR(200)  NULL,            -- "Thông tin giới thiệu"
    AvatarPath        NVARCHAR(260)  NULL,
    LastSeen          DATETIME2      NULL,            -- lần cuối gọi API, dùng tính Online/Offline
    ResetCode         CHAR(6)        NULL,            -- mã quên mật khẩu
    ResetCodeExpires  DATETIME2      NULL,
    CreatedAt         DATETIME2      NOT NULL DEFAULT SYSDATETIME()
);

-- ---------------------------------------------------------------------
-- 2. PHIÊN ĐĂNG NHẬP (tương ứng lớp UserSession)
--    Mỗi lần đăng nhập tạo một token; một tài khoản đăng nhập được nhiều thiết bị.
-- ---------------------------------------------------------------------
CREATE TABLE Sessions (
    Token      CHAR(32)       NOT NULL PRIMARY KEY,   -- Guid dạng "N"
    Username   VARCHAR(32)    NOT NULL REFERENCES Users(Username) ON DELETE CASCADE,
    Device     NVARCHAR(100)  NULL,
    LoginAt    DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    ExpiresAt  DATETIME2      NOT NULL
);
CREATE INDEX IX_Sessions_Username ON Sessions(Username);

-- ---------------------------------------------------------------------
-- 3. BẠN BÈ
--    PENDING  = FromUser đã gửi lời mời cho ToUser
--    ACCEPTED = hai người là bạn
--    Từ chối / huỷ lời mời / xoá bạn = xoá dòng.
-- ---------------------------------------------------------------------
CREATE TABLE Friendships (
    FromUser   VARCHAR(32)  NOT NULL REFERENCES Users(Username),
    ToUser     VARCHAR(32)  NOT NULL REFERENCES Users(Username),
    Status     VARCHAR(10)  NOT NULL DEFAULT 'PENDING' CHECK (Status IN ('PENDING', 'ACCEPTED')),
    CreatedAt  DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    PRIMARY KEY (FromUser, ToUser),
    CHECK (FromUser <> ToUser)
);
CREATE INDEX IX_Friendships_ToUser ON Friendships(ToUser);

-- ---------------------------------------------------------------------
-- 4. PHÒNG CHAT (tương ứng lớp ChatRoom, enum ChatRoomType)
--    Global  = "Nhóm chat tổng", mọi tài khoản tự tham gia
--    Group   = nhóm chat do người dùng tạo
--    Private = chat riêng 2 người
-- ---------------------------------------------------------------------
CREATE TABLE ChatRooms (
    RoomId      VARCHAR(80)    NOT NULL PRIMARY KEY,  -- GLOBAL, GROUP_xxxx, PRIVATE_a_b
    RoomName    NVARCHAR(100)  NOT NULL,
    RoomType    VARCHAR(10)    NOT NULL CHECK (RoomType IN ('Global', 'Group', 'Private')),
    AvatarPath  NVARCHAR(260)  NULL,
    CreatedBy   VARCHAR(32)    NULL REFERENCES Users(Username),
    CreatedAt   DATETIME2      NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE RoomMembers (
    RoomId    VARCHAR(80)  NOT NULL REFERENCES ChatRooms(RoomId) ON DELETE CASCADE,
    Username  VARCHAR(32)  NOT NULL REFERENCES Users(Username),
    IsAdmin   BIT          NOT NULL DEFAULT 0,       -- người tạo nhóm là admin
    JoinedAt  DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    PRIMARY KEY (RoomId, Username)
);
CREATE INDEX IX_RoomMembers_Username ON RoomMembers(Username);

-- ---------------------------------------------------------------------
-- 5. TIN NHẮN
--    Tin văn bản: Content. Tin hình ảnh/tệp: FilePath + FileName (Content có thể trống).
--    Xoá tin = đánh dấu IsDeleted (vẫn giữ trong lịch sử, hiện "Tin nhắn đã bị xoá").
-- ---------------------------------------------------------------------
CREATE TABLE Messages (
    MessageId  BIGINT IDENTITY(1,1) PRIMARY KEY,
    RoomId     VARCHAR(80)     NOT NULL REFERENCES ChatRooms(RoomId) ON DELETE CASCADE,
    Sender     VARCHAR(32)     NOT NULL REFERENCES Users(Username),
    Content    NVARCHAR(2000)  NULL,
    FilePath   NVARCHAR(260)   NULL,
    FileName   NVARCHAR(255)   NULL,
    IsDeleted  BIT             NOT NULL DEFAULT 0,
    SentAt     DATETIME2       NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_Messages_Room ON Messages(RoomId, MessageId);
GO

-- Phòng chat tổng có sẵn (giống ServerForm tạo phòng "GLOBAL" khi khởi động)
INSERT INTO ChatRooms (RoomId, RoomName, RoomType) VALUES ('GLOBAL', N'Nhóm chat tổng', 'Global');
GO

/* Tài khoản mẫu (alice, bob, carol - mật khẩu 12345678) do WebService tự tạo ở lần chạy đầu,
   vì mật khẩu phải được băm PBKDF2 bằng C#, không viết sẵn trong SQL được. */

-- Một vài truy vấn để kiểm tra nhanh trong SSMS:
-- SELECT Username, Email, DisplayName, LastSeen FROM Users;
-- SELECT * FROM ChatRooms;
-- SELECT TOP 20 * FROM Messages ORDER BY MessageId DESC;
