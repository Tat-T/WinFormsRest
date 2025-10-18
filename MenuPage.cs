using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public class MenuPage : UserControl
    {
        private readonly Button btnRefresh;
        private readonly ListView lvMenu;

        public MenuPage()
        {
            Dock = DockStyle.Fill;

            btnRefresh = new Button
            {
                Text = "Обновить",
                Left = 10,
                Top = 10,
                Width = 100
            };
            btnRefresh.Click += async (s, e) => await LoadMenu();

            lvMenu = new ListView
            {
                Left = 10,
                Top = 50,
                Width = 1100,
                Height = 500,
                View = View.Details,
                FullRowSelect = true
            };

            lvMenu.Columns.Add("ID", 60);
            lvMenu.Columns.Add("Название", 250);
            lvMenu.Columns.Add("Цена", 100);
            lvMenu.Columns.Add("Описание", 600);

            Controls.Add(btnRefresh);
            Controls.Add(lvMenu);

            // Загружаем при открытии
            _ = LoadMenu();
        }

        private async Task LoadMenu()
        {
            try
            {
                btnRefresh.Enabled = false;
                lvMenu.Items.Clear();

                using var http = new HttpClient();
                var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/menu";
                var resp = await http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Не удалось получить меню. Код: " + resp.StatusCode);
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<MenuDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var m in items)
                {
                    var item = new ListViewItem(m.Id.ToString());
                    item.SubItems.Add(m.Name ?? "");
                    item.SubItems.Add(m.Price?.ToString("0.00") ?? "");
                    item.SubItems.Add(m.Description ?? "");
                    lvMenu.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки меню: " + ex.Message);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private class MenuDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal? Price { get; set; }
            public string Description { get; set; }
        }
    }
}
