using System;
using System.Runtime.Serialization;

namespace NaverWebtoonDownloader.CoreLib
{
    [Serializable]
    public class EpisodeNotFoundException : Exception
    {
        public EpisodeNotFoundException() : base("")
        {
        }
    }
}