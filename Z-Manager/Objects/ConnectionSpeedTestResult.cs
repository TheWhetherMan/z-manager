using System;

namespace Z_Manager.Objects
{
    public class ConnectionSpeedTestResult
    {
        public TimeSpan DownloadTimeSeconds { get; set; }
        public double DownloadSpeedBitsPerSecond { get; set; }
        public string FileSize { get; set; }
    }
}
