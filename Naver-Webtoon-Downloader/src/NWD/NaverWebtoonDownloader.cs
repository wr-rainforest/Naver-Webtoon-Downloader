using Newtonsoft.Json;
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
            IO.Log("<NaverWebtoonDownloader.ctor()>");
            if (IO.Exists("data\\configs", "config.json"))
            {
                IO.Log("NaverWebtoonDownloader.ctor config.json Exist");
                config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
                IO.Log("NaverWebtoonDownloader.ctor load config.json fin");
            }
            else
            {
                IO.Log("NaverWebtoonDownloader.ctor config.json !Exist");
                config = new Config();
                IO.WriteTextFile("data\\configs", "config.json", config.ToJsonString());
                IO.Log("NaverWebtoonDownloader.ctor gen config.json fin");

            }
            if (IO.Exists("data\\configs", "xpath_config.json"))
            {
                IO.Log("NaverWebtoonDownloader.ctor xpath_config.json Exist");
                xPath = new Parser.XPath(IO.ReadTextFile("data\\configs", "xpath_config.json"));
                IO.Log("NaverWebtoonDownloader.ctor load xpath_config.json fin");
            }
            else
            {
                IO.Log("NaverWebtoonDownloader.ctor xpath_config.json !Exist");
                xPath = new Parser.XPath();
                IO.WriteTextFile("data\\configs", "xpath_config.json", xPath.ToJsonString());
                IO.Log("NaverWebtoonDownloader.ctor gen xpath_config.json fin");
            }
            webtoons = new Dictionary<string, WebtoonInfo>();
            agent = new Parser.Agent();
            parser = new Parser.Parser(agent, xPath);
            IO.Log("</NaverWebtoonDownloader.ctor()>");
        }
#if Console
        public void Download(string titleId)
        {
            //IO.Log(string.Format("<NaverWebtoonDownloader.Download({0})>",titleId));
            agent.LoadPage(string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", titleId));
            var webtoonTitle = parser.GetWebtoonTitle();
            IO.Print(string.Format("{0}({1}) 다운로드를 시작합니다.",webtoonTitle, titleId));
            WebtoonKey webtoonKey = new WebtoonKey(titleId);
            if(IO.Exists("data\\webtoons", webtoonKey.TitleId + ".json"))
            {
                var webtoonInfo = LoadWebtoonInfo(webtoonKey);
                IO.Print(string.Format("{0}({1}) 캐시파일을 불러왔습니다.", webtoonTitle, titleId));
                //IO.Log(string.Format("NaverWebtoonDownloader.Download load cache fin"));
                UpdateWebtoonInfo(webtoonInfo);
                webtoons.Add(titleId, webtoonInfo);
                SaveWebtoonInfo(webtoonInfo);

            }
            else
            {
                var webtoonInfo = BuildWebtoonInfo(webtoonKey);
                webtoons.Add(titleId, webtoonInfo);
                SaveWebtoonInfo(webtoonInfo);
            }
            int webtoonImageCount = webtoons[webtoonKey.TitleId].GetImageCount();
            int downloadedWebtoonImageCount = GetDownloadedImageCount(webtoonKey);
            int episodeCount = webtoons[webtoonKey.TitleId].Episodes.Count;
            int currentImageCount = 0;

            IO.Print(string.Format("{0}({1}) 이미지 다운로드를 시작합니다.", webtoonTitle, titleId));
            for (int episodeNo = 1; episodeNo <= episodeCount; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(titleId, episodeNo);
                agent.AddHeader("Referer", episodeKey.BuildUrl());
                string[] imageUrls = webtoons[webtoonKey.TitleId].Episodes[episodeNo].EpisodeImageUrls;
                for (int imageIndex = 0; imageIndex < imageUrls.Length; imageIndex++)
                {
                    
                    ImageKey imageKey = new ImageKey(titleId, episodeNo, imageIndex);
                    if (ExistsImageFile(imageKey))
                    {
                        continue;
                    }
                    agent.SetHeader("Referer", episodeKey.BuildUrl());
                    byte[] buff = agent.DownloadData(imageUrls[imageIndex]);
                    SaveImageFile(imageKey, buff);
                    currentImageCount++;
                    Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
                    IO.Print(string.Format("{0}({7}) [{1}/{2}] ({3:P}) [{4}] {5} - {6}",
                        webtoons[titleId].WebtoonTitle,
                        (currentImageCount+downloadedWebtoonImageCount).ToString("D"+webtoonImageCount.ToString().Length.ToString()),
                        webtoonImageCount,
                        (decimal)(currentImageCount + downloadedWebtoonImageCount)/ webtoonImageCount,
                        webtoons[titleId].Episodes[episodeNo].EpisodeDate,
                        webtoons[titleId].Episodes[episodeNo].EpisodeTitle,
                        imageIndex,
                        webtoons[titleId].WebtoonTitleId),false);

                }
            }
            Console.WriteLine();
            IO.Print(string.Format("{0}({1}) 이미지 다운로드를 완료하였습니다.", webtoonTitle, titleId));
            IO.Print(string.Format("{0}({1}) 다운로드 완료.", webtoonTitle, titleId));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webtoonKey"></param>
        private void UpdateWebtoonInfo(WebtoonInfo webtoonInfo)
        {
            IO.Print(string.Format("{0}({1}) 업데이트를 확인합니다..", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
            WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
            //comic.naver.com에서 최신 회차의 EpisodeNo를 불러옵니다.
            agent.LoadPage(webtoonKey.BuildUrl());
            int latestEpisodeNo = int.Parse(parser.GetLatestEpisodeNo());
            //webtoonInfo중 가장 마지막 회차의 EpisodeNo를 불러옵니다.
            int lastEpisodeNo = webtoonInfo.GetLastEpisodeNo();
            //마지막 회차가 최신 회차면 업데이트하지 않습니다.
            if (latestEpisodeNo == lastEpisodeNo)
            {
                IO.Print(string.Format("{0}({1}) 업데이트된 회차가 없습니다.", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
                IO.Print(string.Format("{0}({1}) 최신 회차 : [{2}] {3}.", 
                    webtoonInfo.WebtoonTitle, 
                    webtoonInfo.WebtoonTitleId,
                    webtoonInfo.Episodes[latestEpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[latestEpisodeNo].EpisodeTitle
                    ));
                return;
            }
            //웹툰 정보를 업데이트합니다.
            for (int episodeNo = lastEpisodeNo + 1; episodeNo <= latestEpisodeNo; episodeNo++)
            {
                IO.Print(string.Format("{0}({1}) 캐시를 업데이트합니다.", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                agent.LoadPage(episodeKey.BuildUrl());
                string episodeTitle = parser.GetEpisodeTitle();
                string episodeDate = parser.GetEpisodeDate();
                string[] imageUrls = parser.GetComicContentImageUrls();
                webtoonInfo.Episodes.Add(episodeNo, new EpisodeInfo(episodeKey, episodeTitle, imageUrls, episodeDate));
                Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
                IO.Print(string.Format("{0}({1}) [{2}/{3}] ({4:P}) [{5}]",
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle),false);
            }
            Console.WriteLine();
            IO.Print(string.Format("{0}({1}) 캐시를 업데이트하였습니다.", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
        }
        private WebtoonInfo BuildWebtoonInfo(WebtoonKey webtoonKey)
        {
            agent.LoadPage(webtoonKey.BuildUrl());
            int latestEpisodeNo = int.Parse(parser.GetLatestEpisodeNo());
            string webtoonTitle = parser.GetWebtoonTitle();
            WebtoonInfo webtoonInfo = new WebtoonInfo(webtoonKey, webtoonTitle);
            IO.Print(string.Format("{0}({1}) 캐시를 생성합니다.", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
            for (int episodeNo = 1; episodeNo <= latestEpisodeNo; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                agent.LoadPage(episodeKey.BuildUrl());
                string episodeTitle = parser.GetEpisodeTitle();
                string episodeDate = parser.GetEpisodeDate();
                string[] imageUrls = parser.GetComicContentImageUrls();
                webtoonInfo.Episodes.Add(episodeNo, new EpisodeInfo(episodeKey, episodeTitle, imageUrls, episodeDate));
                Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
                IO.Print(string.Format("{0}({1}) [{2}/{3}] ({4:P}) [{5}]",
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle), false);
            }
            Console.WriteLine();
            IO.Print(string.Format("{0}({1}) 캐시를 생성하였습니다.", webtoonInfo.WebtoonTitle, webtoonInfo.WebtoonTitleId));
            return webtoonInfo;
        }

#endif

#if WPF
#endif

        /// <summary>
        /// <paramref name="webtoonInfo"/>를 로컬에 저장합니다.
        /// </summary>
        /// <param name="webtoonInfo"></param>
        private void SaveWebtoonInfo(WebtoonInfo webtoonInfo)
        {
            string json = JsonConvert.SerializeObject(webtoonInfo);
            IO.WriteTextFile("data\\webtoons", webtoonInfo.WebtoonTitleId + ".json", json);
        }
        /// <summary>
        /// 로컬에서<paramref name="webtoonKey"/>가 지정한 웹툰의 <seealso cref="WebtoonInfo"/>를 불러옵니다.
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <returns></returns>
        private WebtoonInfo LoadWebtoonInfo(WebtoonKey webtoonKey)
        {
            string json = IO.ReadTextFile("data\\webtoons", webtoonKey.TitleId + ".json");
            return JsonConvert.DeserializeObject<WebtoonInfo>(json);
        }
        /// <summary>
        /// <paramref name="webtoonKey"/>가 지정한 웹툰의 다운로드된 이미지 갯수를 불러옵니다.
        /// </summary>
        /// <param name="webtoonKey"></param>
        /// <returns></returns>
        private int GetDownloadedImageCount(WebtoonKey webtoonKey)
        {
            int count = 0;
            int lastEpisode = webtoons[webtoonKey.TitleId].GetLastEpisodeNo();
            for (int episodeNo = 1; episodeNo <= lastEpisode; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                int imageCount = webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo].EpisodeImageUrls.Length;
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
            return IO.Exists(BuildImageFileFullDirectory(imageKey),BuildImageFileName(imageKey));
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
                BuildEpisodeDirectoryName(imageKey);
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
            string webtoonTitle = webtoons[imageKey.TitleId].WebtoonTitle;
            string episoneTitle = webtoons[imageKey.TitleId].Episodes[imageKey.EpisodeNo].EpisodeTitle;
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
            string date = webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo].EpisodeDate;
            string webtoonTitle = webtoons[episodeKey.TitleId].WebtoonTitle;
            string episodeTitle = webtoons[episodeKey.TitleId].Episodes[episodeKey.EpisodeNo].EpisodeTitle;
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
            string webtoonTitle = webtoons[episodeKey.TitleId].WebtoonTitle;
            return string.Format(config.WebtoonDirectoryNameFormat,
                titleId,
                ReplaceFileName(webtoonTitle));
        }
        private Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
        private string ReplaceFileName(string filename) => regex.Replace(filename, "&");
    }
}
