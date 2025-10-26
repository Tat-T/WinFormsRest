using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public partial class AddEditDishForm : Form
    {
        private readonly bool isEdit;
        private readonly int? dishId;

        private readonly TextBox txtName;
        private readonly TextBox txtPrice;
        private readonly TextBox txtIngredients;
        private readonly Button btnSave;
        private readonly Button btnCancel;

        public AddEditDishForm(bool isEdit = false, int? dishId = null,
                               string? name = null, decimal? price = null, string? ingredients = null)
        {
            this.isEdit = isEdit;
            this.dishId = dishId;

            Text = isEdit ? "Редактировать блюдо" : "Добавить блюдо";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label lbl1 = new() { Text = "Название:", Left = 20, Top = 20, Width = 100 };
            txtName = new() { Left = 120, Top = 20, Width = 220, Text = name ?? "" };

            Label lbl2 = new() { Text = "Цена:", Left = 20, Top = 60, Width = 100 };
            txtPrice = new() { Left = 120, Top = 60, Width = 100, Text = price?.ToString("0.00") ?? "" };

            Label lbl3 = new() { Text = "Ингредиенты (через запятую):", Left = 20, Top = 100, Width = 250 };
            txtIngredients = new() { Left = 20, Top = 120, Width = 320, Height = 60, Multiline = true, Text = ingredients ?? "" };

            btnSave = new() { Text = "Сохранить", Left = 80, Top = 200, Width = 100 };
            btnCancel = new() { Text = "Отмена", Left = 200, Top = 200, Width = 100 };

            btnCancel.Click += (s, e) => Close();
            btnSave.Click += async (s, e) => await SaveDish();

            Controls.AddRange(new Control[] { lbl1, txtName, lbl2, txtPrice, lbl3, txtIngredients, btnSave, btnCancel });
        }

        private async Task SaveDish()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название блюда");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var price) &&
    !            decimal.TryParse(txtPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                MessageBox.Show("Некорректная цена (пример: 150,00 или 150.00)");
                return;
            }

            string name = txtName.Text.Trim();
            string ingredients = txtIngredients.Text.Trim();

            using var http = new HttpClient();

            // Формируем данные
            var priceText = price.ToString(CultureInfo.CurrentCulture);

            var form = new MultipartFormDataContent
            {
                { new StringContent(txtName.Text), "DishName" },
                { new StringContent(priceText), "Price" },
                { new StringContent(txtIngredients.Text), "IngredientNames" }
            };

            HttpResponseMessage resp;
            string url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/menu";

            try
            {
                if (isEdit && dishId.HasValue)
                {
                    resp = await http.PutAsync($"{url}/{dishId.Value}", form);
                }
                else
                {
                    resp = await http.PostAsync(url, form);
                }

                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show(isEdit ? "Блюдо успешно обновлено" : "Блюдо успешно добавлено");
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    string msg = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при сохранении: {resp.StatusCode}\n{msg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }
    }
}
