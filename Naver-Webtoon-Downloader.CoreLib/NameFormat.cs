using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NaverWebtoonDownloader.CoreLib
{
    /// <summary>
    /// <seealso cref="NaverWebtoonDownloader"/> 설정<br/>
    /// </summary>
    public class NameFormat
    {
        private string imageFileNameFormat;
        /// <summary>
        /// 저장할 이미지의 파일 이름 포맷을 설정합니다. {0~4}는 중복되거나 누락시킬 수 있습니다. {0~4}이외의 다른 숫자는 올 수 없습니다.<br/>
        /// 포맷 : {0}-{1:D4}-{2:D4}-{3}-{4}<br/>
        /// 기본값 : [{5}] {3} - {4} ({2:D3})<br/>
        /// {0} : 웹툰의 titleId 입니다.<br/>
        /// {1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)<br/>
        /// {2} : 이미지 인덱스(imageIndex)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(imageIndex = 3, n = 3 => ex:003)<br/>
        /// {3} : 웹툰 제목(title)입니다.<br/>
        /// {4} : 회차 제목(episodeTitle)입니다.<br/>
        /// {5} : (episodeDate)입니다.<br/>
        /// </summary>
        public string ImageFileNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(imageFileNameFormat))
                    imageFileNameFormat = "[{5}] {3} - {4} ({2:D3})";
                return imageFileNameFormat;
            }
            set
            {
                imageFileNameFormat = value;
            }
        }

        private string episodeFolderNameFormat;
        /// <summary>
        /// 저장할 회차의 폴더 이름 포맷을 설정합니다. {0~5}는 중복되거나 누락시킬 수 있습니다. {0~5}이외의 다른 숫자는 올 수 없습니다.<br/>
        /// 포맷 : {0}-{1:D4}-{2}-{3}-{4}<br/>
        /// 기본값 : [{2}] {4}<br/>
        /// {0} : 웹툰의 titleId 입니다.<br/>
        /// {1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 = ex:0009)<br/>
        /// {2} : 회차 업로드 날짜(date)입니다<br/>
        /// {3} : 웹툰 제목(title)입니다.<br/>
        /// {4} : 회차 제목(episodeTitle)입니다.<br/>
        /// {5} : 작가 이름(WebtoonWriter)입니다.<br/>
        /// </summary>
        public string EpisodeFolderNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(episodeFolderNameFormat))
                    episodeFolderNameFormat = "[{2}] {4}";
                return episodeFolderNameFormat;
            }
            set
            {
                episodeFolderNameFormat = value;
            }
        }

        private string webtoonFolderNameFormat;
        /// <summary>
        /// 저장할 웹툰의 폴더 이름 포맷을 설정합니다. {0~2}은/는 중복되거나 누락시킬 수 있습니다. {0~2}이외의 다른 숫자는 올 수 없습니다.<br/>
        /// 포맷 : {0}-{1}<br/>
        /// 기본값 : {1}<br/>
        /// {0} : 웹툰의 titleId 입니다.<br/>
        /// {1} : 웹툰 제목(title)입니다.<br/>
        /// {2} : 작가 이름(WebtoonWriter)입니다.<br/>
        /// </summary>
        public string WebtoonFolderNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(webtoonFolderNameFormat))
                    webtoonFolderNameFormat = "{1}";
                return webtoonFolderNameFormat;
            }
            set
            {
                webtoonFolderNameFormat = value;
            }
        }

        /// <summary>
        /// <seealso cref="NameFormat"/>의 새 인스턴스를 초기화합니다. 하드코딩된 설정을 적용합니다.
        /// </summary>
        public NameFormat()
        {

        }
    }
}
