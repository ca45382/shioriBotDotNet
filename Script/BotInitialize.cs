using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;

using Brotli;

namespace PriconneBotConsoleApp.Script
{
    class BotInitialize
    {
        private const string rediveURL = "https://redive.estertion.win/";
        private readonly string DataFolderPath = Path.Combine("data");
        private readonly string TempFolderPath = Path.Combine("temp");
        private readonly string RediveJsonName = "last_version_jp.json";
        private readonly string RediveDBName = "redive_jp.db";

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
        }

        public void UpdateRediveDB()
        {
            var webClient = new WebClient();
            var updateRediveString = webClient.DownloadString(rediveURL + RediveJsonName);
            var rediveJsonPath = Path.Combine(DataFolderPath, RediveJsonName);

            if (updateRediveString == null)
            {
                return;
            }

            var updateRediveData = LoadJson<RediveUpdateJsonData>(updateRediveString);

            if (File.Exists(rediveJsonPath))
            {
                var preRediveData = LoadJson<RediveUpdateJsonData>(File.ReadAllText(rediveJsonPath));

                if (updateRediveData == null 
                    || ( preRediveData != null && preRediveData.TruthVersion == updateRediveData.TruthVersion) )
                {
                    return;
                }
            }

            File.WriteAllText(rediveJsonPath, updateRediveString);

            var rediveDBURL = rediveURL + "db/" + RediveDBName + ".br";
            var rediveDBBrotliPath = Path.Combine(TempFolderPath, RediveDBName + ".br");
            var rediveDBPath = Path.Combine(DataFolderPath, RediveDBName);

            webClient.DownloadFile(rediveDBURL, rediveDBBrotliPath);
            DecompressDB(rediveDBBrotliPath, rediveDBPath);
            File.Delete(rediveDBBrotliPath);

            return;
        }



        /// <summary>
        /// 指定されたパスのファイルを解凍する。
        /// </summary>
        /// <param name="preDecompressFilePath">解凍するファイルのパス</param>
        /// <param name="decompressedFilePath">解凍した後のファイルのパス</param>
        /// <returns></returns>
        private bool DecompressDB(string preDecompressFilePath, string decompressedFilePath)
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
            var json = jsonString;
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };

            var instance = JsonSerializer.Deserialize<T>(json, options);
            return instance;
        }
    }


    public class RediveUpdateJsonData
    {
        public string TruthVersion { get; set; }
        public string Hash { get; set; }
        public string PrefabVer { get; set; }
    }
}
