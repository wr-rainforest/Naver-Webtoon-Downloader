using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Data.Sqlite;
using System.IO;
using HtmlAgilityPack;

namespace NaverWebtoonDownloader.CoreLib
{
    public class Downloader
    {
        private static readonly HttpClient _client;

        private NameFormat _format;

        static Downloader()
        {
            var handler = new SocketsHttpHandler()
            {
                AllowAutoRedirect = false,
            };
            _client = new HttpClient(handler);
        }

        public Downloader(NameFormat format)
        {
            _format = format;
        }

        public Task DownloadAsync(string titleId, Action<string> log, IProgress<ProgressArgs> progress)
        {

        }
        
        private string ReplaceFolderName(string name)
        {
            if (name[name.Length - 1] == '.')
                name = name.Substring(0, name.Length - 1) + "．";
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }

        private string ReplaceFileName(string filename)
        {
            return filename.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
