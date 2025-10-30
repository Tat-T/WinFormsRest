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
            if (lvReservations.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите бронирование для удаления");
                return;
            }

            var item = lvReservations.SelectedItems[0];
            int id = int.Parse(item.SubItems[0].Text);

            var confirm = MessageBox.Show("Удалить выбранное бронирование?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            using var http = new HttpClient();
            var url = AppConfig.ApiBaseUrl.TrimEnd('/') + "/api/reservations/" + id;

            var resp = await http.DeleteAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show("Бронирование удалено");
                await LoadReservations();
            }
            else
            {
                MessageBox.Show("Ошибка удаления: " + resp.StatusCode);
            }
        }


        private void EditReservations()
        {
            if (lvReservations.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите бронирование для редактирования");
                return;
            }

            var item = lvReservations.SelectedItems[0];

            using var form = new AddEditReservationForm(
                isEdit: true,
                reservationId: int.Parse(item.SubItems[0].Text),
                name: item.SubItems[1].Text,
                email: item.SubItems[2].Text,
                phone: item.SubItems[3].Text,
                date: DateTime.TryParse(item.SubItems[4].Text, out var date) ? date : DateTime.Today,
                time: TimeSpan.TryParse(item.SubItems[5].Text, out var time) ? time : new TimeSpan(12, 0, 0),
                guests: int.TryParse(item.SubItems[6].Text, out var guests) ? guests : 1,
                message: item.SubItems[7].Text
            );

            if (form.ShowDialog() == DialogResult.OK)
                _ = LoadReservations();
        }


        private void AddReservations()
        {
            using var form = new AddEditReservationForm();
            if (form.ShowDialog() == DialogResult.OK)
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
