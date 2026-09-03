using System;
using System.Drawing;
using System.Windows.Forms;

namespace UserAuthApp
{
    public class UserForm : Form
    {
        public UserForm()
        {
            this.Text = "Рабочий стол пользователя";
            this.Size = new Size(500, 400);
            
            Label lblWelcome = new Label { Text = "Добро пожаловать!", Location = new Point(150, 150), AutoSize = true, Font = new Font("Arial", 16) };
            this.Controls.Add(lblWelcome);
        }
    }
}