using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Collections.Generic;
using System;

using Brotli;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Database;

namespace PriconneBotConsoleApp.Script
{
    class BotInitialize
    {
        private const string rediveURL = "https://redive.estertion.win/";
        private readonly string DataFolderPath = Path.Combine("data");
        private readonly string TempFolderPath = Path.Combine("temp");
        private readonly string RediveJsonName = "last_version_jp.json";
        private readonly string RediveDatabaseName = "redive_jp.db";

        public BotInitialize()
        {
            if (!Directory.Exists(DataFolderPath))
            {
                Directory.CreateDirectory(DataFolderPath);
            }

            if (!Directory.Exists(TempFolderPath))
            {
                Directory.CreateDirectory(TempFolderPath);
            }

            UpdateFeatureList();
        }

        public void UpdateRediveDatabase()
        {
            var webClient = new WebClient();
            string updateRediveString;

            try
            {
                updateRediveString = webClient.DownloadString(rediveURL + RediveJsonName);
            }
            catch
            {
                return;
            }

            var rediveJsonPath = Path.Combine(DataFolderPath, RediveJsonName);

            if (string.IsNullOrEmpty(updateRediveString))
            {
                return;
            }

            var updateRediveData = LoadJson<RediveVersionData>(updateRediveString);

            if (File.Exists(rediveJsonPath))
            {
                var preRediveData = LoadJson<RediveVersionData>(File.ReadAllText(rediveJsonPath));

                if (updateRediveData == null 
                    || ( preRediveData != null && preRediveData.TruthVersion == updateRediveData.TruthVersion) )
                {
                    return;
                }
            }

            File.WriteAllText(rediveJsonPath, updateRediveString);

            var rediveDBURL = rediveURL + "db/" + RediveDatabaseName + ".br";
            var rediveDBBrotliPath = Path.Combine(TempFolderPath, RediveDatabaseName + ".br");
            var rediveDBPath = Path.Combine(DataFolderPath, RediveDatabaseName);

            webClient.DownloadFile(rediveDBURL, rediveDBBrotliPath);
            DecompressBrotli(rediveDBBrotliPath, rediveDBPath);
            File.Delete(rediveDBBrotliPath);

            return;
        }

        /// <summary>
        /// 指定されたパスのファイルを解凍する。
        /// </summary>
        /// <param name="preDecompressFilePath">解凍するファイルのパス</param>
        /// <param name="decompressedFilePath">解凍した後のファイルのパス</param>
        /// <returns></returns>
        private bool DecompressBrotli(string preDecompressFilePath, string decompressedFilePath)
        {
            try
            {
                using var preDecompressFile = new FileStream(
                    preDecompressFilePath,
                    FileMode.Open,
                    FileAccess.Read
                );

                using var decompressedFile = new FileStream(
                    decompressedFilePath,
                    FileMode.Create,
                    FileAccess.Write
                );

                using var bs = new BrotliStream(preDecompressFile, System.IO.Compression.CompressionMode.Decompress);
                bs.CopyTo(decompressedFile);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private T LoadJson<T>(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return default;
            }

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };

            var instance = JsonSerializer.Deserialize<T>(jsonString, options);
            return instance;
        }

        private void UpdateFeatureList()
        {
            var channelFeatures = new List<ChannelFeature>();
            var messageFeatures = new List<MessageFeature>();
            var roleFeatures = new List<RoleFeature>();

            foreach (var channelFeatureValue in Enum.GetValues(typeof(ChannelFeatureType)) )
            {
                ChannelFeatureType data = (ChannelFeatureType)channelFeatureValue;
                channelFeatures.Add(new ChannelFeature() 
                {
                    FeatureID = (uint)channelFeatureValue,
                    FeatureName = data.ToString(),
                });
            }

            foreach (var messageFeatureValue in Enum.GetValues(typeof(MessageFeatureType)))
            {
                MessageFeatureType data = (MessageFeatureType)messageFeatureValue;
                messageFeatures.Add(new MessageFeature()
                {
                    FeatureID = (uint)messageFeatureValue,
                    FeatureName = data.ToString(),
                });
            }

            foreach (var roleFeatureValue in Enum.GetValues(typeof(RoleFeatureType)))
            {
                RoleFeatureType data = (RoleFeatureType)roleFeatureValue;
                roleFeatures.Add(new RoleFeature()
                {
                    FeatureID = (uint)roleFeatureValue,
                    FeatureName = data.ToString(),
                });
            }

            var databaseFeatureController = new DatabaseFeatureController();
            databaseFeatureController.UpdateChannelFeature(channelFeatures);
            databaseFeatureController.UpdateMessageFeature(messageFeatures);
            databaseFeatureController.UpdateRoleFeature(roleFeatures);
        }
    }
}
