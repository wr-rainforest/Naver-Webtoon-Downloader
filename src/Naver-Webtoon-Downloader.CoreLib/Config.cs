using NaverWebtoonDownloader.CoreLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader
{
    public class Config
    {
        public int MaxConnections { get; set; } = 4;

        public string DownloadFolder { get; set; } = "Downloads";

        public NameFormat NameFormat { get; set; } = new NameFormat();
    }
}
