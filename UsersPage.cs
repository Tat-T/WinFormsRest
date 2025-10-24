using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public class UsersPage : UserControl
    {
        private readonly Button btnRefresh;
        private readonly Button btnAdd;
        private readonly Button btnEdit;
        private readonly Button btnDelete;
        private readonly ListView lvUsers;

        public UsersPage()
        {
            Dock = DockStyle.Fill;

            // Кнопки
            btnRefresh = new Button { Text = "Обновить", Left = 10, Top = 10, Width = 100 };
            btnRefresh.Click += async (s, e) => await LoadUsers();

            btnAdd = new Button { Text = "Добавить", Left = 120, Top = 10, Width = 100 };
            btnAdd.Click += (s, e) => AddUser();

            btnEdit = new Button { Text = "Редактировать", Left = 230, Top = 10, Width = 100 };
            btnEdit.Click += (s, e) => EditUser();

            btnDelete = new Button { Text = "Удалить", Left = 340, Top = 10, Width = 100 };
            btnDelete.Click += async (s, e) => await DeleteUser();

            // Список пользователей
            lvUsers = new ListView
            {
                Left = 10,
                Top = 50,
                Width = 1150,
                Height = 500,
                View = View.Details,
                FullRowSelect = true
            };

            // Колонки
            lvUsers.Columns.Add("ID", 40);
            lvUsers.Columns.Add("Фамилия", 100);
            lvUsers.Columns.Add("Имя", 100);
            lvUsers.Columns.Add("Отчество", 100);
            lvUsers.Columns.Add("Логин", 100);
            lvUsers.Columns.Add("Email", 130);
            lvUsers.Columns.Add("Телефон", 100);
            lvUsers.Columns.Add("Дата рождения", 100);
            lvUsers.Columns.Add("Дата создания", 100);
            lvUsers.Columns.Add("Роль", 50);
            lvUsers.Columns.Add("Активен", 60);

            Controls.Add(btnRefresh);
            Controls.Add(btnAdd);
            Controls.Add(btnEdit);
            Controls.Add(btnDelete);
            Controls.Add(lvUsers);

            _ = LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                btnRefresh.Enabled = false;
                lvUsers.Items.Clear();

                using var http = new HttpClient();
                var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/users";

                var resp = await http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ошибка при получении пользователей. Код: " + resp.StatusCode);
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var u in users)
                {
                    var item = new ListViewItem(u.Id.ToString());
                    item.SubItems.Add(u.SurName ?? "");
                    item.SubItems.Add(u.Name ?? "");
                    item.SubItems.Add(u.Patronomic ?? "");
                    item.SubItems.Add(u.UserName ?? "");
                    item.SubItems.Add(u.Email ?? "");
                    item.SubItems.Add(u.PhoneNumber ?? "");
                    item.SubItems.Add(u.Birthdate?.ToString("d") ?? "");
                    item.SubItems.Add(u.CreationDate.ToString("g"));
                    item.SubItems.Add(u.UserRole?.Name ?? "—");
                    item.SubItems.Add(u.IsActive ? "Да" : "Нет");

                    lvUsers.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void AddUser()
        {
            using var form = new UserEditForm();
            if (form.ShowDialog() == DialogResult.OK)
                _ = LoadUsers();
        }

        private void EditUser()
        {
            if (lvUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для редактирования.");
                return;
            }

            var id = int.Parse(lvUsers.SelectedItems[0].Text);
            using var form = new UserEditForm(id);
            if (form.ShowDialog() == DialogResult.OK)
                _ = LoadUsers();
        }

        private async Task DeleteUser()
        {
            if (lvUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            var id = int.Parse(lvUsers.SelectedItems[0].Text);
            if (MessageBox.Show("Удалить пользователя?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            using var http = new HttpClient();
            var resp = await http.DeleteAsync($"{AppConfig.ApiBaseUrl.TrimEnd('/')}/api/users/{id}");
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show("Пользователь удалён");
                await LoadUsers();
            }
            else
            {
                MessageBox.Show("Ошибка при удалении пользователя");
            }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string SurName { get; set; }
            public string Name { get; set; }
            public string Patronomic { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime? Birthdate { get; set; }
            public DateTime CreationDate { get; set; }
            public bool IsActive { get; set; }
            public UserRoleDto UserRole { get; set; }
        }

        private class UserRoleDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
