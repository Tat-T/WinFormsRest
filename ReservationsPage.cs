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
        private readonly Button btnRefresh, btnAdd, btnEdit, btnDelete;
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

            btnAdd = new Button { Text = "Добавить", Left = 120, Top = 10, Width = 100 };
            btnAdd.Click += (s, e) => AddReservations();

            btnEdit = new Button { Text = "Редактировать", Left = 230, Top = 10, Width = 100 };
            btnEdit.Click += (s, e) => EditReservations();

            btnDelete = new Button { Text = "Удалить", Left = 340, Top = 10, Width = 100 };
            btnDelete.Click += async (s, e) => await DeleteReservations();

            lvReservations = new ListView
            {
                Left = 10,
                Top = 50,
                Width = 1150,
                Height = 500,
                View = View.Details,
                FullRowSelect = true
            };

            lvReservations.Columns.Add("N", 40);
            lvReservations.Columns.Add("Имя", 100);
            lvReservations.Columns.Add("Email", 130);
            lvReservations.Columns.Add("Телефон", 100);
            lvReservations.Columns.Add("Дата", 120);
            lvReservations.Columns.Add("Время", 120);
            lvReservations.Columns.Add("Гости", 100);
            lvReservations.Columns.Add("Сообщение", 200);

            Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            Controls.Add(lvReservations);

            _ = LoadReservations();
        }

        private async Task DeleteReservations()
        {
            throw new NotImplementedException();
        }

        private void EditReservations()
        {
            throw new NotImplementedException();
        }

        private void AddReservations()
        {
            throw new NotImplementedException();
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
                    item.SubItems.Add(r.Name ?? "");
                    item.SubItems.Add(r.Email ?? "");
                    item.SubItems.Add(r.Phone ?? "");
                    item.SubItems.Add(r.ReservationDate.ToShortDateString());
                    item.SubItems.Add(r.ReservationTime.ToString(@"hh\:mm"));
                    item.SubItems.Add(r.Guests.ToString());
                    item.SubItems.Add(r.Message ?? "");
                    //item.SubItems.Add(r.CreatedAt.ToString("g") ?? "");
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
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public DateTime ReservationDate { get; set; } = DateTime.Today;
            public TimeSpan ReservationTime { get; set; }
            public int Guests { get; set; }
            public string? Message { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }
}
