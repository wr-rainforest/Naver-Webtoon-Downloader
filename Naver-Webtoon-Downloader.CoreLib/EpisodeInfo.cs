using System.Collections.Generic;

namespace NaverWebtoonDownloader.CoreLib
{
    public class EpisodeInfo
    {
        public EpisodeInfo()
        {
        }

        public EpisodeInfo(string titleId, int episodeNo, string title, string date, List<ImageInfo> images)
        {
            TitleId = titleId;
            No = episodeNo;
            Title = title;
            Date = date;
            Images = images;
        }

        /// <summary>
        /// 웹툰 TitleId입니다.
        /// </summary>
        public string TitleId { get; set; }

        /// <summary>
        /// 회차 번호입니다. 1부터 시작합니다.
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 회차 제목입니다.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 웹툰 등록일(yyyy.MM.dd)입니다.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 웹툰 이미지 Uri 목록입니다.
        /// </summary>
        public List<ImageInfo> Images { get; set; }

    }
}
