using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Diagnostics;
using Z_Manager.Objects;
using System.Net;
using System;

namespace Z_Manager.Managers
{
    public class NetworkManager
    {
        public event Action<string> NetworkConsoleMessage;
        public event Action<double> PingResponseReceived;
        public event Action<ConnectionSpeedTestResult> ConnectionSpeedTestCompleted;

        public static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkManager();

                return _instance;
            }
        }

        public static bool AllowLoopTests { get; set; }

        private Ping _connectionPing;
        private Stopwatch _pingTestTimer;
        private Stopwatch _downloadTestTimer;
        private static bool _testTaskLoopRunning;
        private static bool _downloading;
        private static bool _pinging;

        private NetworkManager()
        {
            _connectionPing = new Ping();
            _pingTestTimer = new Stopwatch();
            _downloadTestTimer = new Stopwatch();
        }

        public Task LoopConnectionTests()
        {
            if (_testTaskLoopRunning)
            {
                LoggingManager.LogMessage("LoopConnectionTests: Test loop task already running");
                NetworkConsoleMessage?.Invoke("Test loop task already running");
                return null;
            }
            else
            {
                LoggingManager.LogMessage("LoopConnectionTests: Starting tests");
                NetworkConsoleMessage?.Invoke("Starting tests");
            }

            try
            {
                Task loop = Task.Run(async () =>
                {
                    _testTaskLoopRunning = true;
                    _pingTestTimer.Start();
                    _downloadTestTimer.Start();

                    while (AllowLoopTests)
                    {
                        if (_downloadTestTimer.ElapsedMilliseconds > 10000 && !_downloading)
                        {
                            _pingTestTimer.Stop();
                            _downloadTestTimer.Stop();

                            NetworkConsoleMessage?.Invoke("About to test download speed, stopping test timers");
                            var speed = await CheckInternetConnectionSpeed();
                            HandleSpeedTestCompletion(speed);

                            continue;
                        }

                        if (_pingTestTimer.ElapsedMilliseconds > 1000 && !_pinging)
                        {
                            _pingTestTimer.Stop();
                            _downloadTestTimer.Stop();
                            
                            var ping = await CheckInternetConnectivityViaPing();
                            HandlePingTestCompletion(ping);

                            continue;
                        }

                        await Task.Delay(250);
                    }

                    _testTaskLoopRunning = false;
                });
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("LoopConnectionTests exception: " + ex.Message);
            }

            return null;
        }

        private void HandlePingTestCompletion(PingReply ping)
        {
            if (ping != null)
            {
                LoggingManager.LogMessage("Ping test result: " + ping.RoundtripTime + "ms");
                PingResponseReceived?.Invoke(ping.RoundtripTime);
            }
            else
            {
                LoggingManager.LogMessage("Ping test result: Failed to get a response");
            }

            _downloadTestTimer.Start();
            _pingTestTimer.Restart();
        }

        private void HandleSpeedTestCompletion(ConnectionSpeedTestResult speed)
        {
            if (speed != null)
            {
                LoggingManager.LogMessage("Download speed result: " + speed.DownloadSpeedBitsPerSecond + "");
                ConnectionSpeedTestCompleted?.Invoke(speed);
            }
            else
            {
                LoggingManager.LogMessage("Speed test result: Failed to complete download task");
            }

            _downloadTestTimer.Restart();
            _pingTestTimer.Restart();
        }

        /// <summary> Check connectivity to stable endpoint like Google </summary>
        public async Task<PingReply> CheckInternetConnectivityViaPing()
        {
            try
            {
                int timeout = 2000;
                string host = "8.8.8.8";
                byte[] buffer = new byte[32];
                PingOptions pingOptions = new PingOptions();
                
                return await Task.Run(async () =>
                {
                    _pinging = true;
                    return await _connectionPing.SendPingAsync(host, timeout, buffer, pingOptions);
                });
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckInternetConnectivityViaPing exception: " + ex.Message);
            }
            finally
            {
                _pinging = false;
            }

            LoggingManager.LogMessage("CheckInternetConnectivityViaPing must have failed");
            return null;
        }

        /// <summary> Time a file download </summary>
        public async Task<ConnectionSpeedTestResult> CheckInternetConnectionSpeed()
        {
            if (_downloading)
            {
                LoggingManager.LogMessage("Can't check connection speed because a previous check is still running");
                return null;
            }
            else
            {
                LoggingManager.LogMessage("About to check connection speed");
                _downloading = true;
            }

            try
            {
                return await Task.Run(async () => 
                { 
                    Stopwatch watch = new Stopwatch();
                    byte[] data;

                    using (var client = new WebClient())
                    {
                        watch.Start();
                        data = await client.DownloadDataTaskAsync(new Uri("http://dl.google.com/googletalk/googletalk-setup.exe?t=" + DateTime.Now.Ticks));
                        watch.Stop();
                    }

                    var speed = data.LongLength / watch.Elapsed.TotalSeconds;

                    LoggingManager.LogMessage("Download duration: " + watch.Elapsed.TotalSeconds + "s");
                    LoggingManager.LogMessage("File size: " + data.Length.ToString("N0") + " @ " + speed.ToString("N0") + "bps");

                    return new ConnectionSpeedTestResult()
                    {
                        DownloadSpeedBitsPerSecond = speed,
                        DownloadTimeSeconds = watch.Elapsed,
                        FileSize = data.Length.ToString("N0")
                    };
                });
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckInternetConnectionSpeed exception: " + ex.Message);
            }
            finally
            {
                _downloading = false;
            }

            return null;
        }

        /// <summary> Send UDP packet for another instance on the LAN to acknowledge </summary>
        public void CheckLocalNetwork()
        {

        }
    }
}
