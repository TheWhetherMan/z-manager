using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using Z_Manager.Objects;
using System.Linq;
using System.Text;
using System;

namespace Z_Manager.Managers
{
    public class MinecraftServerManager
    {
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

        public static bool AllowLoopServerStatusChecks { get; set; }
        public static readonly string StartBatchFilePath = @"C:\Minecraft\run.bat";

        private Stopwatch _statusLoopTimer;
        private Stopwatch _processLoopTimer;
        private static bool _serverCheckLoopRunning;
        private static bool _processCheckLoopRunning;
        private const ushort _dataSize = 512;
        private const ushort _numFields = 6;
        private string _serverAddress = "";
        private ushort _serverPort = 0;
        private bool _checkingProcess;
        private bool _checkingStatus;

        private MinecraftServerManager()
        {
            MinecraftServerStatusMessage += OnMinecraftServerStatusMessage;
        }

        private void OnMinecraftServerStatusMessage(string message)
        {
            LoggingManager.LogMessage(message);
        }

        public Task LoopStatusChecks()
        {
            if (_serverCheckLoopRunning)
            {
                MinecraftServerStatusMessage?.Invoke("Server check task already running");
                return null;
            }
            else
            {
                MinecraftServerStatusMessage?.Invoke("Starting server check task");
            }

            try
            {
                Task loop = Task.Run(async () => { await CheckServerTask(); });
            }
            catch (Exception ex)
            {
                MinecraftServerStatusMessage?.Invoke("LoopStatusChecks exception: " + ex.Message);
            }

            return null;
        }

        private async Task CheckServerTask()
        {
            _serverCheckLoopRunning = true;
            _statusLoopTimer.Start();
            _processLoopTimer.Start();

            while (AllowLoopServerStatusChecks)
            {
                if (_processLoopTimer.ElapsedMilliseconds > 30000)
                {
                    _statusLoopTimer.Stop();
                    _processLoopTimer.Stop();

                    MinecraftServerStatusMessage?.Invoke("About to check server process...");
                    bool processRunning = CheckServerProcess();

                    _statusLoopTimer.Start();
                    _processLoopTimer.Restart();
                }

                if (_statusLoopTimer.ElapsedMilliseconds > 60000)
                {
                    _statusLoopTimer.Stop();
                    _processLoopTimer.Stop();

                    MinecraftServerStatusMessage?.Invoke("About to check server process...");
                    MinecraftStatusDTO processRunning = GetServerStatus();

                    _processLoopTimer.Start();
                    _statusLoopTimer.Restart();
                }

                await Task.Delay(1000);
            }

            _serverCheckLoopRunning = false;
        }

        /// <summary> Check if there are running instance(s) of a Minecraft server </summary>
        public bool CheckServerProcess()
        {
            try
            {
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

            }
        }

        /// <summary> Start the Minecraft server via on-disk batch file </summary>
        public void StartServer()
        {
            try
            {
                MinecraftServerStatusMessage?.Invoke("Attempting server start via batch file...");
                Process.Start(StartBatchFilePath);
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("StartServer exception: " + ex.Message);
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

            return dto;
        }
    }
}
