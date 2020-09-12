using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Key
{
    /// <summary>
    /// 특정 웹툰과 1:1로 매핑되는 고유한 식별자입니다.
    /// </summary>
    class WebtoonKey
    {
        /// <summary>
        /// 웹툰의 titleId입니다. 
        /// </summary>
        public string TitleId { get; private set; }

        /// <summary>
        /// <seealso cref="WebtoonKey"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="titleId"></param>
        public WebtoonKey(string titleId)
        {
            TitleId = titleId;
        }

        /// <summary>
        /// https://comic.naver.com/webtoon/list.nhn?titleId={0} 형식의 url을 생성합니다.
        /// </summary>
        /// <returns><seealso cref="string"/></returns>
        public virtual string BuildUrl()
        {
            return string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", TitleId);
        }

        /// <summary>
        /// TitleId-****-****
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-****-****", TitleId);
        }
    }
}
