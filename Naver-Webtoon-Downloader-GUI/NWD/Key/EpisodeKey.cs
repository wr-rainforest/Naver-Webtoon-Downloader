using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Key
{
    /// <summary>
    /// 특정 웹툰의 특정 에피소드에 1:1로 매핑되는 고유한 식별자입니다.
    /// </summary>
    class EpisodeKey : WebtoonKey
    {
        /// <summary>
        /// 에피소드 번호입니다. 1부터 시작합니다.
        /// </summary>
        public int EpisodeNo { get; private set; }

        /// <summary>
        /// <seealso cref="EpisodeKey"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="titleId">웹툰 titleId</param>
        /// <param name="episdoeNo">에피소드 번호</param>
        public EpisodeKey(string titleId, int episdoeNo) : base(titleId)
        {
            EpisodeNo = episdoeNo;
        }

        /// <summary>
        /// https://comic.naver.com/webtoon/detail.nhn?titleId={0}&no={1} 형식의 url을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public override string BuildUrl()
        {
            return string.Format("https://comic.naver.com/webtoon/detail.nhn?titleId={0}&no={1}", TitleId, EpisodeNo);
        }

        /// <summary>
        /// TitleId-EpisodeNo(4자리 패딩)-****
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1:D4}-****", TitleId, EpisodeNo);
        }
    }
}
