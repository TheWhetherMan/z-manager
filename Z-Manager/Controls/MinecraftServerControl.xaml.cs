using System.Windows.Controls;
using System.ComponentModel;
using Z_Manager.Managers;
using Z_Manager.Objects;
using System.Windows;
using System;

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

        private string _serverAddress;
        public string ServerAddress
        {
            get { return _serverAddress; }
            set { _serverAddress = value; OnPropertyChanged("ServerAddress"); }
        }

        private string _lastServerStatusTime;
        public string LastServerStatusTime
        {
            get { return _lastServerStatusTime; }
            set { _lastServerStatusTime = value; OnPropertyChanged("LastServerStatusTime"); }
        }

        private string _processCheck;
        public string ServerProcessCheck
        {
            get { return _processCheck; }
            set { _processCheck = value; OnPropertyChanged("ServerProcessCheck"); }
        }

        private string _serverVersion;
        public string ServerVersion
        {
            get { return _serverVersion; }
            set { _serverVersion = value; OnPropertyChanged("ServerVersion"); }
        }

        private string _serverMessageOfTheDay;
        public string ServerMessageOfTheDay
        {
            get { return _serverMessageOfTheDay; }
            set { _serverMessageOfTheDay = value; OnPropertyChanged("ServerMessageOfTheDay"); }
        }

        private string _playerCount;
        public string ServerPlayerCount
        {
            get { return _playerCount; }
            set { _playerCount = value; OnPropertyChanged("ServerPlayerCount"); }
        }

        public MinecraftServerControl()
        {
            InitializeComponent();

            Loaded += MinecraftServerControl_Loaded;
        }

        private void MinecraftServerControl_Loaded(object sender, RoutedEventArgs e)
        {
            UnhookSubscriptions();
            MinecraftServerManager.Instance.MinecraftServerManagerMessage += ServerManager_OnStatusMessage;
            MinecraftServerManager.Instance.MinecraftServerProcessUpdate += Server_OnMinecraftProcessUpdate;
            MinecraftServerManager.Instance.MinecraftServerStatusUpdate += Server_OnMinecraftStatusUpdate;
        }

        private void ServerManager_OnStatusMessage(string obj)
        {
            ConsoleText += (DateTime.Now.ToString("MM.dd HH:mm:ss") + " :: " + obj + "\n");
        }

        private void Server_OnMinecraftProcessUpdate(bool obj)
        {
            ServerProcessCheck = "Server Process Running: " + obj;
        }

        private void Server_OnMinecraftStatusUpdate(MinecraftStatusDTO obj)
        {
            ServerManager_OnStatusMessage("Received status update");
            if (obj != null)
            {
                ServerAddress         = "Server Address: " + obj.Address + " : " + obj.Port;
                LastServerStatusTime  = "Last Server Status Update: " + obj.CheckTime.ToString("MM.dd HH:mm:ss");
                ServerVersion         = "Server Version: v" + obj.Version;
                ServerMessageOfTheDay = "Server MOTD: '" + obj.Motd + "'";
                ServerPlayerCount     = "Server Players: " + obj.CurrentPlayers + "/" + obj.MaximumPlayers;
            }
            else
            {
                ServerManager_OnStatusMessage("Update object is null, failed to get server status");
            }
        }

        private void StartStopServerButton_Click(object sender, RoutedEventArgs e)
        {
            MinecraftServerManager.Instance.StartServer();
        }

        private void AllowServerProcessChecksButton_Click(object sender, RoutedEventArgs e)
        {
            if (MinecraftServerManager.AllowServerProcessChecks)
            {
                ServerManager_OnStatusMessage("Disallowing process checks because of user command");
                MinecraftServerManager.AllowServerProcessChecks = false;

                AllowServerProcessChecksButton.Content = "Allow Status Checks";
            }
            else
            {
                ServerManager_OnStatusMessage("Allowing process checks because of user command");
                MinecraftServerManager.AllowServerProcessChecks = true;

                AllowServerProcessChecksButton.Content = "Disallow Status Checks";
            }
        }

        private void AllowServerStatusChecksButton_Click(object sender, RoutedEventArgs e)
        {
            if (MinecraftServerManager.AllowServerStatusChecks)
            {
                ServerManager_OnStatusMessage("Disallowing status checks because of user command");
                MinecraftServerManager.AllowServerStatusChecks = false;

                AllowServerStatusChecksButton.Content = "Allow Status Checks";
            }
            else
            {
                ServerManager_OnStatusMessage("Allowing status checks because of user command");
                MinecraftServerManager.AllowServerStatusChecks = true;

                AllowServerStatusChecksButton.Content = "Disallow Status Checks";
            }
        }

        private void UnhookSubscriptions()
        {
            MinecraftServerManager.Instance.MinecraftServerManagerMessage -= ServerManager_OnStatusMessage;
            MinecraftServerManager.Instance.MinecraftServerProcessUpdate -= Server_OnMinecraftProcessUpdate;
            MinecraftServerManager.Instance.MinecraftServerStatusUpdate -= Server_OnMinecraftStatusUpdate;
        }
    }
}
