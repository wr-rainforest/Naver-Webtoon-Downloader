using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wr_rainforest.NWD.DataType;
using wr_rainforest.NWD.Key;

namespace wr_rainforest.NWD
{
    class FileNameBuilder
    {
        WebtoonInfo webtoonInfo;
        Config config;
        public FileNameBuilder(WebtoonInfo webtoonInfo, Config config)
        {
            this.webtoonInfo = webtoonInfo;
            this.config = config;
        }
        /// <summary>
        /// imageKey가 지정하는 이미지의 전체 파일 경로문자열을 반환합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        public string BuildImageFileFullPath(ImageKey imageKey)
        {
            string path =
                config.DefaultDownloadDirectory + "\\" +
                BuildWebtoonDirectoryName(imageKey) + "\\" +
                BuildEpisodeDirectoryName(imageKey) + "\\" +
                BuildImageFileName(imageKey);
            return path;
        }

        /// <summary>
        /// imageKey가 지정하는 이미지의 전체 폴더 경로 문자열을 반환합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        public string BuildImageFileFullDirectory(ImageKey imageKey)
        {
            string directory =
                config.DefaultDownloadDirectory + "\\" +
                BuildWebtoonDirectoryName(imageKey) + "\\" +
                BuildEpisodeDirectoryName(imageKey);
            return directory;
        }

        /// <summary>
        /// imageKey가 지정하는 이미지의 파일명을 정의된 포맷 형식(<seealso cref="Config.ImageFileNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        public string BuildImageFileName(ImageKey imageKey)
        {
            string titleId = imageKey.TitleId;
            int episodeNo = imageKey.EpisodeNo;
            int ImageIndex = imageKey.ImageIndex;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            string episoneTitle = webtoonInfo.Episodes[imageKey.EpisodeNo].EpisodeTitle;
            return string.Format(config.ImageFileNameFormat,
                titleId,
                episodeNo,
                ImageIndex,
                ReplaceFileName(webtoonTitle),
                ReplaceFileName(episoneTitle),
                webtoonInfo.Episodes[imageKey.EpisodeNo].EpisodeDate);
        }

        /// <summary>
        /// episodeKey가 지정하는 회차의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.EpisodeDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public string BuildEpisodeDirectoryName(EpisodeKey episodeKey)
        {
            string titleId = episodeKey.TitleId;
            int episodeNo = episodeKey.EpisodeNo;
            string date = webtoonInfo.Episodes[episodeKey.EpisodeNo].EpisodeDate;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            string episodeTitle = webtoonInfo.Episodes[episodeKey.EpisodeNo].EpisodeTitle;
            string webtoonWriter = webtoonInfo.WebtoonWriter;
            return string.Format(config.EpisodeDirectoryNameFormat,
                titleId,
                episodeNo,
                date,
                ReplaceFolderName(webtoonTitle),
                ReplaceFolderName(episodeTitle),
                ReplaceFolderName(webtoonWriter));
        }

        /// <summary>
        /// webtoonKey가 지정하는 웹툰의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.WebtoonDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public string BuildWebtoonDirectoryName(WebtoonKey webtoonKey)
        {
            string titleId = webtoonInfo.WebtoonTitleId;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            string webtoonWriter = webtoonInfo.WebtoonWriter;
            return string.Format(config.WebtoonDirectoryNameFormat,
                titleId,
                ReplaceFolderName(webtoonTitle),
                ReplaceFolderName(webtoonWriter));
        }

        /// <summary>
        /// 윈도우에서 파일명으로 사용이 불가능한 반각 특수문자들을 전각 문자로 교체합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string ReplaceFileName(string filename)
        {
            return filename.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }

        /// <summary>
        /// 윈도우에서 폴더명으로 사용이 불가능한 반각 특수문자들을 전각 문자로 교체합니다.
        /// <code>문자열의 마지막에 반각 온점이 있다면 해당 온점은 전각으로 교체합니다. 나머지 온점은 반각 그대로입니다.</code>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string ReplaceFolderName(string name)
        {
            if (name[name.Length - 1] == '.')
            {
                name = name.Substring(0, name.Length - 1) + "．";
            }
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
