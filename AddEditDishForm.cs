using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public partial class AddEditDishForm : Form
    {
        private readonly bool isEdit;
        private readonly int? dishId;
        private readonly string? existingImageUrl;

        private readonly TextBox txtName;
        private readonly TextBox txtPrice;
        private readonly TextBox txtIngredients;
        private readonly Button btnSave;
        private readonly Button btnCancel;
        private readonly PictureBox pbImage;
        private readonly Button btnChooseImage;
        private readonly Button btnDeleteImage;

        private string? selectedImagePath;
        private bool removeExistingImage = false; // флаг удаления старого фото

        public AddEditDishForm(
            bool isEdit = false,
            int? dishId = null,
            string? name = null,
            decimal? price = null,
            string? ingredients = null,
            string? dishImageUrl = null)
        {
            this.isEdit = isEdit;
            this.dishId = dishId;
            this.existingImageUrl = dishImageUrl;

            Text = isEdit ? "Редактировать блюдо" : "Добавить блюдо";
            Width = 480;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;

            Label lbl1 = new() { Text = "Название:", Left = 20, Top = 20 };
            txtName = new() { Left = 120, Top = 20, Width = 320, Text = name ?? "" };

            Label lbl2 = new() { Text = "Цена:", Left = 20, Top = 60 };
            txtPrice = new() { Left = 120, Top = 60, Width = 100, Text = price?.ToString("0.00") ?? "" };

            Label lbl3 = new() { Text = "Ингредиенты (через запятую):", Left = 20, Top = 100 };
            txtIngredients = new() { Left = 20, Top = 120, Width = 420, Height = 60, Multiline = true, Text = ingredients ?? "" };

            Label lbl4 = new() { Text = "Фото блюда:", Left = 20, Top = 200 };
            pbImage = new()
            {
                Left = 120,
                Top = 200,
                Width = 150,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnChooseImage = new()
            {
                Text = "Выбрать фото...",
                Left = 280,
                Top = 200,
                Width = 120
            };
            btnChooseImage.Click += (s, e) => ChooseImage();

            btnDeleteImage = new()
            {
                Text = "Удалить фото",
                Left = 280,
                Top = 240,
                Width = 120
            };
            btnDeleteImage.Click += (s, e) => DeleteImage();

            btnSave = new() { Text = "Сохранить", Left = 120, Top = 380, Width = 100 };
            btnCancel = new() { Text = "Отмена", Left = 240, Top = 380, Width = 100 };

            btnCancel.Click += (s, e) => Close();
            btnSave.Click += async (s, e) => await SaveDish();

            Controls.AddRange(new Control[]
            {
                lbl1, txtName,
                lbl2, txtPrice,
                lbl3, txtIngredients,
                lbl4, pbImage, btnChooseImage, btnDeleteImage,
                btnSave, btnCancel
            });

            // Если редактируем блюдо — показать текущее фото
            if (isEdit && !string.IsNullOrWhiteSpace(dishImageUrl))
            {
                try
                {
                    string fullUrl = AppConfig.ApiBaseUrl.TrimEnd('/') + dishImageUrl;
                    pbImage.LoadAsync(fullUrl);
                }
                catch
                {
                    pbImage.Image = null;
                }
            }
        }

        private void ChooseImage()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Выберите фото блюда",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;
                pbImage.ImageLocation = selectedImagePath;
                removeExistingImage = false; // отменяем флаг удаления, если выбрали новое
            }
        }

        private void DeleteImage()
        {
            pbImage.Image = null;
            selectedImagePath = null;
            removeExistingImage = true; //помечаем, что пользователь хочет удалить старое фото
        }

        private async Task SaveDish()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название блюда");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var price) &&
                !decimal.TryParse(txtPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            {
                MessageBox.Show("Некорректная цена (пример: 150,00 или 150.00)");
                return;
            }

            using var http = new HttpClient();
            var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/menu";

            var priceText = price.ToString(CultureInfo.CurrentCulture);

            var form = new MultipartFormDataContent
            {
                { new StringContent(txtName.Text), "DishName" },
                { new StringContent(priceText), "Price" },
                { new StringContent(txtIngredients.Text), "IngredientNames" }
            };

            // Если фото выбрано — добавляем файл
            if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
            {
                var stream = File.OpenRead(selectedImagePath);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(fileContent, "images", Path.GetFileName(selectedImagePath));
            }

            // Если пользователь удалил фото — сообщаем серверу
            if (removeExistingImage)
                form.Add(new StringContent("true"), "RemoveImage");

            HttpResponseMessage resp;
            if (isEdit && dishId.HasValue)
                resp = await http.PutAsync(url + "/" + dishId, form);
            else
                resp = await http.PostAsync(url, form);

            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show(isEdit ? "Блюдо обновлено" : "Блюдо добавлено");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                var text = await resp.Content.ReadAsStringAsync();
                MessageBox.Show("Ошибка при сохранении: " + resp.StatusCode + "\n" + text);
            }
        }
    }
}
