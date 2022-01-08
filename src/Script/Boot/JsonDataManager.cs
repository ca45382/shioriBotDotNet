using System.IO;
using System.Text.Json;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Script
{
    public static class BotConfigManager
    {
        private static BotConfigData s_ConfigData;

        public static string Token { get; private set; } = string.Empty;

        public static string SQLConnectionString { get; private set; } = string.Empty;

        public static void SetJsonConfig(string path)
        {
            var jsonData = File.ReadAllText(path);
            s_ConfigData = JsonSerializer.Deserialize<BotConfigData>(jsonData);
            Token = s_ConfigData.DiscordConfig.Token;

            SQLConnectionString = $"server = {s_ConfigData.DatabaseConfig.Host};" +
            $"port = {s_ConfigData.DatabaseConfig.Port};" +
            $"user = {s_ConfigData.DatabaseConfig.User};" +
            $"password = {s_ConfigData.DatabaseConfig.Password};" +
            $"database = {s_ConfigData.DatabaseConfig.Database};" +
            $"SslMode = {s_ConfigData.DatabaseConfig.SSLMode}";
        }
    }
}
