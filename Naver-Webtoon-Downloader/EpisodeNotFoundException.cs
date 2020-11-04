using System;

namespace wr_rainforest.Naver_Webtoon_Downloader
{
    public class EpisodeNotFoundException : Exception
    {
        public string TitleId { get; set; }
        public int EpisodeNo { get; set; }

        public EpisodeNotFoundException()
        {

        }

        public EpisodeNotFoundException(string message) : base(message)
        {
        }

        public EpisodeNotFoundException(string titleId, int episodeNo)
        {
            TitleId = titleId;
            EpisodeNo = episodeNo;
        }

        public EpisodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
