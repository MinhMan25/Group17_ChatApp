using ChatappZalu;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ClientForm
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            this.BackColor = AppColors.Background;
            this.FormBorderColor = AppColors.Border;

        }

        private void btn_Register_MouseEnter(object sender, EventArgs e)
        {
            btn_Register.BackColor = AppColors.Primary;
            btn_Register.ForeColor = Color.White;
        }

        private void btn_Register_MouseLeave(object sender, EventArgs e)
        {
            btn_Register.BackColor = Color.White;
            btn_Register.ForeColor = Color.Black;
        }

        private void lkb_Register_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private string KiemTraMatKhau(string matKhau) //dành cho 3 cái kiểm tra, hợp lệ thỉ trả về "", không thì báo lỗi
        {
            if (matKhau == "")
                return "\nVui lòng nhập mật khẩu";
            bool du8KyTu = matKhau.Length >= 8;
            bool coChuSo = matKhau.Any(char.IsDigit);
            bool coChuHoa = matKhau.Any(char.IsUpper);
            bool coKyTuDacBiet = matKhau.Any(c =>
                !char.IsLetterOrDigit(c));
            if (!(du8KyTu && coChuSo && coChuHoa && coKyTuDacBiet))
                return "\nMật khẩu không đúng yêu cầu hãy chọn lại";
            else
                return "";
        }
        private string KiemtraUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                return "Tên tài khoản không được để trống.";

            if (user.Contains("|") || user.Contains("\r") ||
                user.Contains("\n"))
                return "Tên tài khoản chứa ký tự không hợp lệ.";

            return "";
        }

        private string KiemtraEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email không được để trống.";

            if (email.Contains("|") || email.Contains("\r") ||
                email.Contains("\n"))
                return "Email chứa ký tự không hợp lệ.";

            return "";
        }
        private async void btn_Register_Click(object sender, EventArgs e)
        {
            string username = txtbox_Username.Text.Trim();
            string email = txtbox_Email.Text.Trim();
            string password = txtbox_Password.Text;

            string check = KiemtraUser(username) + KiemtraEmail(email) + KiemTraMatKhau(password);
            lb_SaiDinhDang.Text = check;
            if (!string.IsNullOrEmpty(check))
                return;

            btn_Register.Enabled = false;

            try
            {
                await ApiClientManager.Api.RegisterAsync(username, email, password);
                MessageBox.Show(
                    "Đăng ký tài khoản thành công!",
                    "Zalu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (ApiException ex)
            {
                string errorMessage = ex.Code switch
                {
                    "USERNAME_EXISTS" => "Tên tài khoản đã tồn tại.",
                    "EMAIL_EXISTS" => "Email đã được sử dụng.",
                    "INVALID_USERNAME" or "INVALID_EMAIL" or "INVALID_PASSWORD" => ex.Message,
                    "CANNOT_CONNECT" => "Không kết nối được WebService tại http://127.0.0.1:5000.",
                    "TIMEOUT" => "WebService không phản hồi.",
                    _ => ex.Message
                };

                MessageBox.Show(errorMessage, "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể đăng ký qua WebService.\n" + ex.Message,
                    "Zalu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btn_Register.Enabled = true;
            }
        }

        private void btn_ShowPassword_Click(object sender, EventArgs e)
        {
            if (txtbox_Password.PasswordChar == '*')
            {
                txtbox_Password.PasswordChar = '\0';
                btn_ShowPassword.Text = "🔓";
            }
            else
            {
                txtbox_Password.PasswordChar = '*';
                btn_ShowPassword.Text = "🔒";
            }
        }

        private void txtbox_Username_TextChanged(object sender, EventArgs e)
        {
            txtbox_Username.MaxLength = 20;
        }

        private void txtbox_Email_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtbox_Password_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
