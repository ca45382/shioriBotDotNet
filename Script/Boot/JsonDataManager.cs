using System.IO;
using System.Text.Json;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Script
{
    public static class BotConfigManager
    {
        private static BotConfigData m_configData;

        public static string Token { get; private set; } = string.Empty;

        public static string SQLConnectionString { get; private set; } = string.Empty;

        public static void SetJsonConfig(string path)
        {
            var jsonData = File.ReadAllText(path);
            m_configData = JsonSerializer.Deserialize<BotConfigData>(jsonData);
            Token = m_configData.DiscordConfig.Token;

            SQLConnectionString = $"server = {m_configData.DatabaseConfig.Host}; " +
            $"port = {m_configData.DatabaseConfig.Port}; " +
            $"user = {m_configData.DatabaseConfig.User}; " +
            $"password = {m_configData.DatabaseConfig.Password};" +
            $"database = {m_configData.DatabaseConfig.Database};" +
            $"SslMode = {m_configData.DatabaseConfig.SSLMode}";
        }
    }
}
