using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using Z_Manager.Managers;
using Z_Manager.Objects;
using System.Windows;
using System;

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
            UnhookSubscriptions();
            NetworkManager.Instance.PingResponseReceived += Network_PingResponseReceived;
            NetworkManager.Instance.NetworkConsoleMessage += NetworkConsoleMessageUpdate;
            NetworkManager.Instance.ConnectionSpeedTestCompleted += Network_ConnectionSpeedTestCompleted;
        }

        private void NetworkControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void NetworkConsoleMessageUpdate(string obj)
        {
            ConsoleText += (DateTime.Now.ToString("MM.dd HH:mm:ss") + " :: " + obj + "\n");
        }

        private void Network_PingResponseReceived(double obj)
        {
            PingAddress = NetworkManager.PingAddress;
            LastPingTime = obj.ToString();

            ConsoleText += (DateTime.Now.ToString("MM.dd HH:mm:ss") + " :: " + "Ping result: " + obj + "ms" + "\n");
        }

        private void Network_ConnectionSpeedTestCompleted(ConnectionSpeedTestResult obj)
        {
            // This speed seems to be off by a decimal point if divided by 1,000,000 like expected, formatting issue?
            LastDownloadSpeed = (obj.DownloadSpeedBitsPerSecond / 100000).ToString();
            LastDownloadTime = obj.DownloadTimeSeconds.TotalSeconds.ToString();
            LastDownloadFileSize = obj.FileSize;

            ConsoleText += (DateTime.Now.ToString("MM.dd HH:mm:ss") + " :: " + "Download test completed" + "\n");
        }

        private void Cleanup()
        {
            UnhookSubscriptions();
        }

        private void UnhookSubscriptions()
        {
            NetworkManager.Instance.PingResponseReceived -= Network_PingResponseReceived;
            NetworkManager.Instance.NetworkConsoleMessage -= NetworkConsoleMessageUpdate;
            NetworkManager.Instance.ConnectionSpeedTestCompleted -= Network_ConnectionSpeedTestCompleted;
        }

        private void TogglePingTestsButton_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkManager.AllowPingTests)
            {
                ConsoleText += "Stopping ping tests because of user command \n";
                NetworkManager.AllowPingTests = false;

                TogglePingTestsButton.Content = "Allow Ping Tests";
            }
            else
            {
                ConsoleText += "Starting ping tests because of user command \n";
                NetworkManager.AllowPingTests = true;

                TogglePingTestsButton.Content = "Disallow Ping Tests";
            }
        }

        private void ToggleDownloadTestsButton_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkManager.AllowDownloadTests)
            {
                ConsoleText += "Stopping download tests because of user command \n";
                NetworkManager.AllowDownloadTests = false;

                TogglePingTestsButton.Content = "Allow Download Tests";
            }
            else
            {
                ConsoleText += "Starting download tests because of user command \n";
                NetworkManager.AllowDownloadTests = true;

                TogglePingTestsButton.Content = "Disallow Download Tests";
            }
        }
    }
}
