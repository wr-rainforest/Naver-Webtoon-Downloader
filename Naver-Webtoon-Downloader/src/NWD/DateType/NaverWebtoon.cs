using Newtonsoft.Json;
using WRforest.NWD.Key;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.DataType
{
    class NaverWebtoon
    {
        private Dictionary<string, WebtoonInfo> webtoons;

        /// <summary>
        /// <seealso cref="NaverWebtoon"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public NaverWebtoon()
        {
            webtoons = new Dictionary<string, WebtoonInfo>();
        }

        /// <summary>
        /// 직렬화된 Json 텍스트로 <seealso cref="NaverWebtoon"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="json">Json 텍스트입니다.</param>
        public NaverWebtoon(string json)
        {
            webtoons = JsonConvert.DeserializeObject<Dictionary<string, WebtoonInfo>>(json);
        }

        /// <summary>
        /// 현재 인스턴스의 웹툰 정보들을 Json 문자열로 직렬화하여 반환합니다.
        /// </summary>
        /// <returns><seealso cref="string"/> Json</returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(webtoons);
        }

        /// <summary>
        /// <seealso cref="NaverWebtoon"/>에 지정한 웹툰이 포함되어 있는지 여부를 확인합니다.
        /// </summary>
        /// <param name="webtoonKey">지정한 웹툰</param>
        /// <returns><seealso cref="bool"/></returns>
        public bool Exists(WebtoonKey webtoonKey)
        {
            return webtoons.ContainsKey(webtoonKey.TitleId);
        }

        /// <summary>
        /// <seealso cref="NaverWebtoon"/>의 웹툰 titleId 리스트를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public string[] GetWebtoonTitleIdList()
        {
            return webtoons.Keys.ToArray();
        }

        /// <summary>
        /// <seealso cref="NaverWebtoon"/>에 해당 웹툰을 추가합니다.
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <param name="title"></param>
        public void AddWebtoon(WebtoonKey webtoonKey, string title)
        {
            webtoons.Add(webtoonKey.TitleId, new WebtoonInfo(title, webtoonKey.TitleId));
        }

        /// <summary>
        /// <seealso cref="NaverWebtoon"/>에 해당 회차를 추가합니다. 
        /// 해당 회차의 웹툰 정보가 <seealso cref="NaverWebtoon"/>에 존재하지 않는다면 <seealso cref="Exception"/> 합니다.
        /// 회차 추가는 반드시 순차적으로 이루어져야 합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <param name="title"></param>
        /// <param name="imageUrls"></param>
        /// <param name="date"></param>
        public void AddEpisode(EpisodeKey episodeKey, string title, string[] imageUrls, string date)
        {
            webtoons[episodeKey.TitleId].Episodes.Add(new EpisodeInfo(title, episodeKey.EpisodeNo, imageUrls, date));
        }

        /// <summary>
        /// <seealso cref="WebtoonKey"/>가 지정하는 웹툰의 총 이미지 개수를 반환합니다.
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <returns></returns>
        public int GetWebtoonImageCount(WebtoonKey webtoonKey)
        {
            int webtoonImageCount = 0;
            int webtoonEpisodeCount = webtoons[webtoonKey.TitleId].Episodes.Count;
            for (int i = 0; i < webtoonEpisodeCount; i++)
            {
                int episodeImageCount = webtoons[webtoonKey.TitleId].Episodes[i].ImageUrls.Length;
                for (int j = 0; j < episodeImageCount; j++)
                {
                    webtoonImageCount++;
                }
            }
            return webtoonImageCount;
        }
        /// <summary>
        /// <seealso cref="WebtoonKey"/>가 지정하는 웹툰의 에피소드 개수를  반환합니다
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <returns></returns>
        public int GetEpisodeCount(WebtoonKey webtoonKey)
        {
            return webtoons[webtoonKey.TitleId].Episodes.Count;
        }
        /// <summary>
        /// <seealso cref="WebtoonKey"/>가 지정하는 웹툰의 제목을 불러와 반환합니다.
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <returns></returns>
        public string GetWebtoonTitle(WebtoonKey webtoonKey)
        {
            return webtoons[webtoonKey.TitleId].Title;
        }

        /// <summary>
        /// <seealso cref="EpisodeKey"/>가 지정하는 회차의 제목을 불러와 반환합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public string GetEpisodeTitle(EpisodeKey episodeKey)
        {
            //.episodeKey.episodeNo는 1부터 시작하고, Episodes의 인덱스는 0으로 시작하므로 [episodeKey.EpisodeNo - 1] 합니다
            return webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo - 1].Title;
        }

        /// <summary>
        /// <seealso cref="EpisodeKey"/>가 지정하는 회차의 날짜를 불러와 반환합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public string GetEpisodeDate(EpisodeKey episodeKey)
        {
            //.episodeKey.episodeNo는 1부터 시작하고, Episodes의 인덱스는 0으로 시작하므로 [episodeKey.EpisodeNo - 1] 합니다
            return webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo - 1].Date;
        }

        /// <summary>
        /// <seealso cref="EpisodeKey"/>가 지정하는 회차의 이미지 주소 리스트를 불러와 반환합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public string[] GetEpisodeImageUrls(EpisodeKey episodeKey)
        {
            //.episodeKey.episodeNo는 1부터 시작하고, Episodes의 인덱스는 0으로 시작하므로 [episodeKey.EpisodeNo - 1] 합니다
            return webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo - 1].ImageUrls;
        }
    }
}
