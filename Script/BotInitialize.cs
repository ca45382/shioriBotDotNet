using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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


    }
}
