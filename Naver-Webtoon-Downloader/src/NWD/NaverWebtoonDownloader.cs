using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;
namespace WRforest.NWD
{
    class NaverWebtoonDownloader
    {
        private Parser.Agent agent;
        private Parser.Parser parser;
        private Parser.XPath xPath;
        private Dictionary<string, WebtoonInfo> webtoons;
        private Config config;
        /// <summary>
        /// <seealso cref="NaverWebtoonDownloader"/>의 새 인스턴스를 초기화합니다. 
        /// </summary>
        public NaverWebtoonDownloader()
        {
            if (IO.Exists("data\\configs", "config.json"))
            {
                config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
            }
            else
            {
                config = new Config();
                IO.WriteTextFile("data\\configs", "config.json", config.ToJsonString());
            }
            if (IO.Exists("data\\configs", "xpath_config.json"))
            {
                xPath = new Parser.XPath(IO.ReadTextFile("data\\configs", "xpath_config.json"));
            }
            else
            {
                xPath = new Parser.XPath();
                IO.WriteTextFile("data\\configs", "xpath_config.json", xPath.ToJsonString());
            }
            if (IO.Exists("data", "webtoon_metadata.json"))
            {
                naverWebtoon = new DataType.NaverWebtoon(IO.ReadTextFile("data", "webtoon_metadata.json"));
            }
            else
            {
                naverWebtoon = new DataType.NaverWebtoon();
            }
            agent = new Parser.Agent();
            parser = new Parser.Parser(agent, xPath);
        }
#if Console
        public void Download(string titleId)
        {
            WebtoonKey webtoonKey = new WebtoonKey(titleId);
            if (!naverWebtoon.Contains(webtoonKey))
            {
                naverWebtoon.AddWebtoon(webtoonKey,)
            }
            UpdateWebtoonInfo(webtoonKey);
            int webtoonImageCount = naverWebtoon.GetWebtoonImageCount(webtoonKey);
            int episodeCount = naverWebtoon.GetEpisodeCount(webtoonKey);
            int downloadedWebtoonImageCount = 0;
            int currentImageCount = 0;

            for (int no = 1; no <= episodeCount; no++)
            {
                EpisodeKey episodeKey = new EpisodeKey(titleId, no);
                agent.AddHeader("Referer", episodeKey.BuildUrl());
                string[] imageUrls = naverWebtoon.GetEpisodeImageUrls(episodeKey);
                for (int imageIndex = 0; imageIndex < imageUrls.Length; imageIndex++)
                {
                    currentImageCount++;
                    ImageKey imageKey = new ImageKey(titleId, no, imageIndex);
                    if (ExistsImageFile(imageKey))
                    {
                        continue;
                    }
                    downloadedWebtoonImageCount++;
                    agent.SetHeader("Referer", episodeKey.BuildUrl());
                    byte[] buff = agent.DownloadData(imageUrls[imageIndex]);
                    SaveImageFile(imageKey, buff);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webtoonKey"></param>
        public void UpdateWebtoonInfo(WebtoonKey webtoonKey)
        {
            //comic.naver.com에서 최신 회차의 EpisodeNo를 불러옵니다.
            agent.LoadPage(webtoonKey.BuildUrl());
            int latestEpisodeNo = int.Parse(parser.GetLatestEpisodeNo());
            //로컬에 저장된 메타데이터중 가장 마지막 회차의 EpisodeNo를 불러옵니다.
            int lastEpisodeNo = naverWebtoon.GetEpisodeCount(webtoonKey);
            //마지막 회차가 최신 회차면 업데이트하지 않습니다.
            if (latestEpisodeNo == lastEpisodeNo)
            {
                return;
            }
            //로컬에 저장된 웹툰 정보를 업데이트합니다.
            for (int episodeNo = lastEpisodeNo + 1; episodeNo <= latestEpisodeNo; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                agent.LoadPage(episodeKey.BuildUrl());
                string episodeTitle = parser.GetEpisodeTitle();
                string episodeDate = parser.GetEpisodeDate();
                string[] imageUrls = parser.GetComicContentImageUrls();
                naverWebtoon.AddEpisode(episodeKey, episodeTitle, imageUrls, episodeDate);
            }
            IO.WriteTextFile("data", "webtoon_metadata.json", naverWebtoon.ToJsonString());
        }
#endif

#if WPF
#endif
        private int GetDownloadedImageCount(WebtoonKey webtoonKey)
        {
            int count = 0;
            int episodeCount = naverWebtoon.GetEpisodeCount(webtoonKey);
            for (int episodeNo = 1; episodeNo <= episodeCount; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                int imageCount = naverWebtoon.GetEpisodeImageUrls(episodeKey).Length;
                for (int imageIndex = 0; imageIndex < imageCount; imageIndex++)
                {
                    ImageKey imageKey = new ImageKey(episodeKey.TitleId, episodeKey.EpisodeNo, imageIndex);
                    if (ExistsImageFile(imageKey))
                    {
                        count++;
                    }
                }
            }
            return count;
        }


        /// <summary>
        /// <paramref name="imageKey"/>가 지정하는 이미지 파일 경로에 <paramref name="data"/>를 저장합니다
        /// </summary>
        /// <param name="imageKey"></param>
        /// <param name="data"></param>
        private void SaveImageFile(ImageKey imageKey, byte[] data)
        {
            string directory =
                config.DefaultDownloadDirectory + "\\" +
                BuildWebtoonDirectoryName(imageKey) + "\\" +
                BuildEpisodeDirectoryName(imageKey);
            string filename =
                BuildImageFileName(imageKey);
            IO.WriteAllBytes(directory, filename, data);
        }

        /// <summary>
        /// <paramref name="imageKey"/>가 지정하는 이미지 파일이 존재하는지 확인합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        private bool ExistsImageFile(ImageKey imageKey)
        {
            return IO.Exists(BuildImageFileFullDirectory(imageKey));
        }

        /// <summary>
        /// imageKey가 지정하는 이미지의 전체 경로 문자열을 반환합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        private string BuildImageFileFullDirectory(ImageKey imageKey)
        {
            string directory =
                config.DefaultDownloadDirectory + "\\" +
                BuildWebtoonDirectoryName(imageKey) + "\\" +
                BuildEpisodeDirectoryName(imageKey) + "\\" +
                BuildImageFileName(imageKey);
            return directory;
        }

        /// <summary>
        /// imageKey가 지정하는 이미지의 파일명을 정의된 포맷 형식(<seealso cref="Config.ImageFileNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        private string BuildImageFileName(ImageKey imageKey)
        {
            string titleId = imageKey.TitleId;
            int episodeNo = imageKey.EpisodeNo;
            int ImageIndex = imageKey.ImageIndex;
            string webtoonTitle = naverWebtoon.GetWebtoonTitle(imageKey);
            string episoneTitle = naverWebtoon.GetEpisodeTitle(imageKey);
            return string.Format(config.ImageFileNameFormat,
                titleId,
                episodeNo,
                ImageIndex,
                ReplaceFileName(webtoonTitle),
                ReplaceFileName(episoneTitle));
        }

        /// <summary>
        /// episodeKey가 지정하는 회차의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.EpisodeDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        private string BuildEpisodeDirectoryName(EpisodeKey episodeKey)
        {
            string titleId = episodeKey.TitleId;
            int episodeNo = episodeKey.EpisodeNo;
            string date = naverWebtoon.GetEpisodeDate(episodeKey);
            string webtoonTitle = naverWebtoon.GetWebtoonTitle(episodeKey);
            string episodeTitle = naverWebtoon.GetEpisodeTitle(episodeKey);
            return string.Format(config.EpisodeDirectoryNameFormat,
                titleId,
                episodeNo,
                date,
                ReplaceFileName(webtoonTitle),
                ReplaceFileName(episodeTitle));
        }

        /// <summary>
        /// webtoonKey가 지정하는 웹툰의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.WebtoonDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        private string BuildWebtoonDirectoryName(WebtoonKey episodeKey)
        {
            string titleId = episodeKey.TitleId;
            string webtoonTitle = naverWebtoon.GetWebtoonTitle(episodeKey);
            return string.Format(config.WebtoonDirectoryNameFormat,
                titleId,
                ReplaceFileName(webtoonTitle));
        }
        private Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
        private string ReplaceFileName(string filename) => regex.Replace(filename, "&");
    }
}
