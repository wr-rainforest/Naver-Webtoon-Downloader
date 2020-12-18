using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NaverWebtoonDownloader.CoreLib.Database;
using System.Xml;
using System.Xml.Serialization;
namespace NaverWebtoonDownloader.CoreLib
{
    public class NameFormat
    {
        /// <summary>
        /// 저장할 이미지의 파일 이름 포맷을 설정합니다. {0~4}는 중복되거나 누락시킬 수 있습니다. {0~4}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : [{5}] {3} - {4} ({2:D3}).jpg</code>
        /// <code>기본값 : [{5}] {3} - {4} ({2:D3}).jpg</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 회차 번호(episodeNo)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(episodeNo = 9, n = 4 => ex:0009)</code>
        /// <code>{2} : 이미지 인덱스(imageIndex)입니다. / ":Dn" : n자리수가 되도록 0을 패딩합니다.(imageIndex = 3, n = 3 => ex:003)</code>
        /// <code>{3} : 웹툰 제목(title)입니다.</code>
        /// <code>{4} : 회차 제목(episodeTitle)입니다.</code>
        /// /// <code>{5} : (episodeDate)입니다.</code>
        /// </summary>
        [JsonPropertyName("ImageFileNameFormat")]
        public string ImageFileNameFormat { get; set; } = "[{5}] {3} - {4} ({2:D3}).jpg";

        public string BuildImageFileName(Image image)
        {
            return ReplaceFileName(string.Format(ImageFileNameFormat,
                image.WebtoonID,
                image.EpisodeNo,
                image.No,
                image.Webtoon.Title,
                image.Episode.Title,
                image.Episode.Date.ToString("yyyy.MM.dd")));
        }

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
        [JsonPropertyName("EpisodeFolderNameFormat")]
        public string EpisodeFolderNameFormat { get; set; } = "[{2}] {4}";

        public string BuildEpisodeFolderName(Episode episode)
        {
            return ReplaceFolderName(string.Format(EpisodeFolderNameFormat,
                episode.WebtoonID,
                episode.No,
                episode.Date.ToString("yyyy.MM.dd"),
                episode.Webtoon.Title,
                episode.Title,
                episode.Webtoon.Writer));
        }

        /// <summary>
        /// 저장할 웹툰의 폴더 이름 포맷을 설정합니다. {0~2}은/는 중복되거나 누락시킬 수 있습니다. {0~2}이외의 다른 숫자는 올 수 없습니다.
        /// <code>포맷 : {0}-{1}</code>
        /// <code>기본값 : {1}</code>
        /// <c>{0} : 웹툰의 titleId 입니다.</c>
        /// <code>{1} : 웹툰 제목(title)입니다.</code>
        /// </summary>
        [JsonPropertyName("WebtoonFolderNameFormat")]
        public string WebtoonFolderNameFormat { get; set; } = "{1}";

        public string BuildWebtoonFolderName(Webtoon webtoon)
        {
            return ReplaceFolderName(string.Format(WebtoonFolderNameFormat,
                webtoon.ID,
                webtoon.Title,
                webtoon.Writer));
        }

        private static string ReplaceFolderName(string name)
        {
            if (name[name.Length - 1] == '.')
                name = name.Substring(0, name.Length - 1) + "．";
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }

        private static string ReplaceFileName(string filename)
        {
            return filename.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
