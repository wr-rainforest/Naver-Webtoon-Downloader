using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wr_rainforest.WebtoonDownloader
{
    class EpisodeInfo
    {
        /// <summary>
        /// 웹툰 TitleId입니다.
        /// </summary>
        public string TitleId { get; private set; }

        /// <summary>
        /// 회차 제목입니다.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 회차 번호입니다. 1부터 시작합니다.
        /// </summary>
        public int No { get; private set; }

        /// <summary>
        /// 웹툰 등록일(yyyy.MM.dd)입니다.
        /// </summary>
        public string Date { get; private set; }

        /// <summary>
        /// 웹툰 이미지 Url 목록입니다.
        /// </summary>
        public string[] ImageUrls { get; private set; }


        /// <summary>
        /// <seealso cref="EpisodeInfo"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="title">회차 제목</param>
        /// <param name="episodeNo">에피소드 번호</param>
        /// <param name="date">웹툰 등록일</param>
        /// <param name="imageUrls"></param>
        public EpisodeInfo(string titleId, int episodeNo ,string title, string date, string[] imageUrls)
        {
            TitleId = titleId;
            Title = title;
            No = episodeNo;
            Date = date;
            ImageUrls = imageUrls;
        }
        public EpisodeInfo() { }
    }
}
