namespace ClientForm
{
    partial class GiaoDien
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_Chat = new Button();
            btn_Friend = new Button();
            btn_GroupChat = new Button();
            btn_Setting = new Button();
            pnl_Menu = new Panel();
            btn_LogOut = new Button();
            pnl_Chat = new Panel();
            pnl_HoiThoai = new Panel();
            flp_TinNhan = new FlowLayoutPanel();
            pnl_Nhap = new Panel();
            btn_DinhKem = new Button();
            btn_Gui = new Button();
            txt_NoiDung = new TextBox();
            pnl_ChatHeader = new Panel();
            btn_GoiThoai = new Button();
            lb_TrangThaiOnline = new Label();
            lb_TenUser = new Label();
            pic_Avatar = new PictureBox();
            pnl_List = new Panel();
            flp_DanhSachChat = new FlowLayoutPanel();
            pnl_TimKiem = new Panel();
            txt_TimKiem = new TextBox();
            pnl_TieuDe = new Panel();
            lb_TieuDe = new Label();
            pnl_Setting = new Panel();
            pnl_TaiKhoan = new Panel();
            btn_LuuInfo = new Button();
            txt_Info = new RichTextBox();
            ln_DisplayUsername = new Label();
            lb_Info = new Label();
            ln_UserEmail = new Label();
            lb_Username = new Label();
            pictureBox1 = new PictureBox();
            lb_TieuDeTaiKhoan = new Label();
            pnl_DanhMucCaiDat = new Panel();
            lb_MenuCaiDat = new Label();
            btn_BaoMat = new Button();
            btn_TaiKhoan = new Button();
            pnl_GroupChat = new Panel();
            pnl_Friend = new Panel();
            pnl_Menu.SuspendLayout();
            pnl_Chat.SuspendLayout();
            pnl_HoiThoai.SuspendLayout();
            pnl_Nhap.SuspendLayout();
            pnl_ChatHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pic_Avatar).BeginInit();
            pnl_List.SuspendLayout();
            pnl_TimKiem.SuspendLayout();
            pnl_TieuDe.SuspendLayout();
            pnl_Setting.SuspendLayout();
            pnl_TaiKhoan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pnl_DanhMucCaiDat.SuspendLayout();
            SuspendLayout();
            // 
            // btn_Chat
            // 
            btn_Chat.FlatAppearance.BorderSize = 0;
            btn_Chat.FlatStyle = FlatStyle.Flat;
            btn_Chat.Location = new Point(1, 0);
            btn_Chat.Name = "btn_Chat";
            btn_Chat.Size = new Size(123, 45);
            btn_Chat.TabIndex = 1;
            btn_Chat.Text = "Tin Nhắn";
            btn_Chat.UseVisualStyleBackColor = true;
            btn_Chat.Click += btn_Chat_Click;
            // 
            // btn_Friend
            // 
            btn_Friend.FlatAppearance.BorderSize = 0;
            btn_Friend.FlatStyle = FlatStyle.Flat;
            btn_Friend.Location = new Point(1, 45);
            btn_Friend.Name = "btn_Friend";
            btn_Friend.Size = new Size(123, 45);
            btn_Friend.TabIndex = 2;
            btn_Friend.Text = "Bạn Bè";
            btn_Friend.UseVisualStyleBackColor = true;
            btn_Friend.Click += btn_Friend_Click;
            // 
            // btn_GroupChat
            // 
            btn_GroupChat.FlatAppearance.BorderSize = 0;
            btn_GroupChat.FlatStyle = FlatStyle.Flat;
            btn_GroupChat.Location = new Point(1, 87);
            btn_GroupChat.Name = "btn_GroupChat";
            btn_GroupChat.Size = new Size(123, 45);
            btn_GroupChat.TabIndex = 3;
            btn_GroupChat.Text = "Nhóm Chat";
            btn_GroupChat.UseVisualStyleBackColor = true;
            btn_GroupChat.Click += btn_GroupChat_Click;
            // 
            // btn_Setting
            // 
            btn_Setting.FlatAppearance.BorderSize = 0;
            btn_Setting.FlatStyle = FlatStyle.Flat;
            btn_Setting.Location = new Point(0, 129);
            btn_Setting.Name = "btn_Setting";
            btn_Setting.Size = new Size(123, 45);
            btn_Setting.TabIndex = 4;
            btn_Setting.Text = "Cài Đặt";
            btn_Setting.UseVisualStyleBackColor = true;
            btn_Setting.Click += btn_Setting_Click;
            // 
            // pnl_Menu
            // 
            pnl_Menu.BackColor = Color.FromArgb(255, 255, 255);
            pnl_Menu.BorderStyle = BorderStyle.FixedSingle;
            pnl_Menu.Controls.Add(btn_Chat);
            pnl_Menu.Controls.Add(btn_Friend);
            pnl_Menu.Controls.Add(btn_GroupChat);
            pnl_Menu.Controls.Add(btn_Setting);
            pnl_Menu.Controls.Add(btn_LogOut);
            pnl_Menu.Dock = DockStyle.Left;
            pnl_Menu.Location = new Point(0, 0);
            pnl_Menu.Name = "pnl_Menu";
            pnl_Menu.Size = new Size(125, 607);
            pnl_Menu.TabIndex = 5;
            // 
            // btn_LogOut
            // 
            btn_LogOut.Dock = DockStyle.Bottom;
            btn_LogOut.FlatAppearance.BorderSize = 0;
            btn_LogOut.FlatStyle = FlatStyle.Flat;
            btn_LogOut.ForeColor = Color.FromArgb(100, 116, 139);
            btn_LogOut.Location = new Point(0, 560);
            btn_LogOut.Name = "btn_LogOut";
            btn_LogOut.Size = new Size(123, 45);
            btn_LogOut.TabIndex = 6;
            btn_LogOut.Text = "Đăng Xuất";
            btn_LogOut.UseVisualStyleBackColor = true;
            btn_LogOut.Click += btn_LogOut_Click;
            btn_LogOut.MouseEnter += btn_LogOut_MouseEnter;
            btn_LogOut.MouseLeave += btn_LogOut_MouseLeave;
            // 
            // pnl_Chat
            // 
            pnl_Chat.Controls.Add(pnl_HoiThoai);
            pnl_Chat.Controls.Add(pnl_List);
            pnl_Chat.Dock = DockStyle.Fill;
            pnl_Chat.Location = new Point(125, 0);
            pnl_Chat.Name = "pnl_Chat";
            pnl_Chat.Size = new Size(923, 607);
            pnl_Chat.TabIndex = 6;
            // 
            // pnl_HoiThoai
            // 
            pnl_HoiThoai.BackColor = Color.FromArgb(241, 245, 249);
            pnl_HoiThoai.BorderStyle = BorderStyle.FixedSingle;
            pnl_HoiThoai.Controls.Add(flp_TinNhan);
            pnl_HoiThoai.Controls.Add(pnl_Nhap);
            pnl_HoiThoai.Controls.Add(pnl_ChatHeader);
            pnl_HoiThoai.Dock = DockStyle.Fill;
            pnl_HoiThoai.Location = new Point(300, 0);
            pnl_HoiThoai.Name = "pnl_HoiThoai";
            pnl_HoiThoai.Size = new Size(623, 607);
            pnl_HoiThoai.TabIndex = 1;
            // 
            // flp_TinNhan
            // 
            flp_TinNhan.AutoScroll = true;
            flp_TinNhan.Dock = DockStyle.Fill;
            flp_TinNhan.FlowDirection = FlowDirection.TopDown;
            flp_TinNhan.Location = new Point(0, 62);
            flp_TinNhan.Name = "flp_TinNhan";
            flp_TinNhan.Size = new Size(621, 475);
            flp_TinNhan.TabIndex = 3;
            flp_TinNhan.WrapContents = false;
            // 
            // pnl_Nhap
            // 
            pnl_Nhap.BackColor = Color.FromArgb(255, 255, 255);
            pnl_Nhap.Controls.Add(btn_DinhKem);
            pnl_Nhap.Controls.Add(btn_Gui);
            pnl_Nhap.Controls.Add(txt_NoiDung);
            pnl_Nhap.Dock = DockStyle.Bottom;
            pnl_Nhap.Location = new Point(0, 537);
            pnl_Nhap.Name = "pnl_Nhap";
            pnl_Nhap.Size = new Size(621, 68);
            pnl_Nhap.TabIndex = 2;
            // 
            // btn_DinhKem
            // 
            btn_DinhKem.Anchor = AnchorStyles.Left;
            btn_DinhKem.BackColor = Color.Transparent;
            btn_DinhKem.FlatAppearance.BorderSize = 0;
            btn_DinhKem.FlatStyle = FlatStyle.Flat;
            btn_DinhKem.ForeColor = Color.White;
            btn_DinhKem.Image = Properties.Resources.paperclip;
            btn_DinhKem.Location = new Point(5, 11);
            btn_DinhKem.Name = "btn_DinhKem";
            btn_DinhKem.Size = new Size(56, 44);
            btn_DinhKem.TabIndex = 2;
            btn_DinhKem.UseVisualStyleBackColor = false;
            // 
            // btn_Gui
            // 
            btn_Gui.Anchor = AnchorStyles.Right;
            btn_Gui.BackColor = Color.FromArgb(37, 99, 235);
            btn_Gui.FlatAppearance.BorderSize = 0;
            btn_Gui.FlatStyle = FlatStyle.Flat;
            btn_Gui.ForeColor = Color.White;
            btn_Gui.Image = Properties.Resources.send;
            btn_Gui.Location = new Point(556, 11);
            btn_Gui.Name = "btn_Gui";
            btn_Gui.Size = new Size(54, 44);
            btn_Gui.TabIndex = 1;
            btn_Gui.UseVisualStyleBackColor = false;
            btn_Gui.MouseEnter += btn_Gui_MouseEnter;
            btn_Gui.MouseLeave += btn_Gui_MouseLeave;
            // 
            // txt_NoiDung
            // 
            txt_NoiDung.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txt_NoiDung.Location = new Point(67, 11);
            txt_NoiDung.Multiline = true;
            txt_NoiDung.Name = "txt_NoiDung";
            txt_NoiDung.PlaceholderText = "Nhập tin nhắn. . .";
            txt_NoiDung.Size = new Size(483, 44);
            txt_NoiDung.TabIndex = 0;
            // 
            // pnl_ChatHeader
            // 
            pnl_ChatHeader.BackColor = Color.FromArgb(255, 255, 255);
            pnl_ChatHeader.Controls.Add(btn_GoiThoai);
            pnl_ChatHeader.Controls.Add(lb_TrangThaiOnline);
            pnl_ChatHeader.Controls.Add(lb_TenUser);
            pnl_ChatHeader.Controls.Add(pic_Avatar);
            pnl_ChatHeader.Dock = DockStyle.Top;
            pnl_ChatHeader.Location = new Point(0, 0);
            pnl_ChatHeader.Name = "pnl_ChatHeader";
            pnl_ChatHeader.Size = new Size(621, 62);
            pnl_ChatHeader.TabIndex = 0;
            // 
            // btn_GoiThoai
            // 
            btn_GoiThoai.Anchor = AnchorStyles.Right;
            btn_GoiThoai.Location = new Point(535, 17);
            btn_GoiThoai.Name = "btn_GoiThoai";
            btn_GoiThoai.Size = new Size(75, 23);
            btn_GoiThoai.TabIndex = 3;
            btn_GoiThoai.Text = "Gọi Thoại";
            btn_GoiThoai.UseVisualStyleBackColor = true;
            // 
            // lb_TrangThaiOnline
            // 
            lb_TrangThaiOnline.AutoSize = true;
            lb_TrangThaiOnline.ForeColor = Color.FromArgb(22, 163, 74);
            lb_TrangThaiOnline.Location = new Point(84, 36);
            lb_TrangThaiOnline.Name = "lb_TrangThaiOnline";
            lb_TrangThaiOnline.Size = new Size(83, 15);
            lb_TrangThaiOnline.TabIndex = 2;
            lb_TrangThaiOnline.Text = "Online/Offline";
            // 
            // lb_TenUser
            // 
            lb_TenUser.AutoSize = true;
            lb_TenUser.Font = new Font("Segoe UI", 13F);
            lb_TenUser.Location = new Point(84, 8);
            lb_TenUser.Name = "lb_TenUser";
            lb_TenUser.Size = new Size(54, 25);
            lb_TenUser.TabIndex = 1;
            lb_TenUser.Text = "USER";
            // 
            // pic_Avatar
            // 
            pic_Avatar.BackColor = SystemColors.ActiveCaption;
            pic_Avatar.Location = new Point(21, 11);
            pic_Avatar.Name = "pic_Avatar";
            pic_Avatar.Size = new Size(40, 40);
            pic_Avatar.TabIndex = 0;
            pic_Avatar.TabStop = false;
            // 
            // pnl_List
            // 
            pnl_List.BackColor = Color.FromArgb(255, 255, 255);
            pnl_List.BorderStyle = BorderStyle.FixedSingle;
            pnl_List.Controls.Add(flp_DanhSachChat);
            pnl_List.Controls.Add(pnl_TimKiem);
            pnl_List.Controls.Add(pnl_TieuDe);
            pnl_List.Dock = DockStyle.Left;
            pnl_List.Location = new Point(0, 0);
            pnl_List.Name = "pnl_List";
            pnl_List.Size = new Size(300, 607);
            pnl_List.TabIndex = 0;
            // 
            // flp_DanhSachChat
            // 
            flp_DanhSachChat.AutoScroll = true;
            flp_DanhSachChat.Dock = DockStyle.Fill;
            flp_DanhSachChat.FlowDirection = FlowDirection.TopDown;
            flp_DanhSachChat.Location = new Point(0, 87);
            flp_DanhSachChat.Name = "flp_DanhSachChat";
            flp_DanhSachChat.Size = new Size(298, 518);
            flp_DanhSachChat.TabIndex = 2;
            flp_DanhSachChat.WrapContents = false;
            // 
            // pnl_TimKiem
            // 
            pnl_TimKiem.Controls.Add(txt_TimKiem);
            pnl_TimKiem.Dock = DockStyle.Top;
            pnl_TimKiem.ForeColor = Color.FromArgb(255, 255, 255);
            pnl_TimKiem.Location = new Point(0, 45);
            pnl_TimKiem.Name = "pnl_TimKiem";
            pnl_TimKiem.Size = new Size(298, 42);
            pnl_TimKiem.TabIndex = 1;
            // 
            // txt_TimKiem
            // 
            txt_TimKiem.BackColor = Color.FromArgb(241, 245, 249);
            txt_TimKiem.Font = new Font("Segoe UI", 12F);
            txt_TimKiem.Location = new Point(-1, 6);
            txt_TimKiem.Name = "txt_TimKiem";
            txt_TimKiem.PlaceholderText = "Tìm Kiếm. . .";
            txt_TimKiem.Size = new Size(300, 29);
            txt_TimKiem.TabIndex = 0;
            // 
            // pnl_TieuDe
            // 
            pnl_TieuDe.Controls.Add(lb_TieuDe);
            pnl_TieuDe.Dock = DockStyle.Top;
            pnl_TieuDe.Location = new Point(0, 0);
            pnl_TieuDe.Name = "pnl_TieuDe";
            pnl_TieuDe.Size = new Size(298, 45);
            pnl_TieuDe.TabIndex = 0;
            // 
            // lb_TieuDe
            // 
            lb_TieuDe.AutoSize = true;
            lb_TieuDe.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lb_TieuDe.ForeColor = Color.FromArgb(15, 23, 42);
            lb_TieuDe.Location = new Point(76, 9);
            lb_TieuDe.Name = "lb_TieuDe";
            lb_TieuDe.Size = new Size(152, 30);
            lb_TieuDe.TabIndex = 0;
            lb_TieuDe.Text = "TRÒ CHUYỆN";
            // 
            // pnl_Setting
            // 
            pnl_Setting.BackColor = Color.FromArgb(241, 245, 249);
            pnl_Setting.Controls.Add(pnl_TaiKhoan);
            pnl_Setting.Controls.Add(pnl_DanhMucCaiDat);
            pnl_Setting.Dock = DockStyle.Fill;
            pnl_Setting.Location = new Point(125, 0);
            pnl_Setting.Name = "pnl_Setting";
            pnl_Setting.Size = new Size(923, 607);
            pnl_Setting.TabIndex = 7;
            // 
            // pnl_TaiKhoan
            // 
            pnl_TaiKhoan.BackColor = Color.White;
            pnl_TaiKhoan.BorderStyle = BorderStyle.FixedSingle;
            pnl_TaiKhoan.Controls.Add(btn_LuuInfo);
            pnl_TaiKhoan.Controls.Add(txt_Info);
            pnl_TaiKhoan.Controls.Add(ln_DisplayUsername);
            pnl_TaiKhoan.Controls.Add(lb_Info);
            pnl_TaiKhoan.Controls.Add(ln_UserEmail);
            pnl_TaiKhoan.Controls.Add(lb_Username);
            pnl_TaiKhoan.Controls.Add(pictureBox1);
            pnl_TaiKhoan.Controls.Add(lb_TieuDeTaiKhoan);
            pnl_TaiKhoan.Dock = DockStyle.Fill;
            pnl_TaiKhoan.Location = new Point(144, 0);
            pnl_TaiKhoan.Name = "pnl_TaiKhoan";
            pnl_TaiKhoan.Size = new Size(779, 607);
            pnl_TaiKhoan.TabIndex = 1;
            // 
            // btn_LuuInfo
            // 
            btn_LuuInfo.Anchor = AnchorStyles.Top;
            btn_LuuInfo.BackColor = Color.FromArgb(37, 99, 235);
            btn_LuuInfo.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btn_LuuInfo.Location = new Point(150, 506);
            btn_LuuInfo.Name = "btn_LuuInfo";
            btn_LuuInfo.Size = new Size(494, 36);
            btn_LuuInfo.TabIndex = 9;
            btn_LuuInfo.Text = "LƯU THAY ĐỔI";
            btn_LuuInfo.UseVisualStyleBackColor = true;
            // 
            // txt_Info
            // 
            txt_Info.Anchor = AnchorStyles.Top;
            txt_Info.Location = new Point(150, 357);
            txt_Info.Name = "txt_Info";
            txt_Info.Size = new Size(494, 128);
            txt_Info.TabIndex = 8;
            txt_Info.Text = "";
            // 
            // ln_DisplayUsername
            // 
            ln_DisplayUsername.Anchor = AnchorStyles.Top;
            ln_DisplayUsername.AutoSize = true;
            ln_DisplayUsername.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            ln_DisplayUsername.Location = new Point(293, 213);
            ln_DisplayUsername.Name = "ln_DisplayUsername";
            ln_DisplayUsername.Size = new Size(231, 31);
            ln_DisplayUsername.TabIndex = 7;
            ln_DisplayUsername.Text = "DISPLAYUSERNAME";
            // 
            // lb_Info
            // 
            lb_Info.Anchor = AnchorStyles.Top;
            lb_Info.AutoSize = true;
            lb_Info.Font = new Font("Segoe UI", 17F);
            lb_Info.Location = new Point(293, 323);
            lb_Info.Name = "lb_Info";
            lb_Info.Size = new Size(226, 31);
            lb_Info.TabIndex = 6;
            lb_Info.Text = "Thông Tin Giới Thiệu";
            // 
            // ln_UserEmail
            // 
            ln_UserEmail.Anchor = AnchorStyles.Top;
            ln_UserEmail.AutoSize = true;
            ln_UserEmail.Font = new Font("Segoe UI", 13F);
            ln_UserEmail.Location = new Point(329, 272);
            ln_UserEmail.Name = "ln_UserEmail";
            ln_UserEmail.Size = new Size(147, 25);
            ln_UserEmail.TabIndex = 5;
            ln_UserEmail.Text = "User@gmail.com";
            // 
            // lb_Username
            // 
            lb_Username.Anchor = AnchorStyles.Top;
            lb_Username.AutoSize = true;
            lb_Username.Font = new Font("Segoe UI", 13F);
            lb_Username.Location = new Point(352, 244);
            lb_Username.Name = "lb_Username";
            lb_Username.Size = new Size(104, 25);
            lb_Username.TabIndex = 4;
            lb_Username.Text = "USERNAME";
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top;
            pictureBox1.BackColor = SystemColors.ActiveCaption;
            pictureBox1.Location = new Point(361, 130);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(80, 80);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // lb_TieuDeTaiKhoan
            // 
            lb_TieuDeTaiKhoan.AutoSize = true;
            lb_TieuDeTaiKhoan.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lb_TieuDeTaiKhoan.ForeColor = Color.FromArgb(15, 23, 42);
            lb_TieuDeTaiKhoan.Location = new Point(21, 23);
            lb_TieuDeTaiKhoan.Name = "lb_TieuDeTaiKhoan";
            lb_TieuDeTaiKhoan.Size = new Size(165, 37);
            lb_TieuDeTaiKhoan.TabIndex = 0;
            lb_TieuDeTaiKhoan.Text = "TÀI KHOẢN";
            // 
            // pnl_DanhMucCaiDat
            // 
            pnl_DanhMucCaiDat.BackColor = Color.FromArgb(255, 255, 255);
            pnl_DanhMucCaiDat.BorderStyle = BorderStyle.FixedSingle;
            pnl_DanhMucCaiDat.Controls.Add(lb_MenuCaiDat);
            pnl_DanhMucCaiDat.Controls.Add(btn_BaoMat);
            pnl_DanhMucCaiDat.Controls.Add(btn_TaiKhoan);
            pnl_DanhMucCaiDat.Dock = DockStyle.Left;
            pnl_DanhMucCaiDat.Location = new Point(0, 0);
            pnl_DanhMucCaiDat.Name = "pnl_DanhMucCaiDat";
            pnl_DanhMucCaiDat.Size = new Size(144, 607);
            pnl_DanhMucCaiDat.TabIndex = 0;
            // 
            // lb_MenuCaiDat
            // 
            lb_MenuCaiDat.AutoSize = true;
            lb_MenuCaiDat.Font = new Font("Segoe UI", 16F);
            lb_MenuCaiDat.Location = new Point(21, 11);
            lb_MenuCaiDat.Name = "lb_MenuCaiDat";
            lb_MenuCaiDat.Size = new Size(94, 30);
            lb_MenuCaiDat.TabIndex = 5;
            lb_MenuCaiDat.Text = "CÀI ĐẶT";
            // 
            // btn_BaoMat
            // 
            btn_BaoMat.FlatAppearance.BorderSize = 0;
            btn_BaoMat.FlatStyle = FlatStyle.Flat;
            btn_BaoMat.Location = new Point(6, 100);
            btn_BaoMat.Name = "btn_BaoMat";
            btn_BaoMat.Size = new Size(123, 45);
            btn_BaoMat.TabIndex = 4;
            btn_BaoMat.Text = "Bảo Mật";
            btn_BaoMat.UseVisualStyleBackColor = true;
            // 
            // btn_TaiKhoan
            // 
            btn_TaiKhoan.FlatAppearance.BorderSize = 0;
            btn_TaiKhoan.FlatStyle = FlatStyle.Flat;
            btn_TaiKhoan.Location = new Point(6, 49);
            btn_TaiKhoan.Name = "btn_TaiKhoan";
            btn_TaiKhoan.Size = new Size(123, 45);
            btn_TaiKhoan.TabIndex = 2;
            btn_TaiKhoan.Text = "Tài Khoản";
            btn_TaiKhoan.UseVisualStyleBackColor = true;
            // 
            // pnl_GroupChat
            // 
            pnl_GroupChat.Dock = DockStyle.Fill;
            pnl_GroupChat.Location = new Point(0, 0);
            pnl_GroupChat.Name = "pnl_GroupChat";
            pnl_GroupChat.Size = new Size(1048, 607);
            pnl_GroupChat.TabIndex = 8;
            // 
            // pnl_Friend
            // 
            pnl_Friend.Dock = DockStyle.Fill;
            pnl_Friend.Location = new Point(125, 0);
            pnl_Friend.Name = "pnl_Friend";
            pnl_Friend.Size = new Size(923, 607);
            pnl_Friend.TabIndex = 9;
            // 
            // GiaoDien
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1048, 607);
            Controls.Add(pnl_Chat);
            Controls.Add(pnl_Setting);
            Controls.Add(pnl_Friend);
            Controls.Add(pnl_Menu);
            Controls.Add(pnl_GroupChat);
            Name = "GiaoDien";
            Text = "Form1";
            pnl_Menu.ResumeLayout(false);
            pnl_Chat.ResumeLayout(false);
            pnl_HoiThoai.ResumeLayout(false);
            pnl_Nhap.ResumeLayout(false);
            pnl_Nhap.PerformLayout();
            pnl_ChatHeader.ResumeLayout(false);
            pnl_ChatHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pic_Avatar).EndInit();
            pnl_List.ResumeLayout(false);
            pnl_TimKiem.ResumeLayout(false);
            pnl_TimKiem.PerformLayout();
            pnl_TieuDe.ResumeLayout(false);
            pnl_TieuDe.PerformLayout();
            pnl_Setting.ResumeLayout(false);
            pnl_TaiKhoan.ResumeLayout(false);
            pnl_TaiKhoan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnl_DanhMucCaiDat.ResumeLayout(false);
            pnl_DanhMucCaiDat.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btn_Chat;
        private Button btn_Friend;
        private Button btn_GroupChat;
        private Button btn_Setting;
        private Panel pnl_Menu;
        private Button btn_LogOut;
        private Panel pnl_Chat;
        private Panel pnl_Setting;
        private Panel pnl_GroupChat;
        private Panel pnl_Friend;
        private Panel pnl_HoiThoai;
        private Panel pnl_List;
        private Panel pnl_TieuDe;
        private Label lb_TieuDe;
        private Panel pnl_TimKiem;
        private FlowLayoutPanel flp_DanhSachChat;
        private TextBox txt_TimKiem;
        private Panel pnl_ChatHeader;
        private PictureBox pic_Avatar;
        private Label lb_TenUser;
        private Button btn_GoiThoai;
        private Label lb_TrangThaiOnline;
        private Panel pnl_Nhap;
        private Button btn_DinhKem;
        private Button btn_Gui;
        private TextBox txt_NoiDung;
        private FlowLayoutPanel flp_TinNhan;
        private Panel pnl_DanhMucCaiDat;
        private Button btn_TaiKhoan;
        private Label lb_MenuCaiDat;
        private Button btn_BaoMat;
        private Panel pnl_TaiKhoan;
        private Label lb_TieuDeTaiKhoan;
        private Label ln_UserEmail;
        private Label lb_Username;
        private PictureBox pictureBox1;
        private Label ln_DisplayUsername;
        private Label lb_Info;
        private RichTextBox txt_Info;
        private Button btn_LuuInfo;
    }
}