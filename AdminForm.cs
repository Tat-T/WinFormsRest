using System;
using System.Windows.Forms;

namespace WindowsAdminApp
{
    public partial class AdminForm : Form
    {
        private readonly TabControl tabControl;

        public AdminForm()
        {
            Text = "Admin Panel - Restaurant";
            Width = 1200;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Вкладки
            tabControl = new TabControl { Dock = DockStyle.Fill };

            var usersTab = new TabPage("Users");
            usersTab.Controls.Add(new UsersPage { Dock = DockStyle.Fill });

            var menuTab = new TabPage("Menu");
            menuTab.Controls.Add(new MenuPage { Dock = DockStyle.Fill });

            var reservationsTab = new TabPage("Reservations");
            reservationsTab.Controls.Add(new ReservationsPage { Dock = DockStyle.Fill });

            tabControl.TabPages.Add(usersTab);
            tabControl.TabPages.Add(menuTab);
            tabControl.TabPages.Add(reservationsTab);


            Controls.Add(tabControl);
        }
    }
}
