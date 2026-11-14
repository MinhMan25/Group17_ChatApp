using ChatappZalu;

namespace ClientForm
{
    public partial class Login : Form
    {
        private bool loginRequestPending = false;

        public Login()
        {
            InitializeComponent();

            this.BackColor = AppColors.Background;
            this.FormBorderColor = AppColors.Border;

            txtbox_Password.PasswordChar = '*';
            btn_ShowPassword.Text = "🔒";

        }

        private void btn_Login_MouseEnter(object sender, EventArgs e)
        {
            btn_Login.BackColor = AppColors.PrimaryHover;
            btn_Login.ForeColor = AppColors.Selected;
        }

        private void btn_Login_MouseLeave(object sender, EventArgs e)
        {
            btn_Login.BackColor = Color.White;
            btn_Login.ForeColor = Color.Black;
        }

        private void btn_Register_MouseEnter(object sender, EventArgs e)
        {
            btn_Register.BackColor = AppColors.PrimaryHover;
            btn_Register.ForeColor = AppColors.Selected;
        }

        private void btn_Register_MouseLeave(object sender, EventArgs e)
        {
            btn_Register.BackColor = Color.White;
            btn_Register.ForeColor = Color.Black;
        }

        private void btn_Register_Click(object sender, EventArgs e)
        {
            var DangKy = new Register();
            DangKy.ShowDialog();
        }

        private void lkb_ForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var Quenmk = new ForgotPassword();
            Quenmk.ShowDialog();
        }



        private async void btn_Login_Click(object sender, EventArgs e)
        {
            string username = txtb_Username.Text.Trim();
            string password = txtbox_Password.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            // Ký tự phân cách và xuống dòng không được nằm trong dữ liệu.
            if (username.Contains("|") ||
                password.Contains("|") ||
                username.Contains("\r") ||
                username.Contains("\n") ||
                password.Contains("\r") ||
                password.Contains("\n"))
            {
                MessageBox.Show("Thông tin đăng nhập không hợp lệ!");
                return;
            }

            if (loginRequestPending)
                return;

            loginRequestPending = true;
            btn_Login.Enabled = false;

            try
            {
                await ApiClientManager.Api.LoginAsync(username, password);

                MessageBox.Show(
                    "Đăng nhập thành công!",
                    "Zalu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Hide();

                GiaoDien giaoDien = new GiaoDien();
                giaoDien.ShowDialog();

                this.Close();
            }
            catch (ApiException ex)
            {
                loginRequestPending = false;
                btn_Login.Enabled = true;

                string message = ex.Code switch
                {
                    "INVALID_CREDENTIALS" => "Tên đăng nhập hoặc mật khẩu không đúng!",
                    "CANNOT_CONNECT" => "Không kết nối được WebService tại http://127.0.0.1:5000.",
                    "TIMEOUT" => "WebService không phản hồi.",
                    _ => ex.Message
                };

                MessageBox.Show(message, "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                loginRequestPending = false;
                btn_Login.Enabled = true;

                MessageBox.Show(
                    "Không thể đăng nhập qua WebService.\n\n" +
                    ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void lb_UserName_Click(object sender, EventArgs e)
        {

        }

        private void txtb_Username_TextChanged(object sender, EventArgs e)
        {

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
    }
}
