using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.DataType
{
    /// <summary>
    /// 웹툰 메타데이터 클래스입니다. 
    /// </summary>
    class WebtoonInfo
    {
        /// <summary>
        /// 웹툰 제목
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_title")]
        public string Title { get; private set; }

        /// <summary>
        /// 웹툰 id입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_titleid")]
        public string TitleId { get; private set; }

        /// <summary>
        /// 웹툰 장르입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_genre")]
        public string Genre { get; private set; }

        /// <summary>
        /// 웹툰 설명입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_description")]
        public string Description { get; private set; }

        /// <summary>
        /// 웹툰작가 닉네임입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_writer")]
        public string Writer { get; private set; }

        /// <summary>
        /// 웹툰 회차 목록입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_episodes")]
        public List<EpisodeInfo> Episodes { get; private set; }

        /// <summary>
        /// <seealso cref="WebtoonInfo"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="titleId"></param>
        public WebtoonInfo(string title, string titleId)
        {
            Title = title;
            TitleId = titleId;
            Episodes = new List<EpisodeInfo>();
        }
        /// <summary>
        /// <seealso cref="EpisodeInfo"/>
        /// </summary>
        /// <param name="item">추가할 회차입니다.</param>
        public void AddEpisodeInfo(EpisodeInfo item)
        {
            Episodes.Add(item);
        }
    }
}
