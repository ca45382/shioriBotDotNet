using System.Text.Json.Serialization;

namespace ShioriBot.Model
{
    public class BotConfigData
    {
        [JsonPropertyName("discord")]
        public DiscordConfig DiscordConfig { get; set; }

        [JsonPropertyName("database")]
        public DatabaseConfig DatabaseConfig { get; set; }
    }

    public class DiscordConfig
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

    public class DatabaseConfig
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("database")]
        public string Database { get; set; }

        [JsonPropertyName("sslmode")]
        public string SSLMode { get; set; }
    }
}
