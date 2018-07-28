using System.Windows.Controls;
using System.ComponentModel;
using Z_Manager.Managers;
using System.Windows;

namespace Z_Manager.Controls
{
    public partial class MinecraftServerControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _consoleText;
        public string ConsoleText
        {
            get { return _consoleText; }
            set
            {
                ConsoleTextBox.Dispatcher.Invoke(() =>
                {
                    _consoleText = value;
                    OnPropertyChanged("ConsoleText");
                    ConsoleTextBox.ScrollToEnd();
                });
            }
        }

        public MinecraftServerControl()
        {
            InitializeComponent();

            Loaded += MinecraftServerControl_Loaded;
        }

        private void MinecraftServerControl_Loaded(object sender, RoutedEventArgs e)
        {
            MinecraftServerManager.Instance.MinecraftServerStatusMessage += OnMinecraftServerStatusMessage;
        }

        private void OnMinecraftServerStatusMessage(string obj)
        {
            ConsoleText += ("MinecraftServerManager message: " + obj + "\n");
        }

        private void StartStopServerButton_Click(object sender, RoutedEventArgs e)
        {
            MinecraftServerManager.Instance.StartServer();
        }
    }
}
