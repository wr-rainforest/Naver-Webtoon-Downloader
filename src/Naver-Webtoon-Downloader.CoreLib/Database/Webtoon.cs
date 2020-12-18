using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database
{
    public class Webtoon
    {
        public long ID { get; set; }

        public string Title { get; set; }

        public string Writer { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}
