using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public partial class AddEditReservationForm : Form
    {
        private readonly bool isEdit;
        private readonly int? reservationId;

        private readonly TextBox txtName;
        private readonly TextBox txtEmail;
        private readonly TextBox txtPhone;
        private readonly DateTimePicker dpDate;
        private readonly DateTimePicker tpTime;
        private readonly NumericUpDown numGuests;
        private readonly TextBox txtMessage;
        private readonly Button btnSave;
        private readonly Button btnCancel;

        public AddEditReservationForm(bool isEdit = false, int? reservationId = null,
            string? name = null, string? email = null, string? phone = null,
            DateTime? date = null, TimeSpan? time = null, int guests = 1, string? message = null)
        {
            this.isEdit = isEdit;
            this.reservationId = reservationId;

            Text = isEdit ? "Редактировать бронирование" : "Добавить бронирование";
            Width = 400;
            Height = 450;
            StartPosition = FormStartPosition.CenterParent;

            Label lbl1 = new() { Text = "Имя:", Left = 20, Top = 20 };
            txtName = new() { Left = 120, Top = 20, Width = 220, Text = name ?? "" };

            Label lbl2 = new() { Text = "Email:", Left = 20, Top = 60 };
            txtEmail = new() { Left = 120, Top = 60, Width = 220, Text = email ?? "" };

            Label lbl3 = new() { Text = "Телефон:", Left = 20, Top = 100 };
            txtPhone = new() { Left = 120, Top = 100, Width = 220, Text = phone ?? "" };

            Label lbl4 = new() { Text = "Дата:", Left = 20, Top = 140 };
            dpDate = new() { Left = 120, Top = 140, Width = 150, Value = date ?? DateTime.Today };

            Label lbl5 = new() { Text = "Время:", Left = 20, Top = 180 };
            tpTime = new() { Left = 120, Top = 180, Width = 150, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            if (time != null) tpTime.Value = DateTime.Today.Add(time.Value);

            Label lbl6 = new() { Text = "Гостей:", Left = 20, Top = 220 };
            numGuests = new() { Left = 120, Top = 220, Width = 100, Minimum = 1, Maximum = 50, Value = guests };

            Label lbl7 = new() { Text = "Сообщение:", Left = 20, Top = 260 };
            txtMessage = new() { Left = 20, Top = 280, Width = 320, Height = 60, Multiline = true, Text = message ?? "" };

            btnSave = new() { Text = "Сохранить", Left = 80, Top = 360, Width = 100 };
            btnCancel = new() { Text = "Отмена", Left = 200, Top = 360, Width = 100 };

            btnCancel.Click += (s, e) => Close();
            btnSave.Click += async (s, e) => await SaveReservation();

            Controls.AddRange(new Control[] {
                lbl1, txtName, lbl2, txtEmail, lbl3, txtPhone,
                lbl4, dpDate, lbl5, tpTime, lbl6, numGuests,
                lbl7, txtMessage, btnSave, btnCancel
            });
        }

        private async Task SaveReservation()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя");
                return;
            }

            using var http = new HttpClient();
            var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/reservations";

            var reservation = new
            {
                Id = reservationId,
                Name = txtName.Text,
                Email = txtEmail.Text,
                Phone = txtPhone.Text,
                ReservationDate = dpDate.Value.Date,
                ReservationTime = new TimeSpan(tpTime.Value.Hour, tpTime.Value.Minute, 0),
                Guests = (int)numGuests.Value,
                Message = txtMessage.Text
            };

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(reservation, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //HttpResponseMessage resp;
            //if (isEdit && reservationId.HasValue)
            //    resp = await http.PutAsync(url + "/" + reservationId, content);
            //else
            //    resp = await http.PostAsync(url, content);

            //if (resp.IsSuccessStatusCode)
            //{
            //    MessageBox.Show(isEdit ? "Бронирование обновлено" : "Бронирование добавлено");
            //    DialogResult = DialogResult.OK;
            //    Close();
            //}
            //else
            //{
            //    MessageBox.Show("Ошибка сохранения: " + resp.StatusCode);
            //}

            HttpResponseMessage resp;
            if (isEdit && reservationId.HasValue)
                resp = await http.PutAsync(url + "/" + reservationId, content);
            else
                resp = await http.PostAsync(url, content);

            var responseText = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show(isEdit ? "Бронирование обновлено" : "Бронирование добавлено");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                string message = $"Ошибка сохранения ({(int)resp.StatusCode})";

                try
                {
                    // сообщение из JSON { "message": "..." }
                    var error = JsonSerializer.Deserialize<Dictionary<string, string>>(responseText);
                    if (error != null && error.ContainsKey("message"))
                        message += $": {error["message"]}";
                    else if (!string.IsNullOrWhiteSpace(responseText))
                        message += $": {responseText}";
                }
                catch
                {
                    // Если не удалось распарсить JSON — покажем как есть
                    if (!string.IsNullOrWhiteSpace(responseText))
                        message += $": {responseText}";
                }

                MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
