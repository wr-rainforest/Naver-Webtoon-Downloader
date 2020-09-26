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
        public delegate void ProgressChangedEventHandler(string progressMessage);
        public event ProgressChangedEventHandler ProgressChangedEvent;
        private Config config;
        private WebtoonInfo webtoonInfo;
        Agent agent;
        Parser.Parser parser;

        public Downloader(WebtoonInfo webtoonInfo, Config config, XPath xPath)
        {
            agent = new Agent();
            parser = new Parser.Parser(xPath);
            this.webtoonInfo = webtoonInfo;
            this.config = config;
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
        public void UpdateWebtoonInfo(string ProgressTextFormat)
        {
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
                ProgressChangedEvent(string.Format(ProgressTextFormat,
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle));
            }
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
        public void BuildWebtoonInfo(string ProgressTextFormat)
        {

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
                ProgressChangedEvent(string.Format(ProgressTextFormat,
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo).ToString("D" + latestEpisodeNo.ToString().Length.ToString()),
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle));
            }
        }
        public ImageKey[] BuildImageKeysToDown()
        {
            List<ImageKey> list = new List<ImageKey>();
            int lastEpisodeNo = webtoonInfo.GetLastEpisodeNo();
            for(int episodeNo = 1; episodeNo <= lastEpisodeNo; episodeNo++)
            {
                if (!webtoonInfo.Episodes.ContainsKey(episodeNo))
                    continue;
                EpisodeInfo episodeInfo = webtoonInfo.Episodes[episodeNo];
                string[] imageUrls = episodeInfo.EpisodeImageUrls;
                for(int imageIndex = 0; imageIndex < imageUrls.Length; imageIndex++)
                {
                    ImageKey imageKey = new ImageKey(webtoonInfo.WebtoonTitleId, episodeNo, imageIndex);
                    if(ImageExists(imageKey))
                    {
                        continue;
                    }
                    list.Add(imageKey);
                }
            }
            return list.ToArray();
        }
        public bool ImageExists(ImageKey imageKey)
        {
            string directory = BuildImageFileFullDirectory(imageKey);
            string filename = BuildImageFileName(imageKey);
            return File.Exists(string.Format("{0}\\{1}", directory, filename));
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
        public void Download(ImageKey[] imageKeys, string ProgressTextFormat)
        {
            IO.taskEnd = false;
            Task t = new Task(new Action(()=> { IO.SaveFileAsync(); }));
            long size = 0;
            t.Start();
            for(int i=0; i<imageKeys.Length; i++)
            {
                
                EpisodeKey episodeKey = new EpisodeKey(imageKeys[i].TitleId, imageKeys[i].EpisodeNo);
                agent.SetHeader("Referer", episodeKey.BuildUrl());
                byte[] buff = agent.DownloadData(webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeImageUrls[imageKeys[i].ImageIndex]);
                
                IO.WriteAllBytes(
                    BuildImageFileFullDirectory(imageKeys[i]),
                    BuildImageFileName(imageKeys[i]),
                    buff);
                size += buff.Length;
                ProgressChangedEvent(string.Format(ProgressTextFormat,
                    webtoonInfo.WebtoonTitle,
                    webtoonInfo.WebtoonTitleId,
                    i+1,
                    imageKeys.Length,
                    (double)(i+1) / imageKeys.Length,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeTitle,
                    imageKeys[i].ImageIndex,
                    BuildImageFileName(imageKeys[i]),
                    (double)size/1048576
                    ));
            }
            IO.taskEnd = true;
            t.Wait();
        }
        public (int downloadedImageCount, long downloadedImagesSize) GetDownloadedImagesInformation()
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
                    if (IO.Exists(BuildImageFileFullDirectory(imageKey), BuildImageFileName(imageKey)))
                    {
                        count++;
                        size += new FileInfo(BuildImageFileFullDirectory(imageKey) +"\\"+ BuildImageFileName(imageKey)).Length;
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
        private string BuildEpisodeDirectoryName(EpisodeKey episodeKey)
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
        private string BuildWebtoonDirectoryName(WebtoonKey webtoonKey)
        {
            string titleId = webtoonInfo.WebtoonTitleId;
            string webtoonTitle = webtoonInfo.WebtoonTitle;
            return string.Format(config.WebtoonDirectoryNameFormat,
                titleId,
                ReplaceFolderName(webtoonTitle));
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
            if(name[name.Length-1]=='.')
            {
                name = name.Substring(0, name.Length-1)+ "．";
            }
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
