using ChatappZalu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class GiaoDien : Form
    {
        private sealed class GlobalMessageLayout
        {
            public bool IsMine { get; }
            public Panel Bubble { get; }
            public PictureBox Avatar { get; }

            public GlobalMessageLayout(bool isMine, Panel bubble, PictureBox avatar)
            {
                IsMine = isMine;
                Bubble = bubble;
                Avatar = avatar;
            }
        }

        private const string GlobalRoomId = "GLOBAL";
        private bool logoutRequestPending = false;
        private readonly System.Windows.Forms.Timer globalChatTimer = new System.Windows.Forms.Timer();
        private readonly HashSet<long> renderedMessageIds = new HashSet<long>();
        private readonly Dictionary<string, string?> globalMemberAvatarPaths = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Image> avatarImageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private long lastGlobalMessageId;
        private bool globalChatLoaded;
        private bool pollingGlobalMessages;
        private bool sendingGlobalMessage;
        private int globalRoomMemberCount;

        public GiaoDien()
        {
            InitializeComponent();

            this.BackColor = AppColors.Background;
            ChonNutMenu(btn_Chat);
            ChuyenPanelMenu(pnl_Chat);

            pnl_HoiThoaiCaNhan.Visible = false;
            pnl_HoiThoaiNhom.Visible = true;
            lb_TenNhom.Text = "Nhóm chat tổng";
            lb_TrangThaiOnlineNhom.Text = "Tất cả tài khoản";
            btn_DinhKemNhom.Enabled = false;
            flp_TinNhanNhom.AutoScroll = true;
            flp_TinNhanNhom.FlowDirection = FlowDirection.TopDown;
            flp_TinNhanNhom.WrapContents = false;

            globalChatTimer.Interval = 2000;
            globalChatTimer.Tick += GlobalChatTimer_Tick;
            flp_TinNhanNhom.SizeChanged += Flp_TinNhanNhom_SizeChanged;
            btn_GuiNhom.Click += btn_GuiNhom_Click;
            txt_NoiDungNhom.KeyDown += txt_NoiDungNhom_KeyDown;
            Shown += GiaoDien_Shown;
            FormClosed += GiaoDien_FormClosed;
        }

        private async void GiaoDien_Shown(object? sender, EventArgs e)
        {
            await LoadGlobalChatAsync();
            if (globalChatLoaded) globalChatTimer.Start();
        }

        private void GiaoDien_FormClosed(object? sender, FormClosedEventArgs e)
        {
            globalChatTimer.Stop();
            globalChatTimer.Dispose();
            foreach (var image in avatarImageCache.Values)
                image.Dispose();
            avatarImageCache.Clear();
        }

        private bool loadingGlobalChat;

        private async System.Threading.Tasks.Task LoadGlobalChatAsync()
        {
            if (loadingGlobalChat) return;
            loadingGlobalChat = true;
            lb_TenNhom.Text = "Đang tải phòng chat tổng...";
            try
            {
                var room = await ApiClientManager.Api.GetRoomAsync(GlobalRoomId);
                var messages = await ApiClientManager.Api.GetLatestMessagesAsync(GlobalRoomId);

                lb_TenNhom.Text = room.RoomName;
                globalRoomMemberCount = room.Members.Count;
                lb_TrangThaiOnlineNhom.Text = globalRoomMemberCount + " thành viên";
                globalMemberAvatarPaths.Clear();
                foreach (var member in room.Members)
                    globalMemberAvatarPaths[member.Username] = member.AvatarPath;
                flp_TinNhanNhom.Controls.Clear();
                renderedMessageIds.Clear();
                lastGlobalMessageId = 0;

                foreach (var message in messages)
                    AddGlobalMessage(message);

                globalChatLoaded = true;
                if (messages.Count == 0)
                    AddGlobalChatNotice("Chưa có tin nhắn. Hãy gửi lời chào đầu tiên!");
            }
            catch (ApiException ex)
            {
                lb_TenNhom.Text = "Nhóm chat tổng";
                MessageBox.Show(
                    "Không thể tải phòng chat tổng từ WebService.\n" + ex.Message,
                    "Lỗi tải chat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lb_TenNhom.Text = "Nhóm chat tổng";
                MessageBox.Show(
                    "Có lỗi khi tải phòng chat tổng.\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                loadingGlobalChat = false;
            }
        }

        private async void GlobalChatTimer_Tick(object? sender, EventArgs e)
        {
            if (pollingGlobalMessages || !globalChatLoaded || !pnl_Chat.Visible)
                return;

            pollingGlobalMessages = true;
            try
            {
                var messages = await ApiClientManager.Api.GetNewMessagesAsync(GlobalRoomId, lastGlobalMessageId);
                foreach (var message in messages)
                    AddGlobalMessage(message);
                lb_TrangThaiOnlineNhom.Text = globalRoomMemberCount + " thành viên";
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 401)
                {
                    globalChatTimer.Stop();
                    MessageBox.Show(
                        "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                        "Phiên hết hạn",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    lb_TrangThaiOnlineNhom.Text = "Đang chờ kết nối WebService...";
                }
            }
            catch
            {
                lb_TrangThaiOnlineNhom.Text = "Đang chờ kết nối WebService...";
            }
            finally
            {
                pollingGlobalMessages = false;
            }
        }

        private void AddGlobalMessage(MessageDto message)
        {
            if (!renderedMessageIds.Add(message.MessageId)) return;
            if (message.MessageId > lastGlobalMessageId)
                lastGlobalMessageId = message.MessageId;

            var flow = flp_TinNhanNhom;
            if (renderedMessageIds.Count == 1 && flow.Controls.Count == 1 && flow.Controls[0] is Label)
                flow.Controls.Clear();
            int rowWidth = Math.Max(240, flow.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 18);
            const int avatarSize = 38;
            const int gap = 8;
            int maxBubbleWidth = Math.Max(160, Math.Min(440, rowWidth - avatarSize - gap - 20));
            bool isMine = string.Equals(message.Sender, ApiClientManager.Api.Username, StringComparison.OrdinalIgnoreCase);
            string bodyText = message.IsDeleted
                ? "Tin nhắn đã bị xóa."
                : message.Content ?? (message.FileName == null ? "" : "[Tệp] " + message.FileName);
            string headerText = message.SenderName + "  ·  " + message.SentAt.ToLocalTime().ToString("HH:mm");
            using var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 10F);
            Size measuredBody = TextRenderer.MeasureText(bodyText, bodyFont,
                new Size(maxBubbleWidth - 24, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            Size measuredHeader = TextRenderer.MeasureText(headerText, headerFont);
            int bubbleWidth = Math.Min(maxBubbleWidth, Math.Max(150, Math.Max(measuredBody.Width, measuredHeader.Width) + 20));
            int bodyWidth = bubbleWidth - 20;
            int bodyHeight = Math.Max(20, TextRenderer.MeasureText(bodyText, bodyFont,
                new Size(bodyWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height + 2);
            int bubbleHeight = 8 + 18 + 5 + bodyHeight + 10;

            var row = new Panel
            {
                Width = rowWidth,
                Height = bubbleHeight + 8,
                Margin = new Padding(4),
                BackColor = Color.Transparent
            };
            var bubble = new Panel
            {
                Size = new Size(bubbleWidth, bubbleHeight),
                Location = new Point(isMine ? rowWidth - bubbleWidth - avatarSize - gap - 8 : avatarSize + gap + 8, 4),
                BackColor = isMine ? Color.FromArgb(219, 234, 254) : Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var header = new Label
            {
                AutoSize = false,
                Text = headerText,
                TextAlign = isMine ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                Font = headerFont,
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(10, 7),
                Size = new Size(bodyWidth, 18)
            };
            var body = new Label
            {
                AutoSize = false,
                Text = bodyText,
                Font = bodyFont,
                Location = new Point(10, 30),
                Size = new Size(bodyWidth, bodyHeight)
            };

            var avatar = new PictureBox
            {
                Size = new Size(avatarSize, avatarSize),
                Location = new Point(isMine ? rowWidth - avatarSize - 8 : 8, (row.Height - avatarSize) / 2),
                BackColor = Color.FromArgb(203, 213, 225),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            var initials = new Label
            {
                Dock = DockStyle.Fill,
                Text = GetInitials(message.SenderName),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                BackColor = Color.Transparent
            };
            avatar.Controls.Add(initials);
            bubble.Controls.Add(header);
            bubble.Controls.Add(body);
            row.Controls.Add(bubble);
            row.Controls.Add(avatar);
            row.Tag = new GlobalMessageLayout(isMine, bubble, avatar);
            flow.Controls.Add(row);
            flow.ScrollControlIntoView(row);

            if (globalMemberAvatarPaths.TryGetValue(message.Sender, out string? avatarPath) && !string.IsNullOrWhiteSpace(avatarPath))
                _ = LoadAvatarAsync(avatar, initials, avatarPath);
        }

        private void Flp_TinNhanNhom_SizeChanged(object? sender, EventArgs e)
        {
            int rowWidth = Math.Max(240, flp_TinNhanNhom.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 18);
            foreach (Control control in flp_TinNhanNhom.Controls)
            {
                if (control is not Panel row || !(row.Tag is GlobalMessageLayout layout))
                    continue;

                row.Width = rowWidth;
                int bubbleWidth = layout.Bubble.Width;
                layout.Bubble.Location = new Point(
                    layout.IsMine ? rowWidth - bubbleWidth - layout.Avatar.Width - 16 : layout.Avatar.Width + 16,
                    4);
                layout.Avatar.Location = new Point(
                    layout.IsMine ? rowWidth - layout.Avatar.Width - 8 : 8,
                    (row.Height - layout.Avatar.Height) / 2);
            }
        }

        private static string GetInitials(string displayName)
        {
            string[] parts = displayName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                return (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant();
            return displayName.Length == 0 ? "?" : displayName.Substring(0, 1).ToUpperInvariant();
        }

        private async System.Threading.Tasks.Task LoadAvatarAsync(PictureBox picture, Label initials, string path)
        {
            try
            {
                if (!avatarImageCache.TryGetValue(path, out Image? image))
                {
                    image = await ApiClientManager.Api.DownloadImageAsync(path);
                    if (IsDisposed || picture.IsDisposed)
                    {
                        image.Dispose();
                        return;
                    }
                    avatarImageCache[path] = image;
                }

                if (!IsDisposed && !picture.IsDisposed)
                {
                    picture.Image = image;
                    initials.Visible = false;
                }
            }
            catch
            {
                // Giữ avatar chữ cái nếu ảnh chưa có hoặc không tải được.
            }
        }

        private void AddGlobalChatNotice(string text)
        {
            flp_TinNhanNhom.Controls.Clear();
            flp_TinNhanNhom.Controls.Add(new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                Margin = new Padding(12)
            });
        }

        private async void btn_GuiNhom_Click(object? sender, EventArgs e)
        {
            string content = txt_NoiDungNhom.Text.Trim();
            if (content.Length == 0 || sendingGlobalMessage) return;

            sendingGlobalMessage = true;
            btn_GuiNhom.Enabled = false;
            try
            {
                var message = await ApiClientManager.Api.SendMessageAsync(GlobalRoomId, content);
                txt_NoiDungNhom.Clear();
                AddGlobalMessage(message);
                globalChatLoaded = true;
            }
            catch (ApiException ex)
            {
                MessageBox.Show(
                    "Không gửi được tin nhắn.\n" + ex.Message,
                    "Gửi tin nhắn thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi khi gửi tin nhắn.\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                sendingGlobalMessage = false;
                btn_GuiNhom.Enabled = true;
            }
        }

        private void txt_NoiDungNhom_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btn_GuiNhom_Click(sender, EventArgs.Empty);
            }
        }


        private void ChonNutMenu(Button nutDuocChon)
        {
            Button[] danhSachNut =
            {
                btn_Chat, btn_Friend, btn_Setting
            };
            foreach (Button nut in danhSachNut)
            {
                nut.BackColor = AppColors.Surface;
                nut.ForeColor = AppColors.TextSecondary;
            }
            nutDuocChon.BackColor = AppColors.Selected;
            nutDuocChon.ForeColor = AppColors.Primary;
        }
        private void ChonNutBanBe(Button nutDuocChon)
        {
            Button[] danhSachNut =
            {
                btn_LoiMoiKetBan, btn_DanhSachBan
            };
            foreach (Button nut in danhSachNut)
            {
                nut.BackColor = AppColors.Surface;
                nut.ForeColor = AppColors.TextSecondary;
            }
            nutDuocChon.BackColor = AppColors.Selected;
            nutDuocChon.ForeColor = AppColors.Primary;
        }
        private void ChonNutCaiDat(Button nutDuocChon)
        {
            Button[] danhSachNut =
            {
                btn_BaoMat, btn_TaiKhoan
            };
            foreach (Button nut in danhSachNut)
            {
                nut.BackColor = AppColors.Surface;
                nut.ForeColor = AppColors.TextSecondary;
            }
            nutDuocChon.BackColor = AppColors.Selected;
            nutDuocChon.ForeColor = AppColors.Primary;
        }
        private void ChuyenPanelMenu(Panel PanelDuocChon)
        {
            Panel[] danhSachpanel =
            {
                pnl_Chat, pnl_Friend, pnl_Setting
            };
            foreach (Panel pnl in danhSachpanel)
            {
                pnl.Visible = false;
            }
            PanelDuocChon.Visible = true;
        }
        private void ChuyenPanelBanBe(Panel PanelDuocChon)
        {
            Panel[] danhSachpanel =
            {
                pnl_LoiMoiKetBan, pnl_DanhSachBan
            };
            foreach (Panel pnl in danhSachpanel)
            {
                pnl.Visible = false;
            }
            PanelDuocChon.Visible = true;
        }
        private void ChuyenPanelCaiDat(Panel PanelDuocChon)
        {
            Panel[] danhSachpanel =
            {
                pnl_TaiKhoan, pnl_BaoMat
            };
            foreach (Panel pnl in danhSachpanel)
            {
                pnl.Visible = false;
            }
            PanelDuocChon.Visible = true;
        }

        private async void btn_Chat_Click(object sender, EventArgs e)
        {
            ChonNutMenu(btn_Chat);
            ChuyenPanelMenu(pnl_Chat);
            if (!globalChatLoaded)
                await LoadGlobalChatAsync();
            if (globalChatLoaded) globalChatTimer.Start();
        }

        private void btn_Friend_Click(object sender, EventArgs e)
        {
            globalChatTimer.Stop();
            ChonNutMenu(btn_Friend);
            ChuyenPanelMenu(pnl_Friend);
            ChuyenPanelBanBe(pnl_DanhSachBan);
            ChonNutBanBe(btn_DanhSachBan);
        }

        private async void btn_Setting_Click(object sender, EventArgs e)
        {
            globalChatTimer.Stop();
            ChonNutMenu(btn_Setting);
            ChuyenPanelMenu(pnl_Setting);
            ChuyenPanelCaiDat(pnl_TaiKhoan);
            ChonNutCaiDat(btn_TaiKhoan);
            await LoadAccountProfileAsync();
        }

        private void btn_LogOut_MouseEnter(object sender, EventArgs e)
        {
            btn_LogOut.ForeColor = AppColors.Danger;
        }

        private void btn_LogOut_MouseLeave(object sender, EventArgs e)
        {
            btn_LogOut.BackColor = AppColors.Surface;
            btn_LogOut.ForeColor = AppColors.TextSecondary;
        }

        /*private void HienThiPhongChatTong()
        {
            Panel item = new Panel();

            item.Width = flp_DanhSachChat.ClientSize.Width - 5;
            item.Height = 65;
            item.BackColor = Color.White;
            item.Margin = new Padding(0, 0, 0, 5);
            item.Cursor = Cursors.Hand;

            Label lbTenPhong = new Label();
            lbTenPhong.Text = "Nhóm chat tổng";
            lbTenPhong.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbTenPhong.Location = new Point(15, 10);
            lbTenPhong.AutoSize = true;

            Label lbMoTa = new Label();
            lbMoTa.Text = "Tất cả mọi người";
            lbMoTa.Font = new Font("Segoe UI", 8);
            lbMoTa.ForeColor = Color.Gray;
            lbMoTa.Location = new Point(15, 35);
            lbMoTa.AutoSize = true;

            item.Controls.Add(lbTenPhong);
            item.Controls.Add(lbMoTa);

            flp_DanhSachChat.Controls.Add(item);
        }*/

        private void pnl_Chatting_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnl_List_Paint(object sender, PaintEventArgs e)
        {

        }



        private async void btn_LogOut_Click(object sender, EventArgs e)
        {
            if (logoutRequestPending)
                return;

            logoutRequestPending = true;
            btn_LogOut.Enabled = false;

            try
            {
                await ApiClientManager.Api.LogoutAsync();

                MessageBox.Show(
                    "Đăng xuất thành công!",
                    "Zalu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Hide();
                Login login = new Login();
                login.ShowDialog();
                this.Close();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(
                    "Không thể đăng xuất qua WebService.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Đăng xuất thất bại.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                logoutRequestPending = false;
                btn_LogOut.Enabled = true;
            }
        }

        private void btn_Gui_MouseEnter(object sender, EventArgs e)
        {
            btn_Gui.BackColor = AppColors.PrimaryHover;
        }

        private void btn_Gui_MouseLeave(object sender, EventArgs e)
        {
            btn_Gui.BackColor = AppColors.Primary;
        }

        private async void btn_TaiKhoan_Click(object sender, EventArgs e)
        {
            ChuyenPanelCaiDat(pnl_TaiKhoan);
            ChonNutCaiDat(btn_TaiKhoan);
            await LoadAccountProfileAsync();
        }

        private async System.Threading.Tasks.Task LoadAccountProfileAsync() //load dữ liệu của tài khoản lên giao diện
        {
            btn_TaiKhoan.Enabled = false;
            try
            {
                var profile = await ApiClientManager.Api.GetMeAsync();
                var user = profile.User;

                ln_DisplayUsername.Text = user.DisplayName;
                lb_Username.Text = user.Username;
                ln_UserEmail.Text = profile.Email;
                txt_Info.Text = string.IsNullOrWhiteSpace(user.Bio)
                    ? "Chưa có thông tin giới thiệu."
                    : user.Bio;

                txt_EditDisplayUser.Text = user.DisplayName;
                txt_EditUser.Text = user.Username;
                txt_EditUser.ReadOnly = true;
                txt_EditInfo.Text = user.Bio ?? "";
            }
            catch (ApiException ex)
            {
                MessageBox.Show(
                    "Không thể tải thông tin tài khoản từ WebService.\n" + ex.Message,
                    "Lỗi tải tài khoản",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi khi tải thông tin tài khoản.\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btn_TaiKhoan.Enabled = true;
            }
        }

        private void btn_BaoMat_Click(object sender, EventArgs e)
        {
            ChuyenPanelCaiDat(pnl_BaoMat);
            ChonNutCaiDat(btn_BaoMat);
        }
        private void btn_DanhSachBan_Click(object sender, EventArgs e)
        {
            ChuyenPanelBanBe(pnl_DanhSachBan);
            ChonNutBanBe(btn_DanhSachBan);
        }

        private void btn_LoiMoiKetBan_Click(object sender, EventArgs e)
        {
            ChuyenPanelBanBe(pnl_LoiMoiKetBan);
            ChonNutBanBe(btn_LoiMoiKetBan);
        }

        private void btn_DoiMatKhau_MouseEnter(object sender, EventArgs e)
        {
            btn_DoiMatKhau.BackColor = AppColors.PrimaryHover;
        }

        private void btn_DoiMatKhau_MouseLeave(object sender, EventArgs e)
        {
            btn_DoiMatKhau.BackColor = AppColors.Primary;
        }

        private void btn_DoiEmail_MouseEnter(object sender, EventArgs e)
        {
            btn_DoiEmail.BackColor = AppColors.PrimaryHover;
        }

        private void btn_DoiEmail_MouseLeave(object sender, EventArgs e)
        {
            btn_DoiEmail.BackColor = AppColors.Primary;
        }

        private async void btn_Nhom_Click(object sender, EventArgs e)
        {
            pnl_HoiThoaiCaNhan.Visible = false;
            pnl_HoiThoaiNhom.Visible = true;
            btn_Nhom.BackColor = AppColors.Selected;
            if (!globalChatLoaded)
                await LoadGlobalChatAsync();
            if (globalChatLoaded && pnl_Chat.Visible)
                globalChatTimer.Start();
        }
    }
}
