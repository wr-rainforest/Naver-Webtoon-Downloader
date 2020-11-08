using System;

namespace NaverWebtoonDownloader.CoreLib
{
    public class EpisodeNotFoundException : Exception
    {
        public string TitleId { get; set; }
        public int EpisodeNo { get; set; }

        public EpisodeNotFoundException(string titleId, int episodeNo) : base("존재하지 않는 회차입니다.")
        {
            TitleId = titleId;
            EpisodeNo = episodeNo;
        }

        public EpisodeNotFoundException(string titleId, int episodeNo, Exception innerException) : base("존재하지 않는 회차입니다.", innerException)
        {
            TitleId = titleId;
            EpisodeNo = episodeNo;
        }
    }
}
