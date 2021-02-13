using System;
using System.Collections.Generic;
using System.Text;

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
