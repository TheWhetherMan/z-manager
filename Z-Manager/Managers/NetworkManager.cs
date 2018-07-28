using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Diagnostics;
using Z_Manager.Objects;
using System.Threading;
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

        public static bool AllowPingTests { get; set; }
        public static bool AllowDownloadTests { get; set; }
        public static readonly string DownloadURL = "http://dl.google.com/chrome/install/154.36/chrome_installer.exe";
        public static readonly string PingAddress = "8.8.8.8";

        private static bool _downloading;
        private static bool _blockPingTest;
        private Ping _connectionPing;
        private Timer _pingTestTimer;
        private Timer _downloadTestTimer;

        private NetworkManager()
        {
            _connectionPing = new Ping();
            _pingTestTimer = new Timer(OnFirePingTest, null, (int)TimeSpan.FromSeconds(5).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            _downloadTestTimer = new Timer(OnFireDownloadTest, null, (int)TimeSpan.FromSeconds(10).TotalMilliseconds, (int)TimeSpan.FromSeconds(10).TotalMilliseconds);
        }

        private async void OnFirePingTest(object data) 
        {
			if (AllowPingTests && !_blockPingTest) 
			{
				var ping = await CheckInternetConnectivityViaPing();
                HandlePingTestCompletion(ping);
			}
        }

        private async void OnFireDownloadTest(object data) 
        {
			if (AllowDownloadTests) 
			{
                _blockPingTest = true;

                try 
                {
                    NetworkConsoleMessage?.Invoke("About to test download speed...");
                    var speed = await CheckInternetConnectionSpeed();
                    HandleSpeedTestCompletion(speed);
                }
                finally
                {
                    _blockPingTest = false;
                }
			}
			else 
			{
				NetworkConsoleMessage?.Invoke("Download tests not allowed, skipping");
			}
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
                LoggingManager.LogMessage("Ping test result: Failed to get a response from " + PingAddress);
            }
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
        }

        /// <summary> Check connectivity to stable endpoint like Google </summary>
        public async Task<PingReply> CheckInternetConnectivityViaPing()
        {
            try
            {
                int timeout = 2000;
                byte[] buffer = new byte[32];
                PingOptions pingOptions = new PingOptions();
                
                return await Task.Run(async () =>
                {
                    return await _connectionPing.SendPingAsync(PingAddress, timeout, buffer, pingOptions);
                });
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckInternetConnectivityViaPing exception: " + ex.Message);
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
                        data = await client.DownloadDataTaskAsync(new Uri(DownloadURL));
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
