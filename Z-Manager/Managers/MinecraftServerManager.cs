using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
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

        public static readonly string StartBatchFilePath = @"C:\Minecraft\run.bat";

        private MinecraftServerManager()
        {
            MinecraftServerStatusMessage += OnMinecraftServerStatusMessage;
        }

        private void OnMinecraftServerStatusMessage(string message)
        {
            LoggingManager.LogMessage(message);
        }

        /// <summary> Check if there are running instance(s) of a Minecraft server </summary>
        public bool CheckServerStatus()
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
        }

        /// <summary> Start the Minecraft server </summary>
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
    }
}
