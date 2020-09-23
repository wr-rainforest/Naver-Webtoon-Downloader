using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;
using WRforest.NWD.Parser;

namespace WRforest.NWD
{
    class Downloader
    {
        public delegate void ProgressDelegate(string progress);
        private static ProgressDelegate PrintProgress;
        private static Config config;
        public static void SetProgressDelegate(ProgressDelegate progressDelegate)
        {
            PrintProgress = progressDelegate;
        }
        public static void SetConfig(Config config)
        {
            Downloader.config = config;
        }
        /// <summary>
        /// "{0}({1}) [{2}/{3}] ({4:P}) [{5}]"
        /// <code>{0}:웹툰 제목</code>
        /// <code>{1}:웹툰 아이디</code>
        /// <code>{2}:현재 포지션</code>
        /// <code>{3}:총 작업수</code>
        /// <code>{4}:퍼센트</code>
        /// <code>{5}:회차 날짜</code>
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="ProgressTextFormat"></param>
        public static void UpdateWebtoonInfo(WebtoonInfo webtoonInfo, string ProgressTextFormat)
        {
            Agent agent = Agent.Instance;
            Parser.Parser parser = Parser.Parser.Instance;
            WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);

            //comic.naver.com에서 최신 회차의 EpisodeNo를 불러옵니다.
            agent.LoadPage(webtoonKey.BuildUrl());
            int latestEpisodeNo = int.Parse(parser.GetLatestEpisodeNo());
            //webtoonInfo중 가장 마지막 회차의 EpisodeNo를 불러옵니다.
            int lastEpisodeNo = webtoonInfo.GetLastEpisodeNo();

            //웹툰 정보를 업데이트합니다.
            for (int episodeNo = lastEpisodeNo + 1; episodeNo <= latestEpisodeNo; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                agent.LoadPage(episodeKey.BuildUrl());
                string currentEpisodeNo = parser.GetCurrentEpisodeNo();
                if (!currentEpisodeNo.Equals(episodeNo.ToString()))
                {
                    //비어있는 번호 건너뛰기
                    continue;
                }
                string episodeTitle = parser.GetEpisodeTitle();
                string episodeDate = parser.GetEpisodeDate();
                string[] imageUrls = parser.GetComicContentImageUrls();
                webtoonInfo.Episodes.Add(episodeNo, new EpisodeInfo(episodeKey, episodeTitle, imageUrls, episodeDate));
                PrintProgress(string.Format(ProgressTextFormat,
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle));
            }
            Console.WriteLine();
        }

        /// <summary>
        /// "{0}({1}) [{2}/{3}] ({4:P}) [{5}]"
        /// <code>{0}:웹툰 제목</code>
        /// <code>{1}:웹툰 아이디</code>
        /// <code>{2}:현재 포지션</code>
        /// <code>{3}:총 작업수</code>
        /// <code>{4}:퍼센트</code>
        /// <code>{5}:회차 날짜</code>
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="ProgressTextFormat"></param>
        public static void BuildWebtoonInfo(WebtoonInfo webtoonInfo, string ProgressTextFormat)
        {
            Agent agent = Agent.Instance;
            Parser.Parser parser = Parser.Parser.Instance;
            WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
            //comic.naver.com에서 최신 회차의 EpisodeNo를 불러옵니다.
            agent.LoadPage(webtoonKey.BuildUrl());
            int latestEpisodeNo = int.Parse(parser.GetLatestEpisodeNo());
            //웹툰 정보를 업데이트합니다.
            for (int episodeNo = 1; episodeNo <= latestEpisodeNo; episodeNo++)
            {
                EpisodeKey episodeKey = new EpisodeKey(webtoonKey.TitleId, episodeNo);
                agent.LoadPage(episodeKey.BuildUrl());
                string currentEpisodeNo = parser.GetCurrentEpisodeNo();
                if (!currentEpisodeNo.Equals(episodeNo.ToString()))
                {
                    //비어있는 번호 건너뛰기
                    continue;
                }
                string episodeTitle = parser.GetEpisodeTitle();
                string episodeDate = parser.GetEpisodeDate();
                string[] imageUrls = parser.GetComicContentImageUrls();
                webtoonInfo.Episodes.Add(episodeNo, new EpisodeInfo(episodeKey, episodeTitle, imageUrls, episodeDate));
                PrintProgress(string.Format(ProgressTextFormat,
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle));
            }
            Console.WriteLine();
        }
        public static ImageKey[] BuildImageKeysToDown(WebtoonInfo webtoonInfo)
        {
            List<ImageKey> list = new List<ImageKey>();
            int latest = webtoonInfo.GetLastEpisodeNo();
            for(int episodeNo = 1; episodeNo < latest; episodeNo++)
            {
                if (!webtoonInfo.Episodes.ContainsKey(episodeNo))
                    continue;
                EpisodeInfo episodeInfo = webtoonInfo.Episodes[episodeNo];
                string[] imageUrls = episodeInfo.EpisodeImageUrls;
                for(int imageIndex = 0; imageIndex < imageUrls.Length; imageIndex++)
                {
                    ImageKey imageKey = new ImageKey(webtoonInfo.WebtoonTitleId, episodeNo, imageIndex);
                    if(IO.Exists(BuildImageFileFullDirectory(webtoonInfo,imageKey), BuildImageFileName(webtoonInfo, imageKey)))
                    {
                        continue;
                    }
                    list.Add(imageKey);
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// <code>{0}:웹툰 제목</code>
        /// <code>{1}:웹툰 아이디</code>
        /// <code>{2}:현재 포지션</code>
        /// <code>{3}:총 작업수</code>
        /// <code>{4}:퍼센트</code>
        /// <code>{5}:회차 날짜</code>
        /// <code>{6}:회차 제목</code>
        /// <code>{7}:이미지 인덱스</code>
        /// <code>{8}:이미지 파일명</code>
        /// <code>{9}:다운받은 메가바이트 용량</code>
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="imageKeys"></param>
        /// <param name="ProgressTextFormat"></param>
        public static void Download(WebtoonInfo webtoonInfo, ImageKey[] imageKeys, string ProgressTextFormat)
        {
            IO.taskEnd = false;
            Task t = new Task(new Action(()=> { IO.SaveFileAsync(); }));
            Agent agent = Agent.Instance;
            long size = 0;
            t.Start();
            for(int i=0; i<imageKeys.Length; i++)
            {
                
                EpisodeKey episodeKey = new EpisodeKey(imageKeys[i].TitleId, imageKeys[i].EpisodeNo);
                agent.SetHeader("Referer", episodeKey.BuildUrl());
                byte[] buff = agent.DownloadData(webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeImageUrls[imageKeys[i].ImageIndex]);
                
                IO.WriteAllBytes(
                    BuildImageFileFullDirectory(webtoonInfo, imageKeys[i]),
                    BuildImageFileName(webtoonInfo, imageKeys[i]),
                    buff);
                size += buff.Length;
                PrintProgress(string.Format(ProgressTextFormat,
                    webtoonInfo.WebtoonTitle,
                    webtoonInfo.WebtoonTitleId,
                    i+1,
                    imageKeys.Length,
                    (double)(i+1) / imageKeys.Length,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeTitle,
                    imageKeys[i].ImageIndex,
                    BuildImageFileName(webtoonInfo, imageKeys[i]),
                    (double)size/1048576
                    ));
            }
            IO.taskEnd = true;
            t.Wait();
            Console.WriteLine();
        }
        public static (int downloadedImageCount, long downloadedImagesSize) GetDownloadedImagesInformation(WebtoonInfo webtoonInfo)
        {
            WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
            int count = 0;
            long size = 0;
            int lastEpisode = webtoonInfo.GetLastEpisodeNo();
            for (int episodeNo = 1; episodeNo <= lastEpisode; episodeNo++)
            {
                if (!webtoonInfo.Episodes.ContainsKey(episodeNo))
                    continue;
                EpisodeInfo episodeInfo = webtoonInfo.Episodes[episodeNo];
                string[] imageUrls = episodeInfo.EpisodeImageUrls;
                for (int imageIndex = 0; imageIndex < imageUrls.Length; imageIndex++)
                {
                    ImageKey imageKey = new ImageKey(webtoonInfo.WebtoonTitleId, episodeNo, imageIndex);
                    if (IO.Exists(BuildImageFileFullDirectory(webtoonInfo, imageKey), BuildImageFileName(webtoonInfo, imageKey)))
                    {
                        count++;
                        size += new FileInfo(BuildImageFileFullDirectory(webtoonInfo, imageKey) +"\\"+ BuildImageFileName(webtoonInfo, imageKey)).Length;
                    }
                }
            }
            return (count, size);
        }


        /// <summary>
        /// imageKey가 지정하는 이미지의 전체 경로 문자열을 반환합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        private static string BuildImageFileFullDirectory(WebtoonInfo webtoonInfo, ImageKey imageKey)
        {
            string directory =
                config.DefaultDownloadDirectory + "\\" +
                BuildWebtoonDirectoryName(webtoonInfo, imageKey) + "\\" +
                BuildEpisodeDirectoryName(webtoonInfo, imageKey);
            return directory;
        }
        /// <summary>
        /// imageKey가 지정하는 이미지의 파일명을 정의된 포맷 형식(<seealso cref="Config.ImageFileNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="imageKey"></param>
        /// <returns></returns>
        private static string BuildImageFileName(WebtoonInfo webtoonInfo, ImageKey imageKey)
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
                webtoonInfo.Episodes[imageKey.EpisodeNo].EpisodeDate);//이미지 파일은 반각 온점 ok
        }

        /// <summary>
        /// episodeKey가 지정하는 회차의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.EpisodeDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        private static string BuildEpisodeDirectoryName(WebtoonInfo webtoonInfo, EpisodeKey episodeKey)
        {
            string titleId = episodeKey.TitleId;
            int episodeNo = episodeKey.EpisodeNo;
            string date = webtoonInfo.Episodes[episodeKey.EpisodeNo].EpisodeDate;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            string episodeTitle = webtoonInfo.Episodes[episodeKey.EpisodeNo].EpisodeTitle;
            return string.Format(config.EpisodeDirectoryNameFormat,
                titleId,
                episodeNo,
                date,
                ReplaceFolderName(webtoonTitle),
                ReplaceFolderName(episodeTitle));
        }

        /// <summary>
        /// webtoonKey가 지정하는 웹툰의 폴더 이름을 정의된 포맷 형식(<seealso cref="Config.WebtoonDirectoryNameFormat"/>)에 따라 생성합니다.
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        private static string BuildWebtoonDirectoryName(WebtoonInfo webtoonInfo, WebtoonKey webtoonKey)
        {
            string titleId = webtoonInfo.WebtoonTitleId;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            return string.Format(config.WebtoonDirectoryNameFormat,
                titleId,
                ReplaceFolderName(webtoonTitle));
        }

        //private static Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
        private static string ReplaceFileName(string filename) 
        {
            return filename.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
        private static string ReplaceFolderName(string name)
        {
            if(name[name.Length-1]=='.')
            {
                name = name.Substring(0, name.Length-1)+ "．";//끝이 반각 온점일경우 전각으로 교체
            }
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
