using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD
{
    class Downloader
    {
        private WebtoonInfo webtoonInfo;
        Agent agent;
        Parser parser;
        private FileNameBuilder fileNameBuilder;

        public Downloader(WebtoonInfo webtoonInfo, Config config)
        {
            agent = new Agent();
            parser = new Parser(agent);
            this.webtoonInfo = webtoonInfo;
            fileNameBuilder = new FileNameBuilder(webtoonInfo, config);
        }

        private async Task SaveFileAsync(string directory, string filename, byte[] data)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            //File.WriteAllBytesAsync() => .net 5.0
            using (FileStream fs=new FileStream(directory+"\\"+filename, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(data, 0, data.Length);
            }
        }
#if Console
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
        public async Task DownloadAsync(ImageKey[] imageKeys, string ProgressTextFormat, IProgress<string> progress /*CancellationToken cancellationToken*/)
        {

            List<Task> tasks = new List<Task>();
            long size = 0;
            for (int i = 0; i < imageKeys.Length; i++)
            {
                byte[] buff;
                try
                {
                    buff = agent.DownloadWebtoonImage(webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeImageUrls[imageKeys[i].ImageIndex]);
                    size += buff.Length;
                    //400 => 헤더 용량 문제
                    //403 => referer 추가
                    //404 => todo 캐시 초기화 기능 추가
                }
                catch
                {
                    progress.Report("\r\n다운로드 오류가 발생했습니다. 5초뒤 다음 파일로 건너뜁니다.");
                    Thread.Sleep(5000);
                    continue;
                }

                progress.Report(string.Format(ProgressTextFormat,
                    webtoonInfo.WebtoonTitle,
                    webtoonInfo.WebtoonTitleId,
                    i + 1,
                    imageKeys.Length,
                    (double)(i + 1) / imageKeys.Length,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeTitle,
                    imageKeys[i].ImageIndex,
                    fileNameBuilder.BuildImageFileName(imageKeys[i]),
                    (double)size / 1048576
                    ));
                tasks.Add(SaveFileAsync(fileNameBuilder.BuildImageFileFullDirectory(imageKeys[i]), fileNameBuilder.BuildImageFileName(imageKeys[i]), buff));
            }
            await Task.WhenAll(tasks);

            return;
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
        public async Task DownloadAsync(ImageKey[] imageKeys, string ProgressTextFormat, IProgress<string> progress, CancellationToken cancellationToken)
        {

            List<Task> tasks = new List<Task>();
            long size = 0;
            for (int i = 0; i < imageKeys.Length; i++)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                byte[] buff;
                try
                {
                    buff = agent.DownloadWebtoonImage(webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeImageUrls[imageKeys[i].ImageIndex]);
                    size += buff.Length;
                    //400 => 헤더 용량 문제
                    //403 => referer 추가
                    //404 => todo 캐시 초기화 기능 추가
                }
                catch
                {
                    progress.Report("\r\n다운로드 오류가 발생했습니다. 5초뒤 다음 파일로 건너뜁니다.");
                    Thread.Sleep(5000);
                    continue;
                }

                progress.Report(string.Format(ProgressTextFormat,
                    webtoonInfo.WebtoonTitle,
                    webtoonInfo.WebtoonTitleId,
                    i + 1,
                    imageKeys.Length,
                    (double)(i + 1) / imageKeys.Length,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeTitle,
                    imageKeys[i].ImageIndex,
                    fileNameBuilder.BuildImageFileName(imageKeys[i]),
                    (double)size / 1048576
                    ));
                tasks.Add(SaveFileAsync(fileNameBuilder.BuildImageFileFullDirectory(imageKeys[i]), fileNameBuilder.BuildImageFileName(imageKeys[i]), buff));
            }
            await Task.WhenAll(tasks);
            return;
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
        public void UpdateWebtoonInfo(string ProgressTextFormat, IProgress<string> progress)
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
                progress.Report(string.Format(ProgressTextFormat,
                               webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              episodeNo,
                             latestEpisodeNo,
                             (decimal)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle));
            }
        }
#endif
#if WPF
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
        /// <code>{10}:다운로드된 파일수</code>
        /// <code>{11}:총 파일수</code>
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="imageKeys"></param>
        /// <param name="ProgressTextFormat"></param>
        public async Task DownloadAsync(ImageKey[] imageKeys,IProgress<object[]> progress, CancellationToken cancellationToken)
        {
            int fileCount = webtoonInfo.GetImageCount();
            int downloadedCount = fileCount - imageKeys.Length;
            List<Task> tasks = new List<Task>();
            long size = 0;
            for (int i = 0; i < imageKeys.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                byte[] buff;
                try
                {
                    buff = agent.DownloadWebtoonImage(webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeImageUrls[imageKeys[i].ImageIndex]);
                    size += buff.Length;
                    //400 => 헤더 용량 문제
                    //403 => referer 추가
                    //404 => todo 캐시 초기화 기능 추가
                }
                catch
                {
                    continue;
                }

                progress.Report(new object[] {
                    webtoonInfo.WebtoonTitle,
                    webtoonInfo.WebtoonTitleId,
                    i + 1,
                    imageKeys.Length,
                    (double)(i + 1) / imageKeys.Length,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeDate,
                    webtoonInfo.Episodes[imageKeys[i].EpisodeNo].EpisodeTitle,
                    imageKeys[i].ImageIndex,
                    fileNameBuilder.BuildImageFileName(imageKeys[i]),
                    (double)size / 1048576,
                    downloadedCount,
                    fileCount
                    });
                downloadedCount++;
                
                tasks.Add(SaveFileAsync(fileNameBuilder.BuildImageFileFullDirectory(imageKeys[i]), fileNameBuilder.BuildImageFileName(imageKeys[i]), buff));
            }
            await Task.WhenAll(tasks);
            return;
        }
#if false
        /// <summary>
        /// "{0}({1}) [{2}/{3}] ({4:P}) [{5}]"
        /// <code>{0}:웹툰 제목</code>
        /// <code>{1}:웹툰 아이디</code>
        /// <code>{2}:현재 포지션</code>
        /// <code>{3}:총 작업수</code>
        /// <code>{4}:퍼센트</code>
        /// <code>{5}:회차 날짜</code>
        /// <code>{6}:회차 제목</code>
        /// 
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="ProgressTextFormat"></param>
        public void UpdateWebtoonInfo(IProgress<object[]> progress)
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
                progress.Report(new object[]{
                    webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo),
                             latestEpisodeNo,
                             (double)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle});
            }
            Thread.Sleep(500);
        }
#endif
        /// <summary>
        /// "{0}({1}) [{2}/{3}] ({4:P}) [{5}]"
        /// <code>{0}:웹툰 제목</code>
        /// <code>{1}:웹툰 아이디</code>
        /// <code>{2}:현재 포지션</code>
        /// <code>{3}:총 작업수</code>
        /// <code>{4}:퍼센트</code>
        /// <code>{5}:회차 날짜</code>
        /// <code>{6}:회차 제목</code>
        /// <code>{7}:캐싱된 회차수</code>
        /// <code>{8}:총 회차수</code>
        /// </summary>
        /// <param name="webtoonInfo"></param>
        /// <param name="ProgressTextFormat"></param>
        public async Task UpdateWebtoonInfoAsync(IProgress<object[]> progress, CancellationToken cancellationToken)
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
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
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
                progress.Report(new object[]{
                    webtoonInfo.WebtoonTitle,
                               webtoonInfo.WebtoonTitleId,
                              (episodeNo),
                             latestEpisodeNo,
                             (double)(episodeNo) / latestEpisodeNo,
                             webtoonInfo.Episodes[episodeNo].EpisodeDate,
                             webtoonInfo.Episodes[episodeNo].EpisodeTitle});
            }
        }
#endif
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
                    if(File.Exists(fileNameBuilder.BuildImageFileFullPath(imageKey)))
                    {
                        continue;
                    }
                    list.Add(imageKey);
                }
            }
            return list.ToArray();
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
                    if (File.Exists(fileNameBuilder.BuildImageFileFullPath(imageKey)))
                    {
                        count++;
                        size += new FileInfo(fileNameBuilder.BuildImageFileFullPath(imageKey)).Length;
                    }
                }
            }
            return (count, size);
        }
    }
}
