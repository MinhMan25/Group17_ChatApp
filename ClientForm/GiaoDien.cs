using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class GiaoDien : Form
    {
        public GiaoDien()
        {
            InitializeComponent();
            this.BackColor = AppColors.Background;
<<<<<<< Updated upstream
            ChonNut(btn_Chat);
            ChuyenPanel(pnl_Chat);
=======
            ChonNutMenu(btn_Chat);
            ChuyenPanelMenu(pnl_Chat);

            HienThiPhongChatTong();
>>>>>>> Stashed changes
        }

        private void ChonNut(Button nutDuocChon)
        {
            Button[] danhSachNut =
            {
                btn_Chat, btn_Friend, btn_GroupChat, btn_Setting
            };
            foreach (Button nut in danhSachNut)
            {
                nut.BackColor = AppColors.Surface;
                nut.ForeColor = AppColors.TextSecondary;
            }
            nutDuocChon.BackColor = AppColors.Selected;
            nutDuocChon.ForeColor = AppColors.Primary;
        }
        private void ChuyenPanel(Panel PanelDuocChon)
        {
            Panel[] danhSachpanel =
            {
                pnl_Chat, pnl_Friend, pnl_GroupChat, pnl_Setting
            };
            foreach(Panel pnl in danhSachpanel)
            {
                pnl.Visible = false;
            }
            PanelDuocChon.Visible = true;
        }
        private void btn_Chat_Click(object sender, EventArgs e)
        {
            ChonNut(btn_Chat);
            ChuyenPanel(pnl_Chat);
        }

        private void btn_Friend_Click(object sender, EventArgs e)
        {
            ChonNut(btn_Friend);
            ChuyenPanel(pnl_Friend);
        }

        private void btn_GroupChat_Click(object sender, EventArgs e)
        {
            ChonNut(btn_GroupChat);
            ChuyenPanel(pnl_GroupChat);
        }

        private void btn_Setting_Click(object sender, EventArgs e)
        {
            ChonNut(btn_Setting);
            ChuyenPanel(pnl_Setting);
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
<<<<<<< Updated upstream
=======

        private void pnl_Chatting_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnl_List_Paint(object sender, PaintEventArgs e)
        {

        }



        private async void btn_LogOut_Click(object sender, EventArgs e)
        {
            var client = ChatClientManager.Client;

            if (logoutRequestPending)
                return;

            if (!client.IsConnected)
            {
                MessageBox.Show(
                    "Không có kết nối đến server. Không thể xác nhận đăng xuất.",
                    "Zalu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            logoutRequestPending = true;
            btn_LogOut.Enabled = false;

            try
            {
                await client.SendLineAsync("LOGOUT");
            }
            catch (Exception ex)
            {
                logoutRequestPending = false;
                btn_LogOut.Enabled = true;

                MessageBox.Show(
                    "Gửi yêu cầu đăng xuất thất bại.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void HienThiPhongChatTong() //mô tả nó sẽ như nào thôi. Chờ làm nút này.
        {
            // Tạo mục hiển thị phòng chat.
            Panel item = new Panel();

            item.Width = flp_DanhSachChat.ClientSize.Width - 5;
            item.Height = 65;
            item.BackColor = Color.White;
            item.Margin = new Padding(0, 0, 0, 5);
            item.Cursor = Cursors.Hand;

            // Tên phòng chat.
            Label lbTenPhong = new Label();

            lbTenPhong.Text = "Nhóm chat tổng";
            lbTenPhong.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbTenPhong.Location = new Point(15, 10);
            lbTenPhong.AutoSize = true;

            // Mô tả phòng chat.
            Label lbMoTa = new Label();

            lbMoTa.Text = "Tất cả mọi người";
            lbMoTa.Font = new Font("Segoe UI", 8);
            lbMoTa.ForeColor = Color.Gray;
            lbMoTa.Location = new Point(15, 35);
            lbMoTa.AutoSize = true;

            item.Controls.Add(lbTenPhong);
            item.Controls.Add(lbMoTa);

            // Thêm mục vào danh sách hội thoại.
            flp_DanhSachChat.Controls.Add(item);
        }

        private void btn_Gui_MouseEnter(object sender, EventArgs e)
        {
            btn_Gui.BackColor = AppColors.PrimaryHover;
        }

        private void btn_Gui_MouseLeave(object sender, EventArgs e)
        {
            btn_Gui.BackColor = AppColors.Primary;
        }

        private void btn_TaiKhoan_Click(object sender, EventArgs e)
        {
            ChuyenPanelCaiDat(pnl_TaiKhoan);
            ChonNutCaiDat(btn_TaiKhoan);
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
>>>>>>> Stashed changes
    }
}
