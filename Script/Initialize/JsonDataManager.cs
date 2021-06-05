using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;

namespace PriconneBotConsoleApp.Script.Initialize
{
    public class JsonDataManager
    {
        public string Token { get; }

        private static BotConfigSchema m_configData;

        public JsonDataManager()
        {
        }

        public JsonDataManager(string path)
        {
            var sr = new StreamReader(path);
            var jsonData = sr.ReadToEnd();
            m_configData = JsonConvert.DeserializeObject<BotConfigSchema>(jsonData);
            Token = m_configData.DiscordSettingValue.DisordToken;
        }

        public string MySQLConnectionString =>
            $"server = {m_configData.SqlConnectorValue.Host}; " +
            $"port = {m_configData.SqlConnectorValue.Port}; " +
            $"user = {m_configData.SqlConnectorValue.User}; " +
            $"password = {m_configData.SqlConnectorValue.Password};" +
            $"database = {m_configData.SqlConnectorValue.Database};" +
            $"SslMode = {m_configData.SqlConnectorValue.sslMode}";

        [DataContract]
        private class BotConfigSchema
        {
#pragma warning disable CS0649
            [DataMember(Name = "discord")]
            public DiscordSetupData DiscordSettingValue;

            [DataMember(Name = "database")]
            public SqlDatabase SqlConnectorValue;

            [DataContract]
            public class DiscordSetupData
            {
                [DataMember(Name = "token")]
                public string DisordToken;

                [DataMember(Name = "intents")]
                public DiscordIntentsData DiscordIntents;

                public class DiscordIntentsData
                {
                    [DataMember(Name = "members")]
                    public bool members;
                }
            }

            [DataContract]
            public class SqlDatabase
            {
                [DataMember(Name = "host")]
                public string Host;

                [DataMember(Name = "port")]
                public int Port;

                [DataMember(Name = "user")]
                public string User;

                [DataMember(Name = "password")]
                public string Password;

                [DataMember(Name = "database")]
                public string Database;

                [DataMember(Name = "sslmode")]
                public string sslMode;
            }
        }
#pragma warning restore CA0649
    }
}
