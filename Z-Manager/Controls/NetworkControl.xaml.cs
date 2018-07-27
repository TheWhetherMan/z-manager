using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using Z_Manager.Managers;
using Z_Manager.Objects;
using System.Windows;

namespace Z_Manager.Controls
{
    public partial class NetworkControl : UserControl, INotifyPropertyChanged
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

        public NetworkControl()
        {
            InitializeComponent();

            Loaded += NetworkControl_Loaded;
        }

        private void NetworkControl_Loaded(object sender, RoutedEventArgs e)
        {
            NetworkManager.Instance.PingResponseReceived += Network_PingResponseReceived;
            NetworkManager.Instance.NetworkConsoleMessage += Instance_NetworkConsoleMessage;
            NetworkManager.Instance.ConnectionSpeedTestCompleted += Network_ConnectionSpeedTestCompleted;
        }

        private void Instance_NetworkConsoleMessage(string obj)
        {
            ConsoleText += ("Networking message: " + obj + "\n");
        }

        private void Network_PingResponseReceived(double obj)
        {
            ConsoleText += ("Ping test result: " + obj + "ms" + "\n");
        }

        private void Network_ConnectionSpeedTestCompleted(ConnectionSpeedTestResult obj)
        {
            ConsoleText += ("Speed test result: " + obj.DownloadSpeedBitsPerSecond + "bps" + "\n");
        }

        private async void StartTestsButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkManager.AllowLoopTests = true;
            await NetworkManager.Instance.LoopConnectionTests();
        }

        private void StartStopTestLoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkManager.AllowLoopTests)
            {
                NetworkManager.AllowLoopTests = false;
                StartStopTestsButton.Content = "Start Network Tests";
            }
            else
            {
                NetworkManager.AllowLoopTests = true;
                Task loopTask = Task.Run(async () => { await NetworkManager.Instance.LoopConnectionTests(); });
                StartStopTestsButton.Content = "Stop Network Tests";
            }
        }
    }
}
