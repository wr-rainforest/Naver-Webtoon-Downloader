using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.Key;

namespace WRforest.NWD.DataType
{
    class EpisodeInfo
    {
        /// <summary>
        /// 회차 제목입니다.
        /// </summary>
        [JsonProperty(PropertyName = "episode_title")]
        public string EpisodeTitle { get; private set; }

        /// <summary>
        /// 회차 번호입니다. 1부터 시작합니다.
        /// </summary>
        [JsonProperty(PropertyName = "episode_no")]
        public int EpisodeNo { get; private set; }

        /// <summary>
        /// 웹툰 등록일(yyyy.MM.dd)입니다.
        /// </summary>
        [JsonProperty(PropertyName = "episode_date")]
        public string EpisodeDate { get; private set; }

        /// <summary>
        /// 웹툰 이미지 Url 목록입니다.
        /// </summary>
        [JsonProperty(PropertyName = "episode_image_urls")]
        public string[] EpisodeImageUrls { get; private set; }


        /// <summary>
        /// <seealso cref="EpisodeInfo"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="title">회차 제목</param>
        /// <param name="episodeNo">에피소드 번호</param>
        /// <param name="imageUrls"></param>
        /// <param name="date">웹툰 업로드 날짜</param>
        public EpisodeInfo(EpisodeKey episodeKey,string title, string[] imageUrls, string date)
        {
            this.EpisodeTitle = title;
            this.EpisodeNo = episodeKey.EpisodeNo;
            this.EpisodeImageUrls = imageUrls;
            this.EpisodeDate = date;
        }
    }
}
