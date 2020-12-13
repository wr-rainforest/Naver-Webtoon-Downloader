using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database.Model
{
    public class Episode
    {
        public Webtoon Webtoon { get; set; }
        public string TitleId { get; set; }

        [Key]
        public long No { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public List<Image> Images { get; set; }
    }
}
