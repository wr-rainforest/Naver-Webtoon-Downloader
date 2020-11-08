using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.CoreLib
{
    public class ImageInfo
    {
        public string TitleId { get; set; }
        public int No { get; set; }
        public int Index { get; set; }
        public string Uri { get; set; }
        public int Size { get; set; }
        public int Downloaded { get; set; }
    }
}
