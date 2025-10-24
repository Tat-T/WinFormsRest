using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public class UserEditForm : Form
    {
        private TextBox txtSurName, txtName, txtPatronomic, txtUserName, txtPassword, txtEmail, txtPhone;
        private DateTimePicker dtpBirthdate;
        private ComboBox cbRole;
        private CheckBox chkIsActive;
        private Button btnSave;
        private int? userId;
        private List<RoleDto> roles = new List<RoleDto>();

        public UserEditForm(int? id = null)
        {
            userId = id;
            Text = id.HasValue ? "Редактирование пользователя" : "Добавление пользователя";
            Width = 400; Height = 450; FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;

            int top = 20, labelWidth = 100, controlWidth = 250, spacing = 30;

            void AddLabel(string text, int y) => Controls.Add(new Label { Text = text, Left = 20, Top = y, Width = labelWidth });
            void AddControl(Control ctrl, int y) { ctrl.Left = 130; ctrl.Top = y; ctrl.Width = controlWidth; Controls.Add(ctrl); }

            AddLabel("Фамилия:", top); txtSurName = new TextBox(); AddControl(txtSurName, top);
            top += spacing; AddLabel("Имя:", top); txtName = new TextBox(); AddControl(txtName, top);
            top += spacing; AddLabel("Отчество:", top); txtPatronomic = new TextBox(); AddControl(txtPatronomic, top);
            top += spacing; AddLabel("Логин:", top); txtUserName = new TextBox(); AddControl(txtUserName, top);
            top += spacing; AddLabel("Пароль:", top); txtPassword = new TextBox { UseSystemPasswordChar = true }; AddControl(txtPassword, top);
            top += spacing; AddLabel("Email:", top); txtEmail = new TextBox(); AddControl(txtEmail, top);
            top += spacing; AddLabel("Телефон:", top); txtPhone = new TextBox(); AddControl(txtPhone, top);
            top += spacing; AddLabel("Дата рождения:", top); dtpBirthdate = new DateTimePicker { Format = DateTimePickerFormat.Short }; AddControl(dtpBirthdate, top);
            top += spacing; AddLabel("Роль:", top); cbRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; AddControl(cbRole, top);
            top += spacing; AddLabel("Активен:", top); chkIsActive = new CheckBox(); AddControl(chkIsActive, top);

            top += spacing + 10; btnSave = new Button { Text = "Сохранить", Left = 130, Top = top, Width = 100 };
            btnSave.Click += async (s, e) => await SaveUser(); Controls.Add(btnSave);

            LoadRoles().GetAwaiter().GetResult();
            if (userId.HasValue) LoadUser(userId.Value).GetAwaiter().GetResult();
        }

        private async Task LoadRoles()
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync(AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/users/roles");
            if (!resp.IsSuccessStatusCode) return;
            var json = await resp.Content.ReadAsStringAsync();
            roles = JsonSerializer.Deserialize<List<RoleDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            cbRole.Items.Clear();
            foreach (var r in roles) cbRole.Items.Add(new ComboBoxItem(r.Name, r.Id));
            if (cbRole.Items.Count > 0) cbRole.SelectedIndex = 0;
        }

        private async Task LoadUser(int id)
        {
            using var http = new HttpClient();
            var resp = await http.GetAsync($"{AppConfig.ApiBaseUrl.TrimEnd('/')}/api/users/{id}");
            if (!resp.IsSuccessStatusCode) return;
            var json = await resp.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            txtSurName.Text = user.SurName; txtName.Text = user.Name; txtPatronomic.Text = user.Patronomic;
            txtUserName.Text = user.UserName; txtEmail.Text = user.Email; txtPhone.Text = user.PhoneNumber;
            dtpBirthdate.Value = user.Birthdate ?? DateTime.Now; chkIsActive.Checked = user.IsActive;

            for (int i = 0; i < cbRole.Items.Count; i++)
            {
                if (((ComboBoxItem)cbRole.Items[i]).Value == user.UserRole?.Id) { cbRole.SelectedIndex = i; break; }
            }
        }

        private async Task SaveUser()
        {
            btnSave.Enabled = false;
            var dto = new
            {
                SurName = txtSurName.Text,
                Name = txtName.Text,
                Patronomic = txtPatronomic.Text,
                UserName = txtUserName.Text,
                Password = txtPassword.Text,
                Email = txtEmail.Text,
                PhoneNumber = txtPhone.Text,
                Birthdate = dtpBirthdate.Value,
                IdRole = ((ComboBoxItem)cbRole.SelectedItem).Value,
                IsActive = chkIsActive.Checked
            };

            using var http = new HttpClient();
            HttpResponseMessage resp;

            if (userId.HasValue)
            {
                var json = JsonSerializer.Serialize(new UpdateUserDto
                {
                    SurName = dto.SurName,
                    Name = dto.Name,
                    Patronomic = dto.Patronomic,
                    UserName = dto.UserName,
                    NewPassword = string.IsNullOrWhiteSpace(dto.Password) ? null : dto.Password,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Birthdate = dto.Birthdate,
                    IdRole = dto.IdRole,
                    IsActive = dto.IsActive
                });
                resp = await http.PutAsync($"{AppConfig.ApiBaseUrl.TrimEnd('/')}/api/users/{userId.Value}", new StringContent(json, Encoding.UTF8, "application/json"));
            }
            else
            {
                var json = JsonSerializer.Serialize(dto);
                resp = await http.PostAsync($"{AppConfig.ApiBaseUrl.TrimEnd('/')}/api/users", new StringContent(json, Encoding.UTF8, "application/json"));
            }

            if (resp.IsSuccessStatusCode) { MessageBox.Show("Сохранено"); DialogResult = DialogResult.OK; Close(); }
            else MessageBox.Show("Ошибка при сохранении");

            btnSave.Enabled = true;
        }

        private class RoleDto { public int Id; public string Name; }
        private class ComboBoxItem { public string Text; public int Value; public ComboBoxItem(string t, int v) { Text = t; Value = v; } public override string ToString() => Text; }
        private class UserDto { public int Id; public string SurName, Name, Patronomic, UserName, Email, PhoneNumber; public DateTime? Birthdate; public bool IsActive; public RoleDto UserRole; }
        private class UpdateUserDto { public string SurName, Name, Patronomic, UserName, NewPassword, Email, PhoneNumber; public DateTime? Birthdate; public int IdRole; public bool IsActive; }
    }
}
