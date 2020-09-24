using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Parser
{
    [JsonObject(MemberSerialization.OptIn)]
    class XPath
    {
        [JsonIgnore]
        private string latestEpisode;
        /// <summary>
        /// 타이틀(웹툰 목록=list.nhn)에서 웹툰 가장 최신회차의 href="url" 가 있는 노드의 xpath 입니다
        /// </summary>
        [JsonProperty(PropertyName = "episode_latest_href")]
        public string LatestEpisode 
        {
            get
            {
                if (string.IsNullOrWhiteSpace(latestEpisode))
                    latestEpisode = "//td[@class=\"title\"]/a";
                return latestEpisode;
            }
            set
            {
                latestEpisode = value;
            }
        }

        [JsonIgnore]
        private string comicContent;
        /// <summary>
        /// 웹툰 이미지가 담긴 img태그의 xpath. 이미지 url은 src태그에 존재
        /// </summary>
        [JsonProperty(PropertyName = "episode_comic_content_src")]
        public string ComicContent 
        {
            get
            {
                if (string.IsNullOrWhiteSpace(comicContent))
                    comicContent = "//*[@alt=\"comic content\"]";
                return comicContent;
            }
            set
            {
                comicContent = value;
            }
        }

        [JsonIgnore]
        private string webtoonTitle;
        /// <summary>
        /// 웹툰 제목이 담긴 img태그 xpath, content 속성
        /// </summary>
        [JsonProperty(PropertyName = "webtoon_title_content")]
        public string WebtoonTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(webtoonTitle))
                    webtoonTitle = "//*[@property=\"og:title\"]";
                return webtoonTitle;
            }
            set
            {
                webtoonTitle = value;
            }
        }

        [JsonIgnore]
        private string episodeTitle;
        /// <summary>
        /// 해당 XPath 노드의 content 속성=EpisodeTitle
        /// </summary>
        [JsonProperty(PropertyName = "episode_title_content")]
        public string EpisodeTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(episodeTitle))
                    episodeTitle = "//*[@property=\"og:description\"]";
                return episodeTitle;
            }
            set
            {
                episodeTitle = value;
            }
        }

        [JsonIgnore]
        private string date;
        /// <summary>
        /// 해당 XPath노드의 Value=Date
        /// </summary>
        [JsonProperty(PropertyName = "episode_date_Value")]
        public string Date
        {
            get
            {
                if (string.IsNullOrWhiteSpace(date))
                    date = "//*[@class=\"date\"]";
                return date;
            }
            set
            {
                date = value;
            }
        }

        private string webtoonList;
        [JsonProperty(PropertyName = "mainpage_webtoon_list")]
        public string WebtoonList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(webtoonList))
                    webtoonList = "//a[@class=\"title\"]";
                return webtoonList;
            }
            set
            {
                webtoonList = value;
            }
        }
        /// <summary>
        ///<seealso cref="XPath"/>를 초기화합니다. 하드코딩된 설정이 적용됩니다.
        /// </summary>
        public XPath()
        {

        }

        /// <summary>
        /// 직렬화된 Json 텍스트로 <seealso cref="XPath"/>의 새 인스턴스를 초기화합니다. <paramref name="json"/>이 null일 경우 <seealso cref="ArgumentNullException"/>합니다.
        /// </summary>
        /// <param name="json">Json 텍스트입니다.</param>
        public XPath(string json)
        {
            if(string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json text가 null입니다.");
            }
            XPath xPath = JsonConvert.DeserializeObject<XPath>(json);
            LatestEpisode = xPath.LatestEpisode;
            ComicContent = xPath.ComicContent;
            WebtoonTitle = xPath.WebtoonTitle;
            EpisodeTitle = xPath.EpisodeTitle;
            Date = xPath.Date;
        }

        /// <summary>
        /// 현재 인스턴스의 XPath <seealso cref="string"/>을 Json 문자열로 직렬화하여 반환합니다.
        /// </summary>
        /// <returns><seealso cref="string"/> Json</returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
