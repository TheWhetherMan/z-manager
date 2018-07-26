using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using System;

namespace Z_Manager.Managers
{
    public static class LoggingManager
    {
        private static string _logPath = @"C:\temp\zManagerLog.txt";

        static LoggingManager()
        {
            // TODO: Get logging path
        }

        public static void LogMessage(string message)
        {
            if (!Directory.Exists(Path.GetTempPath()))
                Directory.CreateDirectory(Path.GetTempPath());

            using (StreamWriter writer = File.AppendText(Path.GetTempPath() + "zManagerLog.txt"))
            {
                writer.Write(DateTime.Now.ToString("MM.dd.yyyy (ddd) HH:mm:ss") + " :: ");
                writer.Write(message);
                writer.WriteLine();
            }
        }
    }
}
