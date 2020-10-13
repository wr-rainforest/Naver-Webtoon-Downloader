using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Key
{
    /// <summary>
    /// 특정 웹툰, 특정 에피소드, 특정 이미지에 1:1로 매핑되는 고유한 식별자입니다.
    /// </summary>
    class ImageKey : EpisodeKey
    {
        /// <summary>
        /// 이미지 인덱스입니다. 0부터 시작합니다.
        /// </summary>
        public int ImageIndex { get; private set; }

        /// <summary>
        /// ImageKey의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <param name="imageIndex"></param>
        public ImageKey(string titleId, int episodeNo, int imageIndex) : base(titleId, episodeNo)
        {
            ImageIndex = imageIndex;
        }

        /// <summary>
        /// <seealso cref="ImageKey"/>는 Url을 지정하지 않습니다. <seealso cref="NotImplementedException"/>을 throw합니다.
        /// </summary>
        /// <returns></returns>
        public override string BuildUrl()
        {
            throw new NotImplementedException("");
        }

        /// <summary>
        /// TitleId-EpisodeNo(4자리 패딩)-ImageIndex(4자리 패딩)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1:D4}-{2:D4}", TitleId, EpisodeNo, ImageIndex);
        }
    }
}
