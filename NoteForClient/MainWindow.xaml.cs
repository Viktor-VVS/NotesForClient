using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoteForClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //*****Глобальные переменные******//
        private string monitordirrglobal; // путь к папке с клиентами.
        private SyncCollection<string> clientNamecollglobal = new SyncCollection<string>(); // список имен всех клиентов.
        FileSystemWatcher watcherglobal = new FileSystemWatcher(); //мониторит изменения в папке monitordirrglobal.
        private System.Windows.Forms.NotifyIcon TrayIcon = null;
        private ContextMenu TrayMenu = null;
        //*********************************//
        public MainWindow()
        {

            this.ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();
            //считываем данные из сеттингс.ini
            GetSettingsInfo();
            //Обновляем ListBox ClientList
            RefreshClientList();
            //Запускаем мониторинг папки клиентов.
            StartFileSystemWatcher();
        }

#region Сворачивание в Трей 

        // переопределяем обработку первичной инициализации приложения
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e); // базовый функционал приложения в момент запуска.
            createTrayIcon();// создание нашей иконки.
        }
        private WindowState fCurrentWindowState = WindowState.Normal;
        public WindowState CurrentWindowState
        {
            get { return fCurrentWindowState; }
            set { fCurrentWindowState = value; }
        }
        private bool createTrayIcon()
        {
            bool result = false;
            if (TrayIcon == null) // только если мы не создали иконку ранее
            {
                TrayIcon = new System.Windows.Forms.NotifyIcon(); // создаем новую
                TrayIcon.Icon = NoteForClient.Properties.Resources.notebook; // указать полный namespace
                TrayMenu = Resources["TrayMenu"] as ContextMenu; // а здесь уже ресурсы окна и тот самый x:Key
                TrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MouseDoubleClick);
                // сразу же опишем поведение при щелчке мыши
                // это будет просто анонимная функция, незачем выносить ее в класс окна
                TrayIcon.Click += delegate (object sender, EventArgs e)
                {
                    if ((e as System.Windows.Forms.MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Right) // по правой кнопке  показываем меню
                    {
                        TrayMenu.IsOpen = true;
                        Activate(); // нужно отдать окну фокус, см. ниже
                    }
                };
                result = true;
            }
            else  // все переменные были созданы ранее
            {
                result = true;
            }
            TrayIcon.Visible = true; // делаем иконку видимой в трее
            return result;
        }
        private void ShowHideMainWindow(object sender, RoutedEventArgs e)
        {
            TrayMenu.IsOpen = false; // спрячем менюшку, если она вдруг видима
            if (IsVisible)// если окно видно на экране  прячем его
            {
                Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Показать";// меняем надпись на пункте меню
            }
            else  // а если не видно показываем
            {
                Show();
                (TrayMenu.Items[0] as MenuItem).Header = "Спрятать";// меняем надпись на пункте меню
                WindowState = CurrentWindowState;
                Activate(); // обязательно нужно отдать фокус окну,
                // иначе пользователь сильно удивится, когда увидит окно
                // но не сможет в него ничего ввести с клавиатуры
            }
        }
        protected override void OnStateChanged(EventArgs e)  // переопределяем встроенную реакцию на изменение состояния сознания окна
        {
            base.OnStateChanged(e);  // системная обработка
            if (this.WindowState == System.Windows.WindowState.Minimized)  // если окно минимизировали, просто спрячем
            {
                Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Показать"; // и поменяем надпись на менюшке
            }
            else
            {
                CurrentWindowState = WindowState; // в противном случае запомним текущее состояние
            }
        }
        private void MenuExitClick(object sender, RoutedEventArgs e)
        {
            TrayIcon.Visible = false;
            Environment.Exit(0);
        }

        // обработчик двойного клика мышки на развертывание из трея , тут же определяем обработку с Pidgin
        void MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        #endregion

#region Обработчики событий 
        private void ClientsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           CurrentClient.Text = ClientsList.SelectedItem as string;
           LoadingNoteContent();

        } // обработчик двойного нажатия по элементу списка ClientList.
        private void SearchByClientsName_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> SortedByNamecoll = new List<string>();
            TextBox text = sender as TextBox;
            clientNamecollglobal.ForEach(x => { if (x.Contains(text.Text)) SortedByNamecoll.Add(x); });
            ClientsList.ItemsSource = null;
            ClientsList.ItemsSource = SortedByNamecoll;
        } //обработчик фильтра по имени клиента.
        private void Ctrl_S_IsKeyDown(object sender, KeyEventArgs e)
        {
            //если нажаты кнопки Ctl + S
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.S)
            {
                SaveCurrentClientNote();
            }

        } //Обработчик нажатия сочетания клавиш Ctrl + S
#endregion

#region Обработчики нажатия на кнопки
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadingNoteContent();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentClientNote();
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(CurrentClient.Text))
            {
                MessageBox.Show("Ошибка удаления заметки : Сначала необходимо  выбрать заметку двойным щелчком мыши по имени клиента .");
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить текущую заметку по клиенту  \"" + CurrentClient.Text + "\"  ?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {

                    string deletingFilePath = monitordirrglobal + "\\" + CurrentClient.Text + ".txt";
                    if (File.Exists(deletingFilePath)) // если заметка существует ...
                    {
                        try
                        {
                            File.Delete(deletingFilePath); // удаляем заметку
                            ClientContent.Text = "";
                            CurrentClient.Text = "";
                            RefreshClientList(); // обновляем список всех клиентов.
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка удаления заметки : " + ex.ToString());
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка удаления заметки : выбранная заметка не существует");
                    }
                }
            }

        }
        private void RefreshClientsList_Click(object sender, RoutedEventArgs e)
        {
            RefreshClientList();
        }
        private void SearchbyContent_Click(object sender, RoutedEventArgs e)
        {
            List<string> SortedByContentcoll = new List<string>();
            string filepath = "", content = "";
            clientNamecollglobal.ForEach(x => {
                filepath = monitordirrglobal + "\\" + x + ".txt";
                if (File.Exists(filepath) == true)
                {
                    content = File.ReadAllText(filepath, Encoding.Default);
                    if (content.Contains(SearchByContent.Text) == true)
                        SortedByContentcoll.Add(x);
                }
            });
            ClientsList.ItemsSource = null;
            ClientsList.ItemsSource = SortedByContentcoll;

        }

#endregion

#region Рабочие методы

        /// <summary>
        ///Загрузить содержимое заметки.
        /// </summary>
        private void LoadingNoteContent()
        {
            if (String.IsNullOrEmpty(CurrentClient.Text))
                System.Windows.MessageBox.Show("Поле имени клиента \"Client\" не может быть пустым.", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                string neededсlientfpath = monitordirrglobal + "\\" + CurrentClient.Text + ".txt";
                if (File.Exists(neededсlientfpath) == false)
                    System.Windows.MessageBox.Show("Клиент с именем : " + CurrentClient.Text + "  не найден", "INFO", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                {
                    LastModifyInfo.Content = GetOwnersFileInfo(neededсlientfpath);
                    try
                    {
                        ClientContent.Text = File.ReadAllText(neededсlientfpath,Encoding.Default); //Считываем текст заметки.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка чтения из файла клиента : " + ex.ToString());
                    }

                }
            }


        }

        /// <summary>
        /// Получить хозяина файла 
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <returns></returns>
        private string GetOwnersFileInfo(string path)
        {
            FileInfo fi = new FileInfo(path);
            string LastmodifiedData = fi.LastWriteTime.ToString("HH:mm dd/MM/yyyy").Replace(".", "/");
            try
            {
                string ownervalue = fi.GetAccessControl().GetOwner(typeof(NTAccount)).Value;
                string ownerfile = ownervalue.Substring(ownervalue.LastIndexOf("\\") + 1);
                return "Last Modified By : " + ownerfile + " " + LastmodifiedData;
            }
            catch(Exception ex)
            {
                return "Last Modified By : " + "Unknow" + " " + LastmodifiedData;
            }
          
        }

        /// <summary>
        /// получаем информацию из settings.ini
        /// </summary>
        void GetSettingsInfo()
        {
            string defaultDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(defaultDirectoryPath + "settings.ini"))
            {
                try
                {
                    string content = File.ReadAllText(defaultDirectoryPath + "settings.ini", Encoding.Default);
                    int start = content.IndexOf("=") + 1;int end = content.Length - 1;
                    string MonitorDirr = content.Substring(start, end - start);
                    if((String.IsNullOrEmpty(MonitorDirr) == false) && Directory.Exists(MonitorDirr))
                        monitordirrglobal = MonitorDirr; 
                    else
                    {
                        MessageBox.Show("Ошибка! В файле settings.ini некорректно указан путь к папке мониторинга клиентских заметок.: Мониторим папку по умолчанию ("+ defaultDirectoryPath + ") ");
                        monitordirrglobal = defaultDirectoryPath;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка! Неудается прочитать файл settings.ini. Мониторим папку по умолчанию (" + defaultDirectoryPath + ")");
                    monitordirrglobal = defaultDirectoryPath;
                }
            }
            else
            {
                MessageBox.Show("Ошибка! Файла settings.ini не существует.Мониторим папку по умолчанию (" + defaultDirectoryPath + ")");
                monitordirrglobal = defaultDirectoryPath;
            }

        }
        /// <summary>
        /// Обновляет список всех клиентов из папки мониторинга
        /// </summary>
        void RefreshClientList()
        {
            try
            {
                string[] FilesInFolder = Directory.GetFiles(monitordirrglobal);
                clientNamecollglobal.Clear();
                foreach (var fnum in FilesInFolder)
                {
                    if (System.IO.Path.GetExtension(fnum) == ".txt")
                        clientNamecollglobal.Add(System.IO.Path.GetFileNameWithoutExtension(fnum));
                }
                    ClientsList.ItemsSource = null;
                    ClientsList.ItemsSource = clientNamecollglobal;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка! При обновлении списка клиентов " + ex.ToString());
            }

        }
        /// <summary>
        /// Сохраняет заметку в о клиенте.
        /// </summary>
        private void SaveCurrentClientNote()
        {
          if (String.IsNullOrEmpty(CurrentClient.Text))//Имя клиента. т.е название заметки пустое.
            MessageBox.Show("Ошибка сохранения заметки: не указано имя клиента в поле Client.");
             else
             {
                string savepath = monitordirrglobal + "\\" + CurrentClient.Text + ".txt";
                try
                {
                    File.WriteAllText(savepath, ClientContent.Text,Encoding.Default); //Сохраняем заметку.
                    RefreshClientList();
                    LastModifyInfo.Content = GetOwnersFileInfo(savepath);
                }
                catch(Exception e)
                {
                    MessageBox.Show("Ошибка сохранения заметки : " + e.ToString());
                }
             }
            

        }
#endregion

#region Отслеживание изменений в папке мониторинга(FileSystemWatcher) 
        private void StartFileSystemWatcher()
        {

            watcherglobal.Path = monitordirrglobal;
            watcherglobal.Filter = "*.txt";
            watcherglobal.Changed += new FileSystemEventHandler(OnChanged);
            watcherglobal.Created += new FileSystemEventHandler(OnChanged);
            watcherglobal.Deleted += new FileSystemEventHandler(OnChanged);
            watcherglobal.Renamed += new RenamedEventHandler(OnRenamed);
            watcherglobal.EnableRaisingEvents = true;

        }
        private void OnRenamed(object source,RenamedEventArgs e)
        {
            try
            {
                watcherglobal.EnableRaisingEvents = false;
                Utils_.ActionWithGuiThreadInvoke(CurrentClient, () =>
                {
                    RefreshClientList();
                });
            }
            finally
            {
                watcherglobal.EnableRaisingEvents = true;
            }
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                watcherglobal.EnableRaisingEvents = false;
                Utils_.ActionWithGuiThreadInvoke(CurrentClient, () =>
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    if (CurrentClient.Text == System.IO.Path.GetFileNameWithoutExtension(e.FullPath))
                        LoadingNoteContent();
                    RefreshClientList();
                });
            }
            finally
            {
                watcherglobal.EnableRaisingEvents = true;
            }
        }
#endregion

    }
}
