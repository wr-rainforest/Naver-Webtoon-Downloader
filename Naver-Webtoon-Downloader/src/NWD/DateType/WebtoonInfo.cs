using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.Key;

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
        public string WebtoonTitle { get; private set; }

        /// <summary>
        /// 웹툰 id입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_titleId")]
        public string WebtoonTitleId { get; private set; }

        /// <summary>
        /// 웹툰 장르입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_genre")]
        public string WebtoonGenre { get; private set; }

        /// <summary>
        /// 웹툰 설명입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_description")]
        public string WebtoonDescription { get; private set; }

        /// <summary>
        /// 웹툰작가 닉네임입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_writer")]
        public string WebtoonWriter { get; private set; }

        /// <summary>
        /// 웹툰 회차 목록입니다.
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_episodes")]
        public Dictionary<int, EpisodeInfo> Episodes { get; private set; }

        /// <summary>
        /// <seealso cref="WebtoonInfo"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="titleId"></param>
        public WebtoonInfo(WebtoonKey webtoonKey, string title)
        {
            WebtoonTitle = title;
            WebtoonTitleId = webtoonKey.TitleId;
            WebtoonEpisodes = new Dictionary<int, EpisodeInfo>();
        }
        /// <summary>
        /// <seealso cref="EpisodeInfo"/>
        /// </summary>
        /// <param name="item">추가할 회차입니다.</param>
        public void AddEpisodeInfo(EpisodeInfo item)
        {
            WebtoonEpisodes.Add(item.EpisodeNo, item);
        }
        /// <summary>
        /// 인스턴스의 가장 마지막 회차 번호를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public int GetLastEpisodeNo()
        {
            return WebtoonEpisodes.Keys.Max();
        }
        public int GetImageCount()
        {
            int imageCount = 0;
            int[] episodeNoList = WebtoonEpisodes.Keys.ToArray();
            for(int i = 0; i < episodeNoList.Length; i++)
            {
                imageCount += WebtoonEpisodes[episodeNoList[i]].EpisodeImageUrls.Length;
            }
            return imageCount;
        }
    }
}
