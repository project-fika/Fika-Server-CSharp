using System.Text.Json.Serialization;

namespace FikaWebApp.Models
{
    public record FikaConfigServer
    {
        [JsonPropertyName("SPT")]
        public FikaSPTServerConfig SPT { get; set; } = new();

        [JsonPropertyName("allowItemSending")]
        public bool AllowItemSending { get; set; } = true;

        [JsonPropertyName("itemSendingStorageTime")]
        public int ItemSendingStorageTime { get; set; } = 7;

        [JsonPropertyName("sentItemsLoseFIR")]
        public bool SentItemsLoseFIR { get; set; } = true;

        [JsonPropertyName("launcherListAllProfiles")]
        public bool LauncherListAllProfiles { get; set; } = false;

        [JsonPropertyName("sessionTimeout")]
        public int SessionTimeout { get; set; } = 5;

        [JsonPropertyName("showDevProfile")]
        public bool ShowDevProfile { get; set; } = false;

        [JsonPropertyName("showNonStandardProfile")]
        public bool ShowNonStandardProfile { get; set; } = false;

        [JsonPropertyName("adminIds")]
        public List<string> AdminIds { get; set; } = [];

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;
    }

    public record FikaSPTServerConfig
    {
        [JsonPropertyName("http")]
        public FikaSPTHttpServerConfig Http { get; set; } = new();

        [JsonPropertyName("disableSPTChatBots")]
        public bool DisableSPTChatBots { get; set; } = true;
    }

    public record FikaSPTHttpServerConfig
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = "0.0.0.0";

        [JsonPropertyName("port")]
        public int Port { get; set; } = 6969;

        [JsonPropertyName("backendIp")]
        public string BackendIp { get; set; } = "0.0.0.0";

        [JsonPropertyName("backendPort")]
        public int BackendPort { get; set; } = 6969;
    }
}