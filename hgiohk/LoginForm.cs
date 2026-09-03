using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UserAuthApp
{
    public class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCheckPuzzle;
        private Label lblCaptchaStatus;
        
        // Пазл
        private Panel panelTarget; // Куда нужно перетащить
        private PictureBox[] puzzlePieces; // Сами части
        private Point[] targetPositions; // Правильные координаты
        
        private int failedAttempts = 0;
        private bool isCaptchaSolved = false;

        public LoginForm()
        {
            this.Text = "Авторизация в системе";
            this.Size = new Size(750, 650);
            this.MinimumSize = new Size(700, 600);

            // --- Поля логина и пароля ---
            Label lblLogin = new Label { Text = "Логин:", Location = new Point(50, 50), AutoSize = true };
            txtLogin = new TextBox { Location = new Point(120, 47), Width = 200 };
            
            Label lblPass = new Label { Text = "Пароль:", Location = new Point(50, 90), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 87), Width = 200, UseSystemPasswordChar = true };

            // --- Кнопка входа ---
            btnLogin = new Button { Text = "Войти", Location = new Point(120, 130), Width = 100 };
            btnLogin.Click += btnLogin_Click;

            // --- Настройка пазла (Капчи) ---
            Label lblPuzzle = new Label { Text = "Перетащите части картинки в рамку, чтобы собрать целое изображение:", Location = new Point(50, 180), AutoSize = true };
            
            // Рамка (куда тащить)
            panelTarget = new Panel
            {
                Location = new Point(50, 210),
                Size = new Size(300, 300),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };

            // Создаем 4 части
            puzzlePieces = new PictureBox[4];
            targetPositions = new Point[]
            {
                new Point(0, 0),     // Верх-лево (1.png)
                new Point(150, 0),   // Верх-право (2.png)
                new Point(0, 150),   // Низ-лево (3.png)
                new Point(150, 150)  // Низ-право (4.png)
            };

            for (int i = 0; i < 4; i++)
            {
                string fileName = $"{i + 1}.png";
                
                // Ищем картинку: сначала рядом с exe, потом в корне проекта
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (!File.Exists(imagePath))
                {
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    imagePath = Path.Combine(projectRoot, fileName);
                }

                Bitmap img = null;
                if (File.Exists(imagePath))
                {
                    img = new Bitmap(Image.FromFile(imagePath), 150, 150); // Масштабируем
                }
                else
                {
                    // Заглушка, если файла нет
                    img = new Bitmap(150, 150);
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.Clear(Color.FromArgb(100 + i * 40, 150, 200));
                        g.DrawString(fileName, new Font("Arial", 14), Brushes.White, new PointF(40, 60));
                    }
                }

                puzzlePieces[i] = new PictureBox
                {
                    Image = img,
                    Size = new Size(150, 150),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Tag = i, // Индекс для определения правильной позиции
                    Location = new Point(400 + (i % 2) * 160, 210 + (i / 2) * 160) // Раскидываем справа
                };

                // Drag & Drop события
                puzzlePieces[i].MouseDown += PuzzlePiece_MouseDown;
                puzzlePieces[i].MouseMove += PuzzlePiece_MouseMove;
                puzzlePieces[i].MouseUp += PuzzlePiece_MouseUp;

                this.Controls.Add(puzzlePieces[i]);
            }
            this.Controls.Add(panelTarget);

            // Кнопка проверки
            btnCheckPuzzle = new Button { Text = "Проверить капчу", Location = new Point(50, 530), Width = 130 };
            btnCheckPuzzle.Click += btnCheckPuzzle_Click;

            lblCaptchaStatus = new Label { Text = "Капча не пройдена", Location = new Point(200, 535), AutoSize = true, ForeColor = Color.Red };

            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(lblPuzzle);
            this.Controls.Add(btnCheckPuzzle);
            this.Controls.Add(lblCaptchaStatus);
        }

        // --- Логика Drag & Drop ---
        private bool isDragging = false;
        private Point dragStart;

        private void PuzzlePiece_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStart = e.Location;
                ((PictureBox)sender).BringToFront();
            }
        }

        private void PuzzlePiece_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                PictureBox pb = (PictureBox)sender;
                pb.Left += e.X - dragStart.X;
                pb.Top += e.Y - dragStart.Y;
            }
        }

        private void PuzzlePiece_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // --- Проверка капчи ---
        private void btnCheckPuzzle_Click(object sender, EventArgs e)
        {
            bool isSolved = true;

            for (int i = 0; i < 4; i++)
            {
                Point correctPos = targetPositions[i];
                Point currentPos = puzzlePieces[i].Location;
                
                // Проверяем, находится ли элемент внутри рамки и на правильном месте (с допуском 5 пикселей)
                bool isInsidePanel = currentPos.X >= panelTarget.Left - 5 && currentPos.X <= panelTarget.Right - 145 &&
                                     currentPos.Y >= panelTarget.Top - 5 && currentPos.Y <= panelTarget.Bottom - 145;
                
                bool isCorrect = Math.Abs(currentPos.X - (panelTarget.Left + correctPos.X)) <= 5 &&
                                 Math.Abs(currentPos.Y - (panelTarget.Top + correctPos.Y)) <= 5;

                if (!isInsidePanel || !isCorrect)
                {
                    isSolved = false;
                    break;
                }
            }

            if (isSolved)
            {
                isCaptchaSolved = true;
                lblCaptchaStatus.Text = "Капча пройдена";
                lblCaptchaStatus.ForeColor = Color.Green;
            }
            else
            {
                isCaptchaSolved = false;
                lblCaptchaStatus.Text = "Капча не пройдена!";
                lblCaptchaStatus.ForeColor = Color.Red;

                failedAttempts++;
                // Блокируем ТОЛЬКО после 3 попыток подряд
                if (failedAttempts >= 3)
                {
                    DatabaseService.BlockUser(txtLogin.Text);
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    failedAttempts = 0;
                }
            }
        }

        // --- Кнопка входа ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Поля логин и пароль обязательны для заполнения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!isCaptchaSolved)
            {
                MessageBox.Show("Сначала соберите пазл!", "Капча", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = DatabaseService.Authenticate(txtLogin.Text, txtPassword.Text);

            if (user != null)
            {
                if (user.IsBlocked)
                {
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DatabaseService.UpdateFailedAttempts(user.Username, 0);
                MessageBox.Show("Вы успешно авторизовались", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (user.Role == "Администратор")
                {
                    AdminForm adminForm = new AdminForm();
                    adminForm.Show();
                }
                else
                {
                    UserForm userForm = new UserForm();
                    userForm.Show();
                }
                this.Hide();
            }
            else
            {
                failedAttempts++;
                DatabaseService.UpdateFailedAttempts(txtLogin.Text, failedAttempts);

                if (failedAttempts >= 3)
                {
                    DatabaseService.BlockUser(txtLogin.Text);
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные", 
                                    "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}