using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.IO;

using Newtonsoft.Json;


namespace PriconneBotConsoleApp
{
    public class JsonDataManager
    {
        public string m_Token { get; } 

        public JsonDataManager(string path)
        {
            //path = @"C:\Users\ca45382\source\repos\PriconneBot\PriconneBot\botConfig.json";
            var sr = new StreamReader(path);
            var jsonData = sr.ReadToEnd();
            var data = JsonConvert.DeserializeObject<JsonData>(jsonData);
            m_Token = data.DiscordSettingValue.DisordToken;
        }

        [DataContract]
        private class JsonData
        {

            [DataMember(Name = "discord")]
            public DiscordSetupData DiscordSettingValue;

            [DataMember(Name ="database")]
            public SqlDatabase SqlConnectorValue;


            [DataContract]
            public class DiscordSetupData
            {
                [DataMember(Name ="token")]
                public string DisordToken;

                [DataMember (Name = "intents")]
                public DiscordIntentsData DiscordIntents;

                public class DiscordIntentsData
                {
                    [DataMember (Name = "members")]
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
    }
}
