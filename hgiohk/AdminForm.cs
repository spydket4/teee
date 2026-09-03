using System;
using System.Drawing;
using System.Windows.Forms;

namespace UserAuthApp
{
    public class AdminForm : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtNewLogin;
        private TextBox txtNewPass;
        private ComboBox cmbRole;
        private Button btnAddUser;
        private Button btnUnblock;

        public AdminForm()
        {
            this.Text = "Панель администратора";
            this.Size = new Size(800, 600);

            // Таблица пользователей
            dgvUsers = new DataGridView { Location = new Point(20, 20), Size = new Size(740, 300) };
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.ReadOnly = true;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Поля добавления
            Label lblLogin = new Label { Text = "Логин:", Location = new Point(20, 340), AutoSize = true };
            txtNewLogin = new TextBox { Location = new Point(80, 337), Width = 150 };

            Label lblPass = new Label { Text = "Пароль:", Location = new Point(250, 340), AutoSize = true };
            txtNewPass = new TextBox { Location = new Point(320, 337), Width = 150 };

            Label lblRole = new Label { Text = "Роль:", Location = new Point(500, 340), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(550, 337), Width = 150 };
            cmbRole.Items.AddRange(new object[] { "Администратор", "Пользователь" });
            cmbRole.SelectedIndex = 1;

            // Кнопки
            btnAddUser = new Button { Text = "Добавить", Location = new Point(20, 380), Width = 120 };
            btnAddUser.Click += btnAddUser_Click;

            btnUnblock = new Button { Text = "Снять блокировку", Location = new Point(160, 380), Width = 150 };
            btnUnblock.Click += btnUnblock_Click;

            // Добавляем на форму
            this.Controls.Add(dgvUsers);
            this.Controls.Add(lblLogin);
            this.Controls.Add(txtNewLogin);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtNewPass);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRole);
            this.Controls.Add(btnAddUser);
            this.Controls.Add(btnUnblock);

            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = DatabaseService.LoadUsers();
            dgvUsers.DataSource = null;
            dgvUsers.DataSource = users;
            dgvUsers.Columns["Password"].Visible = false; // Скрываем пароль
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewLogin.Text) || string.IsNullOrWhiteSpace(txtNewPass.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (DatabaseService.UserExists(txtNewLogin.Text))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка добавления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseService.AddUser(txtNewLogin.Text, txtNewPass.Text, cmbRole.SelectedItem.ToString());
            MessageBox.Show("Пользователь успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            LoadUsers();
            txtNewLogin.Clear();
            txtNewPass.Clear();
        }

        private void btnUnblock_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя в таблице!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = (int)dgvUsers.SelectedRows[0].Cells["Id"].Value;
            DatabaseService.UnblockUser(userId);

            MessageBox.Show("Блокировка снята.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUsers();
        }
    }
}