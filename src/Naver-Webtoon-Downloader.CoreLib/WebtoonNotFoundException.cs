using System;
using System.Runtime.Serialization;

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