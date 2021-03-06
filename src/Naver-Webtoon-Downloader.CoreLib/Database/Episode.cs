﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database
{
    public class Episode
    {
        public Webtoon Webtoon { get; set; }
        public long WebtoonID { get; set; }

        public long No { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public List<Image> Images { get; set; }
    }
}
