using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public class MenuPage : UserControl
    {
        private readonly Button btnRefresh, btnAdd, btnEdit, btnDelete;
        private readonly ListView lvMenu;
        private List<MenuItemDto> currentMenu = new();

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

            btnAdd = new Button { Text = "Добавить", Left = 120, Top = 10, Width = 100 };
            btnAdd.Click += (s, e) => AddMenu();

            btnEdit = new Button { Text = "Редактировать", Left = 230, Top = 10, Width = 100 };
            btnEdit.Click += (s, e) => EditMenu();

            btnDelete = new Button { Text = "Удалить", Left = 340, Top = 10, Width = 100 };
            btnDelete.Click += async (s, e) => await DeleteMenu();

            lvMenu = new ListView
            {
                Left = 10,
                Top = 50,
                Width = 1150,
                Height = 500,
                View = View.Details,
                FullRowSelect = true
            };

            lvMenu.Columns.Add("Фото", 300);
            lvMenu.Columns.Add("Название блюда", 150);
            lvMenu.Columns.Add("Ингредиенты", 300);
            lvMenu.Columns.Add("Цена", 80);

            Controls.AddRange (new Control [] { btnRefresh, btnAdd, btnEdit, btnDelete});
            Controls.Add(lvMenu);

            // Загружаем при открытии
            _ = LoadMenu();
        }

        private async Task DeleteMenu()
        {
            if (lvMenu.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для удаления");
                return;
            }

            var item = lvMenu.SelectedItems[0];
            int index = item.Index;
            var dish = currentMenu[index];

            var confirm = MessageBox.Show($"Удалить блюдо \"{dish.DishName}\"?", "Подтверждение", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using var http = new HttpClient();
            var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/menu/" + dish.DishID;

            var resp = await http.DeleteAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show("Блюдо удалено");
                await LoadMenu();
            }
            else
            {
                MessageBox.Show("Ошибка удаления: " + resp.StatusCode);
            }
        }

        private void EditMenu()
        {
            if (lvMenu.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите блюдо для редактирования");
                return;
            }

            var item = lvMenu.SelectedItems[0];
            int index = item.Index;
            var dish = currentMenu[index];

            using var form = new AddEditDishForm(
                isEdit: true,
                dishId: dish.DishID,
                name: dish.DishName,
                price: dish.Price,
                ingredients: string.Join(", ", dish.Ingredients),
                dishImageUrl: dish.DishImage
            );


            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadMenu();
            }

        }

        private void AddMenu()
        {
            using var form = new AddEditDishForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadMenu();
            }
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
                var items = JsonSerializer.Deserialize<List<MenuItemDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var m in items)
                {
                    var item = new ListViewItem(m.DishImage ?? "");
                    item.SubItems.Add(m.DishName ?? "");

                    // ingredientsText собираем из списка строк Ingredients
                    string ingredientsText = "";
                    if (m.Ingredients != null && m.Ingredients.Count > 0)
                    {
                        ingredientsText = string.Join(", ", m.Ingredients);
                    }
                    item.SubItems.Add(ingredientsText);

                    item.SubItems.Add(m.Price?.ToString("0.00") ?? "");
                    lvMenu.Items.Add(item);
                }
                currentMenu = items ?? new List<MenuItemDto>();
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

        private class MenuItemDto
        {
            public int DishID { get; set; }
            public string? DishImage { get; set; }
            public string DishName { get; set; } = string.Empty;
            public decimal? Price { get; set; }
            public List<string> Ingredients { get; set; } = new();
        }
    }
}
