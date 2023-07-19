using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Collections.Generic;
using System.IO.Compression;
using System;

using ShioriBot.Model;
using ShioriBot.DataType;
using ShioriBot.Database;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShioriBot.Script
{
    public class BotInitialize
    {
        private const string rediveURL = "https://redive.estertion.win/";
        private static readonly string DataFolderPath = Path.Combine("data");
        private static readonly string TempFolderPath = Path.Combine("temp");
        private static readonly string RediveJsonName = "last_version_jp.json";
        private static readonly string RediveDatabaseName = "redive_jp.db";

        public BotInitialize()
        {
            UpdateFeatureList();
        }
        
        /// <summary>
        /// Rediveデータの更新
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateRediveDatabase()
        {
            if(!MakeSaveFolder())
            {
                return;
            }

            var httpClient = new HttpClient();
            string updateRediveString;

            try
            {
                updateRediveString = await httpClient.GetStringAsync(rediveURL + RediveJsonName);
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
                    || (preRediveData != null && preRediveData.TruthVersion == updateRediveData.TruthVersion))
                {
                    return;
                }
            }

            File.WriteAllText(rediveJsonPath, updateRediveString);

            var rediveDBURL = rediveURL + "db/" + RediveDatabaseName + ".br";
            var rediveDBBrotliPath = Path.Combine(TempFolderPath, RediveDatabaseName + ".br");
            var rediveDBPath = Path.Combine(DataFolderPath, RediveDatabaseName);

            var rediveResult = await httpClient.GetAsync(rediveDBURL);

            if (rediveResult.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            using (var stream = await rediveResult.Content.ReadAsStreamAsync())
            using (var outStream = File.Create(rediveDBBrotliPath))
            {
                stream.CopyTo(outStream);
            }

            if (!DecompressBrotli(rediveDBBrotliPath, rediveDBPath))
            {
                Console.WriteLine("False");
            }

            File.Delete(rediveDBBrotliPath);

            return;
        }

        /// <summary>
        /// 指定されたパスのファイルを解凍する。
        /// </summary>
        /// <param name="preDecompressFilePath">解凍するファイルのパス</param>
        /// <param name="decompressedFilePath">解凍した後のファイルのパス</param>
        /// <returns></returns>
        private static bool DecompressBrotli(string preDecompressFilePath, string decompressedFilePath)
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

                using var bs = new BrotliStream(preDecompressFile, CompressionMode.Decompress);
                bs.CopyTo(decompressedFile);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Jsonデータをデシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private static T LoadJson<T>(string jsonString)
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

            return JsonSerializer.Deserialize<T>(jsonString, options);
        }

        /// <summary>
        /// 保存するフォルダを生成する.
        /// </summary>
        private static bool MakeSaveFolder()
        {
            try
            {
                if (!Directory.Exists(DataFolderPath))
                {
                    Directory.CreateDirectory(DataFolderPath);
                }

                if (!Directory.Exists(TempFolderPath))
                {
                    Directory.CreateDirectory(TempFolderPath);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static void UpdateFeatureList()
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

            DatabaseFeatureController.UpdateChannelFeature(channelFeatures);
            DatabaseFeatureController.UpdateMessageFeature(messageFeatures);
            DatabaseFeatureController.UpdateRoleFeature(roleFeatures);
        }
    }
}
