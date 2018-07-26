using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace Z_Manager.Managers
{
    public class NetworkManager
    {
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

        private static bool _downloading;

        private NetworkManager() { }

        /// <summary> Check connectivity to stable endpoint like Google </summary>
        public bool CheckInternetConnectivity()
        {
            try
            {
                int timeout = 2000;
                string host = "8.8.8.8";
                byte[] buffer = new byte[32];
                Ping myPing = new Ping();
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);

                return reply.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> Time a file download </summary>
        public async Task CheckInternetConnectionSpeed()
        {
            if (_downloading)
            {
                LoggingManager.LogMessage("Can't check connection speed because a previous check is still running");
                return;
            }
            else
            {
                LoggingManager.LogMessage("About to check connection speed");
                _downloading = true;
            }

            try
            {
                Stopwatch watch = new Stopwatch();
                byte[] data;

                using (var client = new WebClient())
                {
                    watch.Start();
                    data = await client.DownloadDataAsync(new Uri("http://dl.google.com/googletalk/googletalk-setup.exe?t=" + DateTime.Now.Ticks));
                    watch.Stop();
                }

                var speed = data.LongLength / watch.Elapsed.TotalSeconds;

                LoggingManager.LogMessage("Download duration: " + watch.Elapsed.TotalSeconds + "s");
                LoggingManager.LogMessage("File size: " + data.Length.ToString("N0") + " @ " + speed.ToString("N0") + "bps");
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckInternetConnectionSpeed exception: " + ex.Message);
            }
            finally
            {
                _downloading = false;
            }
        }

        /// <summary> Send UDP packet for another instance on the LAN to acknowledge </summary>
        public void CheckLocalNetwork()
        {

        }
    }
}
