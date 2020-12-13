using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database.Model
{
    public class Image
    {
        public Webtoon Webtoon { get; set; }
        public string TitleId { get; set; }

        public Episode Episode { get; set; }
        public long EpisodeNo { get; set; }

        [Key]
        public long ImageNo { get; set; }

        public string FilePath { get; set; }

        public string Url { get; set; }

        public int Size { get; set; }

        public bool IsDownloaded { get; set; }
    }
}
