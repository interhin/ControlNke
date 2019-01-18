using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using nke.Models;
using Microsoft.Win32;
using System.Management;
using nke.Classes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace nke
{
    /// <summary>
    /// Логика взаимодействия для UserPanel.xaml
    /// </summary>
    ///

    public partial class UserPanel : Window, ServerChat.IServerChatCallback
    {
        ServerChat.ServerChatClient client;
        int ID = 0;


        Users user; // Экземпляр авторизованного пользователя
        List<Programms> programmsList = new List<Programms>(); // Лист доступных программ пользователя (в панели сверху)


        public ObservableCollection<ProgrammsContext> programmsContextList { get; set; } = new ObservableCollection<ProgrammsContext>(); // Лист программ на компьютере

        public UserPanel(Users usr)
        {
            InitializeComponent();


            user = usr; // Передача данных об авторизованном пользователе

            switch (user.Role) {

                case "Student":
                    loadStudent();
                    break;
                case "Admin":
                    loadAdmin();
                    break;

            }

            TitleText.Text = user.Login;


            // Подключение к серверу
            client = new ServerChat.ServerChatClient(new System.ServiceModel.InstanceContext(this)); // Создаем экземпляр сервиса и инициализируем новый экземпляр класса InstanceContext для заданного обьекта который реализуем экземпляр сервиса
            ID = client.Connect(user.Login, user.Role); // Подключаемся к сервису и получаем свой ID


            // Костыль
            MessageText.AddHandler(System.Windows.Controls.TextBox.DropEvent, new System.Windows.DragEventHandler(MessageText_Drop), true);
            MessageText.AddHandler(System.Windows.Controls.TextBox.PreviewDropEvent, new System.Windows.DragEventHandler(MessageText_Drop), true);

        }

    
        // Events

        private void PublicBack_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserPublic.CanGoBack)
                BrowserPublic.GoBack();
        }

        private void PublicForward_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserPublic.CanGoForward)
                BrowserPublic.GoForward();
        }

        private void UserBack_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserUser.CanGoBack)
                BrowserUser.GoBack();
        }

        private void UserForward_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserUser.CanGoForward)
                BrowserUser.GoForward();
        }


        private void MessageText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Проверяем нажал ли пользователь в текстбоксе на Enter
            if (e.Key == Key.Enter)
            {
                client.SendMsg(MessageText.Text, ID, null); // Отправляем сообщение на сервер
                MessageText.Text = string.Empty; // Очищаем текстбокс
            }
        }

        private void MessageText_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Проверяем нажали пользователь в текстбоксе на стрелку вверх или вниз
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                // Если нажал и контекстное меню открыто то переводим фокус на него
                if (ContextMenu.IsOpen)
                {
                    ContextMenu.Focusable = true;
                    ContextMenu.Focus();
                    ContextMenu.Focusable = false;

                }
            }
        }

        private void UsersCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обновление списка программ для выбранного пользователя ( автоматическая расстановка чекбоксов )
            UpdateProgrammsList(Convert.ToInt32(UsersCB.SelectedValue));
        }

        private void saveP_Click(object sender, RoutedEventArgs e)
        {

            using (nkeEntities1 db = new nkeEntities1())
            {

                // Загружаем список программ которые уже есть у пользователя в базе данных
                List<Programms> oldProgramms = new List<Programms>();
                oldProgramms = db.Programms.Where(x => x.UserID == (int)UsersCB.SelectedValue).ToList();


                foreach (var pr in programmsContextList) // Пробегаемся по списку всех программ на компьютере
                {
                    // Если нашли программу которая есть у пользователя в БД и галочка убрана то удаляем её из БД
                    var isset = oldProgramms.FirstOrDefault(x => x.ProgrammName == pr.ProgrammName && x.UserID == (int)UsersCB.SelectedValue);
                    if (isset != null && !pr.IsChecked)
                    {
                        var delP = db.Programms.First(x => x.ProgrammName == isset.ProgrammName && x.UserID == (int)UsersCB.SelectedValue);
                        db.Programms.Remove(delP);
                    }
                    else if (isset == null) // Если программы нет в БД у пользователя и она отмечена галочкой то добавляем её в БД
                    {
                        if (pr.IsChecked)
                        {
                            Programms program = new Programms();
                            program.ProgrammName = pr.ProgrammName;
                            program.ProgrammPath = pr.ProgrammPath;
                            program.UserID = Convert.ToInt32(UsersCB.SelectedValue);
                            db.Programms.Add(program);
                        }
                    }
                }
                db.SaveChanges();
            }
            System.Windows.MessageBox.Show("Изменения успешно сохранены!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            // Если был отмечен чекбокс то ищем выбранную программу в листе, узнаем путь до её папки и открываем диалоговое окно
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox; // Узнаем какой чекбокс вызвал событие
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog(); // Создаем экземпляр диалога
            var p = programmsContextList.First(x => x.ProgrammName == checkBox.Content.ToString());
            dlg.InitialDirectory = p.ProgrammPath; // Меняем начальный путь для диалога на путь к папке с программой
            dlg.Filter = "Программа | *.exe";
            if (dlg.ShowDialog() == true) // Проверяем нажали ли пользователь на отмену или нет
                p.ProgrammPath = dlg.FileName; // Меняем путь программы в листе
            else
            {
                System.Windows.MessageBox.Show("Программа не была добавлена!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                // Обновление списка ( чтобы отключился чекбокс (костыль) )
                UpdateProgrammsList(Convert.ToInt32(UsersCB.SelectedValue));
            }


        }

        private void addP_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалоговое окно и добавляем в лист выбранный файл
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Программа | *.exe"; // Делаем фильтр для диалогового окна чтобы он показывал только .exe файлы
            dlg.ShowDialog(); // Показываем диалог
            string pName = dlg.SafeFileName.Replace(".exe", string.Empty); // Убираем расширение из имени файла
            programmsContextList.Add(new ProgrammsContext() { ProgrammName = pName, ProgrammPath = dlg.FileName, IsChecked = true }); // Добавляем программу в лист
            System.Windows.MessageBox.Show("Программа с именем: \"" + pName + "\" и путем: \"" + dlg.FileName + "\" успешно добавлена!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProgrammsCheckBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Отключаем пользователя при закрытии формы
            client.Disconnect(ID);
            client.Close();
        }


        // Functions

        void loadAdmin()
        {
            TopPanel.Height = new GridLength(0); // Скрываем верхнюю панель
            // Меняем пропорции у чата
            ChatRow1.Height = new GridLength(0.08,GridUnitType.Star);
            ChatRow2.Height = new GridLength(0.84, GridUnitType.Star);
            // Меняем цвет у таблиц с файлами
            GridLocal.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD"));
            GridNetwork.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#BDBDBD"));

            AdminPanel.Visibility = Visibility.Visible; // Показываем панель админа
            StudentPanel.Visibility = Visibility.Hidden; // Скрываем панель студента

            // Указываем путь для сетевых и локальных папок
            BrowserPublic.Navigate(@"C:\");
            BrowserUser.Navigate(@"C:\");

            // Загружаем список студентов в Combobox
            using (nkeEntities1 db = new nkeEntities1())
                UsersCB.ItemsSource = db.Users.Where(x => x.Role == "Student").ToList();

            UsersCB.VerticalContentAlignment = VerticalAlignment.Center;
            // Устанавливаем поле которое будет отображаться
            UsersCB.DisplayMemberPath = "Login";
            // Устанавливаем фактическое значение отображаемого поля
            UsersCB.SelectedValuePath = "ID";
            UsersCB.SelectedIndex = 0; // Делаем выбранным первый элемент

            // Загружаем список установленных на компьютере программ
            loadProgrammsContext();

            // Расставляем чекбоксы на программах которые уже доступны пользователю
            UpdateProgrammsList(Convert.ToInt32(UsersCB.SelectedValue));
        }

        void loadProgrammsContext()
        {
            // Загрузка в лист списка установленных программ
            programmsContextList = Getinstalledsoftware();
            // Загрузка в ListBox списка установленных программ
            ProgrammsCheckBox.DataContext = programmsContextList;
            // Устанавливаем фактическое значение отображаемого поля в ListBox (путь до программы)
            ProgrammsCheckBox.SelectedValuePath = "ProgrammPath";
        }

        void loadStudent()
        {
            AdminPanel.Visibility = Visibility.Hidden; // Скрываем панель админа
            StudentPanel.Visibility = Visibility.Visible; // Показываем панель студента

            // Процедура отлова всех окон
            //CatchingWindows();

            // Указываем путь для локальных и сетевых папок
            BrowserPublic.Navigate(@"C:\");
            //BrowserUser.Navigate(@"\\DC-CLOUD-2\" + user.Login);

            using (nkeEntities1 db = new nkeEntities1())
            {
                var programms = db.Programms.Where(x => x.UserID == user.ID).ToList(); // Загружаем список доступных пользователю программ
                foreach (var item in programms) // Создание кнопки для каждой программы
                {
                    System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
                    btn.Click += OpenProgramm; // Вешаем событие на кнопку
                    btn.Content = item.ProgrammName; // Указываем имя программы
                    btn.Height = 40;
                    btn.Cursor = System.Windows.Input.Cursors.Hand;
                    btn.Style = Resources["RoundedButtonStyle"] as Style;
                    btn.Margin = new Thickness(5);
                    btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    spProgramms.Children.Add(btn); // Добавляем кнопку на форму в stack panel
                    programmsList.Add(item); // Так же добавляем запись о кнопке в лист
                }
            }
        }

        void OpenProgramm(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button; // Узнаем какая кнопка вызвала событие
            var programm = programmsList.First(x => x.ProgrammName == btn.Content.ToString()); // По названию программы в кнопке ищем её путь в листе
            Process p = new Process(); // Создаем процесс
            p.StartInfo.FileName = programm.ProgrammPath; // Пишем в переменную путь до процесса
            p.Start(); // Запускаем
        }

        void CatchingWindows()
        {
            Process curProcess = Process.GetCurrentProcess(); // Создаем переменную нашего процесса чтобы узнать свой id

            // Запускаем новый поток который постоянно будет отлавливать все окна
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    // Массив Handle'ов всех окон
                    List<IntPtr> windowsList = new List<IntPtr>();
                    // Поиск всех открытых окон
                    WinApi.EnumWindows((hWnd, lParam) =>
                    {
                        if (WinApi.IsWindowVisible(hWnd) && WinApi.GetWindowTextLength(hWnd) != 0) // Проверяем видимое ли окно и есть ли у него заголовок
                        {
                            int pID = 0;
                            // Процедура которая узнаем к какому процессу (id) принадлежит хэндл
                            WinApi.GetWindowThreadProcessId(hWnd, out pID); // Исключаем из списка программ которые нужно поместить в panel1: рабочий стол и все окна своей сборки
                            if (GetWindowText(hWnd) != "nke (Running) - Microsoft Visual Studio  (Administrator)" && curProcess.Id != pID && GetWindowText(hWnd) != "Program Manager")
                                windowsList.Add(hWnd);
                        }
                        return true;
                    }, IntPtr.Zero);


                    // Пробегаемся по листу с окнами которые нужно положить
                    foreach (var item in windowsList)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            // Кладем все окна в panel1
                            WinApi.SetParent(item, panel1.Handle);
                            WinApi.MoveWindow(item, 0, 0, panel1.Width, panel1.Height, true);
                        }));
                    }

                    windowsList.Clear(); // Очищаем список

                    Thread.Sleep(100); // Задержка чтобы не нагружать проц
                }
            });
        }

        // Узнаем заголовок окна по хэндлу
        public string GetWindowText(IntPtr hWnd)
        {
            int len = WinApi.GetWindowTextLength(hWnd) + 1; // Узнаем длину текста
            StringBuilder sb = new StringBuilder(len); // Создаем string builder с указанной длиной
            len = WinApi.GetWindowText(hWnd, sb, len); // Пихаем название из функции в string builder
            return sb.ToString(0, len); // Возвращаем значение
        }

        // Поиск дочерних элементов по хендлу
        List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            List<IntPtr> res = new List<IntPtr>(); // Лист с дочерними элементами
            int ct = 0; // Счетчик
            IntPtr prevChild = IntPtr.Zero; // Буфер для предыдущего хэндла
            IntPtr currChild = IntPtr.Zero; // Буфер для текущего хэндла
            while (true && ct < maxCount)
            {
                currChild = WinApi.FindWindowEx(hParent, prevChild, null, null); // Ищем окно у хэндла переданного в аргументах
                if (currChild == IntPtr.Zero) break; // Если не нашли то выходим из цикла и возвращает пустой лист
                res.Add(currChild); // Если нашли то добавляем хэндл в лист
                prevChild = currChild;
                ++ct;
            }
            return res;
        }

        ObservableCollection<ProgrammsContext> Getinstalledsoftware()
        {

            ObservableCollection<ProgrammsContext> list = new ObservableCollection<ProgrammsContext>(); // Буферный список чтобы вернуть

         
            string SoftwareKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"; // Путь к программам в реестре
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(SoftwareKey))
            {
            
                foreach (string skName in rk.GetSubKeyNames()) // Пробегаемся по всем папкам
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName)) // Открываем все записи в папке
                    {
                        try
                        {

                            if (sk.GetValue("DisplayName") != null) // Если у проги есть имя то добавляем в список
                            {
                                    list.Add(new ProgrammsContext() { ProgrammName = sk.GetValue("DisplayName") as string, ProgrammPath = sk.GetValue("InstallLocation") as string, IsChecked = false });
                            }
                        }
                        catch
                        {
                            
                        }
                    }
                }
            }

            return list;
        }

        void UpdateProgrammsList (int id)
        {
            loadProgrammsContext(); // Обновляем список
            using (nkeEntities1 db = new nkeEntities1())
            {
                // Загружаем список доступных пользователю программ
                var userProgramms = db.Programms.Where(x => x.UserID == id).ToList();

                foreach (var programm in userProgramms)
                {
                    // Если нашли программу в листе то устанавливаем чекбокс в true
                    try
                    {
                        var attachedProgramm = programmsContextList.First(x => x.ProgrammName == programm.ProgrammName);
                        attachedProgramm.IsChecked = true;
                    }
                    catch // Если программы нет в листе, но есть в базе то добавляем
                    {
                        programmsContextList.Add(new ProgrammsContext() { ProgrammName = programm.ProgrammName, ProgrammPath = programm.ProgrammPath, IsChecked = true });
                    }

                }
            }
        }

        public void MsgCallBack(string msg)
        {
            // Если в сообщении есть ссылка на картинку то вытаскиваем её из тега <ы>
            if (msg.Contains("<ы>"))
            {
                try
                {
                    Regex rx = new Regex("<ы>.*</ы>");
                    var m = Regex.Match(msg, "<ы>.*</ы>"); // Ищем тег <ы></ы>
                    string imageSrc = m.Value.Replace("<ы>", string.Empty).Replace("</ы>", string.Empty).ToString(); // Вытаскиваем тег из строки
                    msg = rx.Replace(msg, string.Empty); // Убираем сам тег
                    Image img = new Image();
                    img.Width = 225;
                    img.Height = 225;
                    img.Source = new BitmapImage(new Uri(imageSrc)); // Пихаем картинку по ссылке в переменную

                    TextBlock tx = new TextBlock(); // Пихаем в текст блок с переносами и ставим ему ширину как у чата
                    tx.TextWrapping = TextWrapping.Wrap; 
                    tx.Width = ChatList.Width;
                    tx.Text = msg;

                    ChatList.Items.Add(tx); // Добавляем сначала текст потом картинку
                    ChatList.Items.Add(img);
                }
                catch (Exception e)
                {
                    //System.Windows.MessageBox.Show(e.Message);
                }
            }
            else // Если в сообщении нет ссылки на картинку то просто отправляем его
            {
                // Чтобы работали переносы
                TextBlock tx = new TextBlock();
                tx.TextWrapping = TextWrapping.Wrap; // Говорим текстбоксу чтобы переносил строки
                tx.Width = ChatList.Width; // Говорим текстбоксу чтобы подстраивался под ширину чата
                tx.Text = msg;

                // Добавляем текст с переносами
                ChatList.Items.Add(tx);
                ChatList.ScrollIntoView(ChatList.Items[ChatList.Items.Count - 1]); // Скролим чат на последнее сообщение
            }
        }

        private void MessageText_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
        }

        private void MessageText_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string imgS = "<ы>" + (e.Data.GetData(System.Windows.DataFormats.Text) as string) + "</ы>"; // Кладем картинку которую перенесли в тег <ы></ы>
            MessageText.Text = imgS;
            
        }

        private void MessageText_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.All;
            e.Handled = true;
        }

        bool IsEndEnterUser = true; // Переменная чтобы знать когда введение получателя через собаку окончено
        int indexSob = 0; // Переменная чтобы запоминать где находится собака

        private void MessageText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (MessageText.Text[MessageText.Text.Length - 1].ToString() == " ") // Если мы нажали Enter значит написали имя получателя
                    IsEndEnterUser = true;
                if (MessageText.Text[MessageText.Text.Length - 1].ToString() == "@") // Если мы ввели собаку то показываем меню и запоминаем где собака
                {
                    indexSob = MessageText.Text.Length - 1;
                    loadPopup("");
                }
                else if (!IsEndEnterUser) // Если мы еще не закончили писать имя получателя то при введении каждого символа фильтруем список по уже введенным данным
                {
                    string reg = "";
                    for (int i = indexSob; i <= MessageText.Text.Length-1; i++) // Пробегаемся по строке от собаки до конца и формируем имя пользователя
                        reg += MessageText.Text[i];
                    reg = reg.Replace("@",string.Empty); // Убираем собаку из переменной
                    loadPopup(reg); // И фильтруем список по введенному суффиксу
                }
            }
            catch { }
        }

        void loadPopup(string start)
        {
            string[] authorizedUsers = client.GetAuthorizedUsers();
            List<string> authorizedUsersList = new List<string>();
            foreach (var item in authorizedUsers) authorizedUsersList.Add(item);
            var myself = authorizedUsersList.First(x => x == user.Login);
            authorizedUsersList.Remove(myself);
            List<string> users = new List<string>(); // Список отфильтрованных пользователей
            List<System.Windows.Controls.MenuItem> menuItems = new List<System.Windows.Controls.MenuItem>(); // Список для добавления в меню
            users = authorizedUsersList.Where(x => x.StartsWith(start)).ToList(); // Ищем в базе всех пользователей которые начинаются с 'start'
            foreach (var user in users)
                menuItems.Add(new System.Windows.Controls.MenuItem() { Header = user }); // Создаем эелемент меню для каждого логина
            ContextMenu.ItemsSource = menuItems; // Кидаем в ContextMenu полученный список
            ContextMenu.Focusable = false; // Отключаем фокус у меню
            ContextMenu.PlacementTarget = MessageText; // Меню будет появляться у текстбокса
            ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom; // Снизу или сверху если не будет места
            foreach (System.Windows.Controls.MenuItem item in ContextMenu.Items)
                item.Click += ContextMenuItemClick; // Вешаем на каждый элемент обработчик события при клике
            ContextMenu.IsOpen = true; // Открываем меню
            IsEndEnterUser = false; // Говорим что начали писать логин получателя
        }

        void ContextMenuItemClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem; // Узнаем на какую кнопку кликнули ( кто инициатор события )
            string s = "";
            for (int i = 0; i <= indexSob; i++) // Сохраняем в переменную 's' все что до собаки
                s += MessageText.Text[i];
            MessageText.Text = ""; // Очищаем текстбокс
            MessageText.Text +=s + item.Header + " "; // Подставляем сообщение + имя получателя
            MessageText.SelectionStart = MessageText.Text.Length; // Ставим стрелочку в конец текстбокса
        }
    }
}
