using System;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public partial class LoginForm : Form
    {
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblStatus;

        public LoginForm()
        {
            Text = "Admin Login - Restaurant";
            Width = 380;
            Height = 220;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblEmail = new Label { Text = "Email:", Left = 20, Top = 20, Width = 80 };
            txtEmail = new TextBox { Left = 110, Top = 18, Width = 230 };

            var lblPassword = new Label { Text = "Password:", Left = 20, Top = 60, Width = 80 };
            txtPassword = new TextBox { Left = 110, Top = 58, Width = 230, UseSystemPasswordChar = true };

            btnLogin = new Button { Text = "Login", Left = 110, Top = 100, Width = 100 };
            btnLogin.Click += BtnLogin_Click;

            lblStatus = new Label { Left = 20, Top = 140, Width = 320, Height = 40 };

            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblStatus);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblStatus.Text = "Вход...";

            // Простейшая проверка (можно убрать совсем или добавить локальную проверку)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblStatus.Text = "Успешно. Открытие панели администратора...";
                Hide();

                // Передаём AdminForm без токена
                var adminForm = new AdminForm();
                adminForm.FormClosed += (s, args) => this.Close();
                adminForm.Show();
            }
            else
            {
                lblStatus.Text = "Введите Email и пароль.";
            }

            btnLogin.Enabled = true;
        }
    }

    public static class AppConfig
    {
        public static string ApiBaseUrl = "http://localhost:5015";
    }
}
