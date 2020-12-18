using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database
{
    public class Image
    {
        public Webtoon Webtoon { get; set; }
        public long WebtoonID { get; set; }

        public Episode Episode { get; set; }
        public long EpisodeNo { get; set; }
        
        public long No { get; set; }

        public string ImageUrl { get; set; }

        public long Size { get; set; }

        public bool IsDownloaded { get; set; }
    }
}
