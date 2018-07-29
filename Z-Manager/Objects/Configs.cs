using System.Collections.Generic;
using Newtonsoft.Json;

public class Config
{
    [JsonProperty("ServerAddress")]
    public string ServerAddress { get; set; }

    [JsonProperty("ServerPort")]
    public string ServerPort { get; set; }

    [JsonProperty("AutoStartServer")]
    public bool? AutoStartServer { get; set; }

    [JsonProperty("AutoCheckPing")]
    public bool? AutoCheckPing { get; set; }

    [JsonProperty("AutoCheckDownloadSpeed")]
    public bool? AutoCheckDownloadSpeed { get; set; }

    [JsonProperty("AutoCheckServerProcess")]
    public bool? AutoCheckServerProcess { get; set; }

    [JsonProperty("AutoCheckServerStatus")]
    public bool? AutoCheckServerStatus { get; set; }
}

public class ConfigFile
{
    [JsonProperty("ConfigsList")]
    public IList<Config> ConfigsList { get; set; }
}
