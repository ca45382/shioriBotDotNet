using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Brotli;

namespace PriconneBotConsoleApp.Script
{
    class BotInitialize
    {
        private readonly string DataFolderPath = Path.Combine("data");
        private readonly string CacheFolderPath = Path.Combine("cache");
        public BotInitialize()
        {
            if ( !Directory.Exists(DataFolderPath) )
            {
                Directory.CreateDirectory(DataFolderPath);
            }

            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
        }

        public bool DecompressDB()
        {
            try
            {
                using var rediveDataFile = new FileStream(
                    Path.Combine(CacheFolderPath, "redive_jp.db.br"),
                    FileMode.Open,
                    FileAccess.Read
                );

                using var rediveDecompressedFile = new FileStream(
                    Path.Combine(DataFolderPath, "redive_jp.db"),
                    FileMode.Create,
                    FileAccess.Write
                );

                using var bs = new BrotliStream(rediveDataFile, System.IO.Compression.CompressionMode.Decompress);
                bs.CopyTo(rediveDecompressedFile);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
