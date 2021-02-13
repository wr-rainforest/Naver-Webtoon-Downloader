using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib
{
    public class Webtoon
    {
        public string Platform { get; set; }

        public long ID { get; set; }

        public string Title { get; set; }

        public string Writer { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}
