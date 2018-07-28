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
        public event Action<bool> MinecraftServerStatusUpdate;
        public event Action<string> MinecraftServerStatusMessage;

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
        private string _serverAddress = "";
        private ushort _serverPort = 0;
        private bool _checkingProcess;
        private bool _checkingStatus;

        private MinecraftServerManager()
        {
            _serverProcessTimer = new Timer(OnServerProcessCheck, null, (int)TimeSpan.FromSeconds(5).TotalMilliseconds, (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
            _serverStatusTimer = new Timer(OnServerStatusCheck, null, (int)TimeSpan.FromSeconds(30).TotalMilliseconds, (int)TimeSpan.FromSeconds(60).TotalMilliseconds);

            MinecraftServerStatusMessage += OnMinecraftServerStatusMessage;
        }

        private void OnMinecraftServerStatusMessage(string message)
        {
            LoggingManager.LogMessage(message);
        }

        private void OnServerProcessCheck(object data) 
        {
			if (AllowServerProcessChecks) 
			{
				MinecraftServerStatusMessage?.Invoke("About to check server process...");
				bool processRunning = CheckServerProcess();

				MinecraftServerProcessUpdate?.Invoke(processRunning);
			}
			else 
			{
				MinecraftServerStatusMessage?.Invoke("Server process checks are not allowed, skipping");
			}
        }

        private void OnServerStatusCheck(object data) 
        {
			if (AllowServerStatusChecks) 
			{
				MinecraftServerStatusMessage?.Invoke("About to check server process...");
				MinecraftStatusDTO statusResult = GetServerStatus();

				MinecraftServerStatusUpdate?.Invoke(statusResult.ServerUp);
			}
			else 
			{
				MinecraftServerStatusMessage?.Invoke("Server status checks are not allowed, skipping");
			}
        }

        /// <summary> Start the Minecraft server via on-disk batch file </summary>
        public void StartServer()
        {
            try
            {
                MinecraftServerStatusMessage?.Invoke("Attempting server start via batch file...");

                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = BatchFileWorkingDirectory;
                proc.StartInfo.FileName = StartBatchFilePath;
                proc.Start();
            }
            catch (Exception ex)
            {
                MinecraftServerStatusMessage?.Invoke("StartServer exception: " + ex.Message);
            }
        }

        /// <summary> Check if there are running instance(s) of a Minecraft server </summary>
        public bool CheckServerProcess()
        {
            try
            {
                _checkingProcess = true;

                if (Process.GetProcessesByName("javaw").Count() > 0)
                {
                    MinecraftServerStatusMessage?.Invoke("Minecraft server appears to be running");
                    MinecraftServerStatusUpdate?.Invoke(true);
                    return true;
                }
                else
                {
                    MinecraftServerStatusMessage?.Invoke("Minecraft server process not found");
                    MinecraftServerStatusUpdate?.Invoke(false);
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
                MinecraftServerStatusMessage?.Invoke("Can't get server status because address/port do not appear to be set");
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
                        dto.CheckTime = stopWatch.ElapsedMilliseconds;

                        NetworkStream stream = client.GetStream();
                        stream.Write(payload, 0, payload.Length);
                        stream.Read(rawServerData, 0, _dataSize);
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    MinecraftServerStatusMessage?.Invoke("GetServerStatus exception: " + ex.Message);
                    dto.ServerUp = false;
                    return dto;
                }

                if (rawServerData == null || rawServerData.Length == 0)
                {
                    MinecraftServerStatusMessage?.Invoke("Server status data was not populated");
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

                        MinecraftServerStatusMessage?.Invoke("Got valid server status data");
                    }
                    else
                    {
                        MinecraftServerStatusMessage?.Invoke("Server status data was not populated as expected");
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
