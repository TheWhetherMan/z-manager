using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.IO;
using System;

namespace Z_Manager.Managers
{
    public class ConfigurationManager
    {
        public static ConfigurationManager _instance;
        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigurationManager();

                return _instance;
            }
        }

        public static string ConfigFilePath { get; set; }

        private ConfigFile _lastReadConfigs = null;

        private ConfigurationManager() { }

        public dynamic GetConfigValue(string key) 
        {
            CheckConfigFile();

            // TODO

            return null;
        }

        private ConfigFile GetConfigsFromFile()
        {
            ConfigFile readFile = new ConfigFile();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZManagerConfigs", "zConfigs.json");
            if (File.Exists(path))
            {
                try
                {
                    using (StreamReader streamReader = File.OpenText(path))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                        {
                            readFile = (ConfigFile)JToken.ReadFrom(jsonReader).ToObject(typeof(ConfigFile));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggingManager.LogMessage("GetConfigsFromFile exception: " + ex.Message);
                }
            }
            else
            {
                LoggingManager.LogMessage("GetConfigsFromFile: Config file doesn't exist!");
            }

            return readFile;
        }

        private bool CheckConfigFile() 
        {
            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZManagerConfigs");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "zConfigs.json");
                if (!File.Exists(filePath))
                {
                    JObject configJson = (JObject)JToken.FromObject(CreateNewConfigFile());

                    using (StreamWriter fileStream = File.CreateText(filePath))
                    {
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(fileStream))
                        {
                            configJson.WriteTo(jsonWriter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingManager.LogMessage("CheckConfigFile exception: " + ex.Message);
            }

            return File.Exists(ConfigFilePath);
        }

        private ConfigFile CreateNewConfigFile()
        {
            List<Config> defaultConfigs = new List<Config>
            {
                new Config()
                {
                    ServerAddress = "192.168.1.114",
                    ServerPort = "25565",
                    AutoStartServer = true,
                    AutoCheckServerProcess = true,
                    AutoCheckServerStatus = true,
                    AutoCheckPing = true,
                    AutoCheckDownloadSpeed = true
                }
            };

            return new ConfigFile() { ConfigsList = defaultConfigs };
        }
    }
}
