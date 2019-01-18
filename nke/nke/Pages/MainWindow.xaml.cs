using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using nke.Models;

namespace nke
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            TitleT.Text = this.Title;
        }

        private void AuthBut_Click(object sender, RoutedEventArgs e)
        {
            Auth(LoginT.Text,PassT.Password);
        }

        // Functions

        void Auth(string login, string pass)
        {
            using (nkeEntities1 db = new nkeEntities1())
            {
                var User = db.Users.FirstOrDefault(x => x.Login == login && x.Password == pass);
                if (User != null)
                {
                    //switch (isUser.Role) {
                    //    case "Admin":
                    //        MessageBox.Show("Admin","Info",MessageBoxButton.OK,MessageBoxImage.Information);
                    //        break;
                    //    case "Teacher":
                    //        MessageBox.Show("Teacher", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        break;
                    //    case "Student":
                    //        MessageBox.Show("Student", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        break;
                    //    case "User":
                    //        MessageBox.Show("User", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        break;
                    //    case "Internet":
                    //        MessageBox.Show("Internet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        break;
                    //}

                    UserPanel f = new UserPanel(User);
                    f.Show();

                }
                else
                {
                    System.Windows.MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
