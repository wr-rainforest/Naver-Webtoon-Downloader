using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wr_rainforest.WebtoonDownloader
{
    /// <summary>
    /// <seealso cref="NaverWebtoonDownloader"/> 설정
    /// </summary>
    class Config
    {
        [JsonIgnore]
        private string defaultDownloadDirectory;
        /// <summary>
        /// 기본 다운로드 위치를 설정합니다. 기본값은 실행 파일 위치의 download 폴더입니다.
        /// </summary>
        public string DefaultDownloadDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(defaultDownloadDirectory))
                    defaultDownloadDirectory = "Download";
                return defaultDownloadDirectory;
            }
            set
            {
                defaultDownloadDirectory = value;
            }
        }

        [JsonIgnore]
        private string imageFileNameFormat;
        /// <summary>
        /// 저장할 이미지의 파일 이름 포맷을 설정합니다. {0~4}는 중복되거나 누락시킬 수 있습니다. {0~4}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1:D4}-{2:D4}-{3}-{4}.jpg</code>
        /// <code>기본값 : [{0}-{1:D4}-{2:D4}] {3} - {4}.jpg</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)</code>
        /// <code>{2} : 이미지 인덱스(imageIndex)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(imageIndex = 3, n = 3 => ex:003)</code>
        /// <code>{3} : 웹툰 제목(title)입니다.</code>
        /// <code>{4} : 회차 제목(episodeTitle)입니다.</code>
        /// /// <code>{5} : (episodeDate)입니다.</code>
        /// </summary>
        public string ImageFileNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(imageFileNameFormat))
                    imageFileNameFormat = "[{5}] {3} - {4} ({2:D3}).jpg";
                return imageFileNameFormat;
            }
            set
            {
                imageFileNameFormat = value;
            }
        }

        [JsonIgnore]
        private string episodeDirectoryNameFormat;
        /// <summary>
        /// 저장할 회차의 폴더 이름 포맷을 설정합니다. {0~5}은/는 중복되거나 누락시킬 수 있습니다. {0~5}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1:D4}-{2}-{3}-{4}</code>
        /// <code>기본값 : [{2}] {4}</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)</code>
        /// <code>{2} : 회차 업로드 날짜(date)입니다</code>
        /// <code>{3} : 웹툰 제목(title)입니다.</code>
        /// <code>{4} : 회차 제목(episodeTitle)입니다.</code>
        /// <code>{5} : 작가 이름(WebtoonWriter)입니다.</code>
        /// </summary>
        public string EpisodeDirectoryNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(episodeDirectoryNameFormat))
                    episodeDirectoryNameFormat = "[{2}] {4}";
                return episodeDirectoryNameFormat;
            }
            set
            {
                episodeDirectoryNameFormat = value;
            }
        }

        [JsonIgnore]
        private string webtoonDirectoryNameFormat;
        /// <summary>
        /// 저장할 웹툰의 폴더 이름 포맷을 설정합니다. {0~2}은/는 중복되거나 누락시킬 수 있습니다. {0~2}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1}</code>
        /// <code>기본값 : {1}</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 웹툰 제목(title)입니다.</code>
        /// <code>{2} : 작가 이름(WebtoonWriter)입니다.</code>
        /// </summary>
        public string WebtoonDirectoryNameFormat
        {
            get
            {
                if (string.IsNullOrWhiteSpace(webtoonDirectoryNameFormat))
                    webtoonDirectoryNameFormat = "{1}";
                return webtoonDirectoryNameFormat;
            }
            set
            {
                webtoonDirectoryNameFormat = value;
            }
        }

        /// <summary>
        /// <seealso cref="Config"/>의 새 인스턴스를 초기화합니다. 하드코딩된 설정을 적용합니다.
        /// </summary>
        public Config()
        {

        }
        /// <summary>
        /// 직렬화된 Json 텍스트로 <seealso cref="Config"/>의 새 인스턴스를 초기화합니다. <paramref name="json"/>이 null일 경우 <seealso cref="ArgumentNullException"/>합니다.
        /// </summary>
        /// <param name="json">Json 텍스트입니다.</param>
        public Config(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json text가 null입니다.");
            }
            Config config = JsonConvert.DeserializeObject<Config>(json);
            DefaultDownloadDirectory = config.DefaultDownloadDirectory;
            ImageFileNameFormat = config.ImageFileNameFormat;
            EpisodeDirectoryNameFormat = config.EpisodeDirectoryNameFormat;
            WebtoonDirectoryNameFormat = config.WebtoonDirectoryNameFormat;
        }
        /// <summary>
        /// 현재 인스턴스를 직렬화된 Json 문자열로 반환합니다.
        /// </summary>
        /// <returns><seealso cref="string"/> Json</returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
