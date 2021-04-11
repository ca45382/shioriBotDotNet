using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.IO;

using Newtonsoft.Json;


namespace PriconneBotConsoleApp.Script
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
            //path = @"C:\Users\ca45382\source\repos\PriconneBot\PriconneBot\botConfig.json";
            var sr = new StreamReader(path);
            var jsonData = sr.ReadToEnd();
            m_configData = JsonConvert.DeserializeObject<BotConfigSchema>(jsonData);
            Token = m_configData.DiscordSettingValue.DisordToken;
        }

        public string MySQLConnectionString()
        {
            var hostName = m_configData.SqlConnectorValue.Host;
            var portNumber = m_configData.SqlConnectorValue.Port;
            var userName = m_configData.SqlConnectorValue.User;
            var password = m_configData.SqlConnectorValue.Password;
            var databaseName = m_configData.SqlConnectorValue.Database;

            var connectionString =
                $"server = {hostName}; " +
                $"port = {portNumber}; " +
                $"user = {userName}; " +
                $"password = {password};" + 
                $"database = {databaseName}";
            return connectionString;
        }

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
            }
        }
#pragma warning restore CA0649
    }
}
