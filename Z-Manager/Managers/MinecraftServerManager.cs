using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using Z_Manager.Objects;
using System.Threading;
using System.Linq;
using System.Text;
using System;

namespace Z_Manager.Managers
{
    public class MinecraftServerManager
    {
        public event Action<bool> MinecraftServerProcessUpdate;
        public event Action<MinecraftStatusDTO> MinecraftServerStatusUpdate;
        public event Action<string> MinecraftServerManagerMessage;

        public static MinecraftServerManager _instance;
        public static MinecraftServerManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MinecraftServerManager();

                return _instance;
            }
        }

        public static bool AllowServerProcessChecks { get; set; }
        public static bool AllowServerStatusChecks { get; set; }
        public static readonly string StartBatchFilePath = @"C:\Minecraft\run.bat";
        public static readonly string BatchFileWorkingDirectory = @"C:\Minecraft\";

        private Timer _serverProcessTimer;
        private Timer _serverStatusTimer;
        private const ushort _dataSize = 512;
        private const ushort _numFields = 6;
        private string _serverAddress = "192.168.1.114";
        private ushort _serverPort = 25565;
        private bool _checkingProcess;
        private bool _checkingStatus;

        private MinecraftServerManager()
        {
            _serverProcessTimer = new Timer(OnServerProcessCheck, null, (int)TimeSpan.FromSeconds(1).TotalMilliseconds, (int)TimeSpan.FromSeconds(60).TotalMilliseconds);
            _serverStatusTimer = new Timer(OnServerStatusCheck, null, (int)TimeSpan.FromSeconds(10).TotalMilliseconds, (int)TimeSpan.FromSeconds(60).TotalMilliseconds);

            MinecraftServerManagerMessage += OnMinecraftServerStatusMessage;
        }

        private void OnMinecraftServerStatusMessage(string message)
        {
            LoggingManager.LogMessage(message);
        }

        private void OnServerProcessCheck(object data) 
        {
			if (AllowServerProcessChecks) 
			{
				MinecraftServerManagerMessage?.Invoke("About to check server process...");
				bool processRunning = CheckServerProcess();

				MinecraftServerProcessUpdate?.Invoke(processRunning);
			}
			else 
			{
				MinecraftServerManagerMessage?.Invoke("Server process checks are not allowed, skipping");
			}
        }

        private void OnServerStatusCheck(object data) 
        {
			if (AllowServerStatusChecks) 
			{
				MinecraftServerManagerMessage?.Invoke("About to check server process...");
				MinecraftStatusDTO statusResult = GetServerStatus();

				MinecraftServerStatusUpdate?.Invoke(statusResult);
			}
			else 
			{
				MinecraftServerManagerMessage?.Invoke("Server status checks are not allowed, skipping");
			}
        }

        /// <summary> Start the Minecraft server via on-disk batch file </summary>
        public void StartServer()
        {
            try
            {
                MinecraftServerManagerMessage?.Invoke("Attempting server start via batch file...");

                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = BatchFileWorkingDirectory;
                proc.StartInfo.FileName = StartBatchFilePath;
                proc.Start();
            }
            catch (Exception ex)
            {
                MinecraftServerManagerMessage?.Invoke("StartServer exception: " + ex.Message);
            }
        }

        /// <summary> Check if there are running instance(s) of a Minecraft server </summary>
        public bool CheckServerProcess()
        {
            try
            {
                if (_checkingProcess)
                {
                    MinecraftServerManagerMessage?.Invoke("Already checking server process");
                    return false;
                }

                _checkingProcess = true;

                if (Process.GetProcessesByName("java").Count() > 0)
                {
                    MinecraftServerManagerMessage?.Invoke("Minecraft server appears to be running");
                    MinecraftServerProcessUpdate?.Invoke(true);
                    return true;
                }
                else
                {
                    MinecraftServerManagerMessage?.Invoke("Minecraft server process not found");
                    MinecraftServerProcessUpdate?.Invoke(false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckServerStatus exception: " + ex.Message);
                return false;
            }
            finally
            {
                _checkingProcess = false;
            }
        }

        /// <summary> Get the status of the server by talking to it directly </summary>
        /// <see cref="https://github.com/ldilley/minestat/blob/master/CSharp/MineStat.cs"/>
        public MinecraftStatusDTO GetServerStatus()
        {
            if (string.IsNullOrEmpty(_serverAddress) || _serverPort == 0)
            {
                MinecraftServerManagerMessage?.Invoke("Can't get server status because address/port do not appear to be set");
                return null;
            }

            if (_checkingStatus)
            {
                MinecraftServerManagerMessage?.Invoke("Already checking server status");
                return null;
            }

            MinecraftStatusDTO dto = new MinecraftStatusDTO();
            byte[] rawServerData = new byte[_dataSize];

            dto.Address = _serverAddress;
            dto.Port = _serverPort;

            try
            {
                _checkingStatus = true;

                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    byte[] payload = new byte[] { 0xFE, 0x01 };

                    using (TcpClient client = new TcpClient() { SendTimeout = 3000, ReceiveTimeout = 3000 })
                    {
                        stopWatch.Start();
                        client.Connect(_serverAddress, _serverPort);
                        stopWatch.Stop();
                        dto.CheckTime = DateTime.Now;

                        NetworkStream stream = client.GetStream();
                        stream.Write(payload, 0, payload.Length);
                        stream.Read(rawServerData, 0, _dataSize);
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    MinecraftServerManagerMessage?.Invoke("GetServerStatus exception: " + ex.Message);
                    dto.ServerUp = false;
                    return dto;
                }

                if (rawServerData == null || rawServerData.Length == 0)
                {
                    MinecraftServerManagerMessage?.Invoke("Server status data was not populated");
                    dto.ServerUp = false;
                }
                else
                {
                    var serverData = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
                    if (serverData != null && serverData.Length >= _numFields)
                    {
                        dto.ServerUp = true;
                        dto.Version = serverData[2];
                        dto.Motd = serverData[3];
                        dto.CurrentPlayers = serverData[4];
                        dto.MaximumPlayers = serverData[5];

                        MinecraftServerManagerMessage?.Invoke("Got valid server status data");
                    }
                    else
                    {
                        MinecraftServerManagerMessage?.Invoke("Server status data was not populated as expected");
                        dto.ServerUp = false;
                    }
                }
            }
            finally
            {
                _checkingStatus = false;
            }

            return dto;
        }
    }
}
