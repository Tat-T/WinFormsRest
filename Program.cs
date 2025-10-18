//using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Collections.Generic;


//namespace WindowsAdminApp
//{
//    internal static class Program
//    {

//        [STAThread]
//        static void Main()
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            Application.Run(new LoginForm());
//        }
//    }

//    // Простая модель для ответа авторизации
//    public class AuthResponse
//    {
//        public string Token { get; set; }
//        public string Message { get; set; }
//    }

//    public static class AppConfig
//    {
//        public static string ApiBaseUrl = "http://localhost:5015";
//    }

//    // Форма логина
//    public partial class LoginForm : Form
//    {
//        private TextBox txtEmail;
//        private TextBox txtPassword;
//        private Button btnLogin;
//        private Label lblStatus;


//        public LoginForm()
//        {
//            Text = "Admin Login - Restaurant";
//            Width = 380;
//            Height = 220;
//            FormBorderStyle = FormBorderStyle.FixedDialog;
//            MaximizeBox = false;


//            var lblEmail = new Label { Text = "Email:", Left = 20, Top = 20, Width = 80 };
//            txtEmail = new TextBox { Left = 110, Top = 18, Width = 230 };


//            var lblPassword = new Label { Text = "Password:", Left = 20, Top = 60, Width = 80 };
//            txtPassword = new TextBox { Left = 110, Top = 58, Width = 230, UseSystemPasswordChar = true };


//            btnLogin = new Button { Text = "Login", Left = 110, Top = 100, Width = 100 };
//            btnLogin.Click += BtnLogin_Click;


//            lblStatus = new Label { Left = 20, Top = 140, Width = 320, Height = 40 };


//            Controls.Add(lblEmail);
//            Controls.Add(txtEmail);
//            Controls.Add(lblPassword);
//            Controls.Add(txtPassword);
//            Controls.Add(btnLogin);
//            Controls.Add(lblStatus);
//        }

//        private async void BtnLogin_Click(object sender, EventArgs e)
//        {
//            btnLogin.Enabled = false;
//            lblStatus.Text = "Авторизация...";
//            var email = txtEmail.Text.Trim();
//            var password = txtPassword.Text;


//            try
//            {
//                var token = await Authenticate(email, password);
//                if (!string.IsNullOrEmpty(token))
//                {
//                    lblStatus.Text = "Успешно. Открытие панели администратора...";
//                    Hide();
//                    var adminForm = new AdminForm(token);
//                    adminForm.FormClosed += (s, args) => this.Close();
//                    adminForm.Show();
//                }
//                else
//                {
//                    lblStatus.Text = "Ошибка авторизации. Проверьте email и пароль.";
//                }
//            }
//            catch (Exception ex)
//            {
//                lblStatus.Text = "Ошибка: " + ex.Message;
//            }
//            finally
//            {
//                btnLogin.Enabled = true;
//            }
//        }

//        private async Task<string> Authenticate(string email, string password)
//        {
//            using var http = new HttpClient();
//            var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/auth/login";

//            var body = new { email = email, password = password };
//            var json = JsonSerializer.Serialize(body);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var resp = await http.PostAsync(url, content);
//            var respJson = await resp.Content.ReadAsStringAsync();

//            if (!resp.IsSuccessStatusCode)
//            {
//                lblStatus.Text = "Ошибка: " + respJson;
//                return null;
//            }

//            using var doc = JsonDocument.Parse(respJson);
//            var root = doc.RootElement;

//            string role = root.TryGetProperty("role", out var r) ? r.GetString() : null;
//            string message = root.TryGetProperty("message", out var m) ? m.GetString() : null;

//            // Разрешаем вход только админу
//            if (role == "Admin")
//                return "ok"; // условный токен

//            lblStatus.Text = message ?? "Нет прав администратора";
//            return null;
//        }

//    }

//    public partial class AdminForm : Form
//    {
//        private string token;
//        private TabControl tabControl;
//        private Button btnRefreshUsers;
//        private ListView lvUsers;


//        private Button btnRefreshMenu;
//        private ListView lvMenu;


//        private Button btnRefreshReservations;
//        private ListView lvReservations;
//        public AdminForm(string jwtToken)
//        {
//            token = jwtToken;
//            Text = "Admin Panel - Restaurant";
//            Width = 1200;
//            Height = 600;


//            tabControl = new TabControl { Dock = DockStyle.Fill };


//            // Users tab
//            var tabUsers = new TabPage("Users");
//            btnRefreshUsers = new Button { Text = "Refresh", Left = 10, Top = 10, Width = 80 };
//            btnRefreshUsers.Click += async (s, e) => await LoadUsers();
//            lvUsers = new ListView { Left = 10, Top = 50, Width = 1150, Height = 440, View = View.Details, FullRowSelect = true };
//            lvUsers.Columns.Add("Id", 50);
//            lvUsers.Columns.Add("Фамилия", 100);
//            lvUsers.Columns.Add("Имя", 100);
//            lvUsers.Columns.Add("Отчество", 100);
//            lvUsers.Columns.Add("Логин", 100);
//            lvUsers.Columns.Add("Email", 150);
//            lvUsers.Columns.Add("Телефон", 100);
//            lvUsers.Columns.Add("Дата рождения", 100);
//            lvUsers.Columns.Add("Дата создания", 100);
//            //lvUsers.Columns.Add("Name", 150);
//            lvUsers.Columns.Add("Роль", 50);
//            lvUsers.Columns.Add("Активен", 80);
//            lvUsers.Columns.Add("", 100);
//            tabUsers.Controls.Add(btnRefreshUsers);
//            tabUsers.Controls.Add(lvUsers);


//            // Menu tab
//            var tabMenu = new TabPage("Menu");
//            btnRefreshMenu = new Button { Text = "Refresh", Left = 10, Top = 10, Width = 80 };
//            btnRefreshMenu.Click += async (s, e) => await LoadMenu();
//            lvMenu = new ListView { Left = 10, Top = 50, Width = 840, Height = 440, View = View.Details, FullRowSelect = true };
//            lvMenu.Columns.Add("Id", 80);
//            lvMenu.Columns.Add("Name", 260);
//            lvMenu.Columns.Add("Price", 100);
//            lvMenu.Columns.Add("Description", 360);
//            tabMenu.Controls.Add(btnRefreshMenu);
//            tabMenu.Controls.Add(lvMenu);


//            // Reservations tab
//            var tabReservations = new TabPage("Reservations");
//            btnRefreshReservations = new Button { Text = "Refresh", Left = 10, Top = 10, Width = 80 };
//            btnRefreshReservations.Click += async (s, e) => await LoadReservations();
//            lvReservations = new ListView { Left = 10, Top = 50, Width = 840, Height = 440, View = View.Details, FullRowSelect = true };
//            lvReservations.Columns.Add("Id", 80);
//            lvReservations.Columns.Add("User", 200);
//            lvReservations.Columns.Add("Date", 200);
//            lvReservations.Columns.Add("Guests", 80);
//            lvReservations.Columns.Add("Status", 200);
//            tabReservations.Controls.Add(btnRefreshReservations);
//            tabReservations.Controls.Add(lvReservations);


//            tabControl.TabPages.Add(tabUsers);
//            tabControl.TabPages.Add(tabMenu);
//            tabControl.TabPages.Add(tabReservations);


//            Controls.Add(tabControl);


//            Load += async (s, e) => await LoadUsers();
//        }

//        private HttpClient CreateHttpClient()
//        {
//            var http = new HttpClient();
//            http.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
//            if (!string.IsNullOrEmpty(token))
//            {
//                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//            }
//            return http;
//        }

//        private async Task LoadUsers()
//        {
//            try
//            {
//                btnRefreshUsers.Enabled = false;
//                lvUsers.Items.Clear();

//                using var http = CreateHttpClient();
//                var resp = await http.GetAsync("/api/users");
//                if (!resp.IsSuccessStatusCode)
//                {
//                    MessageBox.Show("Не удалось получить список пользователей. Статус: " + resp.StatusCode);
//                    return;
//                }

//                var json = await resp.Content.ReadAsStringAsync();
//                var users = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                // 💡 создаём колонки (если их нет)
//                if (lvUsers.Columns.Count == 0)
//                {
//                    //lvUsers.Columns.Clear();
//                    lvUsers.Columns.Add("Id", 50);
//                    lvUsers.Columns.Add("SurName", 100);
//                    lvUsers.Columns.Add("Name", 100);
//                    lvUsers.Columns.Add("Patronomic", 100);
//                    lvUsers.Columns.Add("Login", 100);
//                    lvUsers.Columns.Add("Email", 150);
//                    lvUsers.Columns.Add("PhoneNumber", 100);
//                    lvUsers.Columns.Add("Birthdate", 100);
//                    lvUsers.Columns.Add("CreationDate", 100);
//                    lvUsers.Columns.Add("Role", 50);
//                    lvUsers.Columns.Add("Active", 50);
//                    lvUsers.Columns.Add("", 100);
//                }

//                // 💡 заполняем данными
//                foreach (var u in users)
//                {
//                    //var fullName = $"{u.SurName} {u.Name} {u.Patronomic}".Trim();
//                    var item = new ListViewItem(u.Id.ToString());
//                    //item.SubItems.Add(fullName);
//                    item.SubItems.Add(u.SurName);
//                    item.SubItems.Add(u.Name);
//                    item.SubItems.Add(u.Patronomic);
//                    item.SubItems.Add(u.UserName);
//                    item.SubItems.Add(u.Email ?? "");
//                    item.SubItems.Add(u.PhoneNumber);
//                    item.SubItems.Add(u.Birthdate?.ToString("d"));
//                    item.SubItems.Add(u.CreationDate.ToString("g") ?? "");
//                    item.SubItems.Add(u.UserRole?.Name ?? "—"); 
//                    item.SubItems.Add(u.IsActive ? "Да" : "Нет");
//                    lvUsers.Items.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Ошибка: " + ex.Message);
//            }
//            finally
//            {
//                btnRefreshUsers.Enabled = true;
//            }
//        }


//        private async Task LoadMenu()
//        {
//            try
//            {
//                btnRefreshMenu.Enabled = false;
//                lvMenu.Items.Clear();
//                using var http = CreateHttpClient();
//                var resp = await http.GetAsync("/api/menu");
//                if (!resp.IsSuccessStatusCode)
//                {
//                    MessageBox.Show("Не удалось получить меню. Статус: " + resp.StatusCode);
//                    return;
//                }
//                var json = await resp.Content.ReadAsStringAsync();
//                var items = JsonSerializer.Deserialize<List<MenuDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
//                foreach (var m in items)
//                {
//                    var item = new ListViewItem(m.Id.ToString());
//                    item.SubItems.Add(m.Name ?? "");
//                    item.SubItems.Add(m.Price?.ToString() ?? "");
//                    item.SubItems.Add(m.Description ?? "");
//                    lvMenu.Items.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Ошибка: " + ex.Message);
//            }
//            finally
//            {
//                btnRefreshMenu.Enabled = true;
//            }
//        }

//        private async Task LoadReservations()
//        {
//            try
//            {
//                btnRefreshReservations.Enabled = false;
//                lvReservations.Items.Clear();
//                using var http = CreateHttpClient();
//                var resp = await http.GetAsync("/api/reservations");
//                if (!resp.IsSuccessStatusCode)
//                {
//                    MessageBox.Show("Не удалось получить бронирования. Статус: " + resp.StatusCode);
//                    return;
//                }
//                var json = await resp.Content.ReadAsStringAsync();
//                var items = JsonSerializer.Deserialize<List<ReservationDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
//                foreach (var r in items)
//                {
//                    var item = new ListViewItem(r.Id.ToString());
//                    item.SubItems.Add(r.UserEmail ?? "");
//                    item.SubItems.Add(r.Date?.ToString("g") ?? "");
//                    item.SubItems.Add(r.Guests.ToString());
//                    item.SubItems.Add(r.Status ?? "");
//                    lvReservations.Items.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Ошибка: " + ex.Message);
//            }
//            finally
//            {
//                btnRefreshReservations.Enabled = true;
//            }
//        }

//        // DTO для пользователя
//        // DTO GetAllUsers
//        private class UserDto
//        {
//            public int Id { get; set; }
//            public string SurName { get; set; }
//            public string Name { get; set; }
//            public string Patronomic { get; set; }
//            public string UserName { get; set; }
//            public string Email { get; set; }
//            public string PhoneNumber { get; set; }
//            public DateTime? Birthdate { get; set; }
//            public DateTime CreationDate { get; set; }

//            public bool IsActive { get; set; }

//            public UserRoleDto UserRole { get; set; }
//        }

//        private class UserRoleDto
//        {
//            public int Id { get; set; }
//            public string Name { get; set; }
//        }


//        private class MenuDto { public int Id { get; set; } public string Name { get; set; } public decimal? Price { get; set; } public string Description { get; set; } }
//        private class ReservationDto { public int Id { get; set; } public string UserEmail { get; set; } public DateTime? Date { get; set; } public int Guests { get; set; } public string Status { get; set; } }
//    }
//}

using System;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запуск окна авторизации
            Application.Run(new LoginForm());
        }
    }
}