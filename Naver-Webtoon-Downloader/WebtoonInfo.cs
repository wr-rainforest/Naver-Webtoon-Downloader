namespace wr_rainforest.Naver_Webtoon_Downloader
{
    public class WebtoonInfo
    {
        public WebtoonInfo()
        {
        }

        public WebtoonInfo(string titleId, string title, string writer, string genre, string description)
        {
            TitleId = titleId;
            Title = title;
            Writer = writer;
            Genre = genre;
            Description = description;
        }

        /// <summary>
        /// 웹툰 제목입니다.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 웹툰 id입니다.
        /// </summary>
        public string TitleId { get; set; }

        /// <summary>
        /// 웹툰작가 닉네임입니다.
        /// </summary>
        public string Writer { get; set; }

        /// <summary>
        /// 웹툰 장르입니다.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// 웹툰 설명입니다.
        /// </summary>
        public string Description { get; set; }

    }
}
