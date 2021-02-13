using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib
{
    [Serializable]
    public class WebtoonNotFoundException : Exception
    {
        public WebtoonNotFoundException() : base("")
        {
        }
    }
}
