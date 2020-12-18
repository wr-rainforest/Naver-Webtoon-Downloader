using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib
{
    public static class GlobalStatic
    {
        public static string AppDataFolderPath { get; }

        public static string WebtoonDatabaseFilePath { get; }

        public static string ConfigFilePath { get; }

        static GlobalStatic()
        {
            AppDataFolderPath = $"AppData";
            if (!Directory.Exists(AppDataFolderPath))
                Directory.CreateDirectory(AppDataFolderPath);
            WebtoonDatabaseFilePath = $"{AppDataFolderPath}\\webtoon.sqlite";
            ConfigFilePath = $"{AppDataFolderPath}\\config.json";
        }
    }
}
