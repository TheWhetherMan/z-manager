using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
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

        private string _lastPingTime;
        public string LastPingTime
        {
            get { return _lastPingTime; }
            set { _lastPingTime = "Last Ping Time: " + value + "ms"; OnPropertyChanged("LastPingTime"); }
        }

        private string _pingAddress;
        public string PingAddress
        {
            get { return _pingAddress; }
            set { _pingAddress = "Ping Address: " + value; OnPropertyChanged("PingAddress"); }
        }

        private string _lastDownloadTime;
        public string LastDownloadTime
        {
            get { return _lastDownloadTime; }
            set { _lastDownloadTime = "Last Download Duration: " + value + "s"; OnPropertyChanged("LastDownloadTime"); }
        }

        private string _lastDownloadSpeed;
        public string LastDownloadSpeed
        {
            get { return _lastDownloadSpeed; }
            set { _lastDownloadSpeed = "Last Download Speed: " + value + " Mb/s"; OnPropertyChanged("LastDownloadSpeed"); }
        }

        private string _lastDownloadFileSize;
        public string LastDownloadFileSize
        {
            get { return _lastDownloadFileSize; }
            set { _lastDownloadFileSize = "Last Download File Size: " + value + " bytes"; OnPropertyChanged("LastDownloadFileSize"); }
        }

        public NetworkControl()
        {
            InitializeComponent();

            Loaded += NetworkControl_Loaded;
            Unloaded += NetworkControl_Unloaded;
        }

        private void NetworkControl_Loaded(object sender, RoutedEventArgs e)
        {
            NetworkManager.Instance.PingResponseReceived += Network_PingResponseReceived;
            NetworkManager.Instance.NetworkConsoleMessage += Instance_NetworkConsoleMessage;
            NetworkManager.Instance.ConnectionSpeedTestCompleted += Network_ConnectionSpeedTestCompleted;
        }

        private void NetworkControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void Instance_NetworkConsoleMessage(string obj)
        {
            ConsoleText += ("NetworkManager message: " + obj + "\n");
        }

        private void Network_PingResponseReceived(double obj)
        {
            PingAddress = NetworkManager.PingAddress;
            LastPingTime = obj.ToString();

            ConsoleText += ("Ping test result: " + obj + "ms" + "\n");
        }

        private void Network_ConnectionSpeedTestCompleted(ConnectionSpeedTestResult obj)
        {
            // This speed seems to be off by a decimal point if divided by 1,000,000 like expected, formatting issue?
            LastDownloadSpeed = (obj.DownloadSpeedBitsPerSecond / 100000).ToString();
            LastDownloadTime = obj.DownloadTimeSeconds.TotalSeconds.ToString();
            LastDownloadFileSize = obj.FileSize;

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
                ConsoleText += "Stopping network tests because of user command";
                NetworkManager.AllowLoopTests = false;

                StartStopTestsButton.Content = "Start Network Tests";
            }
            else
            {
                ConsoleText += "Starting network tests because of user command \n";
                NetworkManager.AllowLoopTests = true;
                Task loopTask = Task.Run(async () => { await NetworkManager.Instance.LoopConnectionTests(); });

                StartStopTestsButton.Content = "Stop Network Tests";
            }
        }

        private void Cleanup()
        {
            NetworkManager.Instance.PingResponseReceived -= Network_PingResponseReceived;
            NetworkManager.Instance.NetworkConsoleMessage -= Instance_NetworkConsoleMessage;
            NetworkManager.Instance.ConnectionSpeedTestCompleted -= Network_ConnectionSpeedTestCompleted;
        }
    }
}
