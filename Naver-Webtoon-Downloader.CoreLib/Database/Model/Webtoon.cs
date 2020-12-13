using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib.Database.Model
{
    public class Webtoon
    {
        [Key]
        public string TitleId { get; set; }

        public string Title { get; set; }

        public string Writer { get; set; }

        public string[] Genre { get; set; }

        public string Description { get; set; }

        public List<Episode> Episodes { get; set; }
    }
}
