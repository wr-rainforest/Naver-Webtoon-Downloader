using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.CoreLib
{
    class WebtoonNotFoundException : Exception
    {
        public string TitleId { get; set; }

        public WebtoonNotFoundException(string titleId) : base("존재하지 않는 웹툰입니다.")
        {
            TitleId = titleId;
        }

        public WebtoonNotFoundException(string titleId, Exception innerException) : base("존재하지 않는 웹툰입니다.", innerException)
        {
            TitleId = titleId;
        }
    }
}
