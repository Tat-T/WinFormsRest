using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public class ReservationsPage : UserControl
    {
        private readonly Button btnRefresh;
        private readonly ListView lvReservations;

        public ReservationsPage()
        {
            Dock = DockStyle.Fill;

            btnRefresh = new Button
            {
                Text = "Обновить",
                Left = 10,
                Top = 10,
                Width = 100
            };
            btnRefresh.Click += async (s, e) => await LoadReservations();

            lvReservations = new ListView
            {
                Left = 10,
                Top = 50,
                Width = 1100,
                Height = 500,
                View = View.Details,
                FullRowSelect = true
            };

            lvReservations.Columns.Add("ID", 60);
            lvReservations.Columns.Add("Пользователь", 200);
            lvReservations.Columns.Add("Дата", 200);
            lvReservations.Columns.Add("Гостей", 80);
            lvReservations.Columns.Add("Статус", 150);

            Controls.Add(btnRefresh);
            Controls.Add(lvReservations);

            _ = LoadReservations();
        }

        private async Task LoadReservations()
        {
            try
            {
                btnRefresh.Enabled = false;
                lvReservations.Items.Clear();

                using var http = new HttpClient();
                var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/reservations";
                var resp = await http.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Не удалось получить бронирования. Код: " + resp.StatusCode);
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<List<ReservationDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var r in items)
                {
                    var item = new ListViewItem(r.Id.ToString());
                    item.SubItems.Add(r.UserEmail ?? "");
                    item.SubItems.Add(r.Date?.ToString("g") ?? "");
                    item.SubItems.Add(r.Guests.ToString());
                    item.SubItems.Add(r.Status ?? "");
                    lvReservations.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки бронирований: " + ex.Message);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private class ReservationDto
        {
            public int Id { get; set; }
            public string UserEmail { get; set; }
            public DateTime? Date { get; set; }
            public int Guests { get; set; }
            public string Status { get; set; }
        }
    }
}
