using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Data.Sqlite;
using System.IO;
using HtmlAgilityPack;

namespace NaverWebtoonDownloader.CoreLib
{
    public class Downloader
    {
        private static readonly NaverWebtoonHttpClient client;
        public static NaverWebtoonHttpClient Client => client;
        private Database database;
        public Database Database => database;
        private NameFormat format;

        static Downloader()
        {
            client = new NaverWebtoonHttpClient();
        }

        /// <summary>
        /// <seealso cref="Downloader"/>의 새 인스턴스를 초기화합니다. 
        /// </summary>
        /// <param name="dataSource">SQLite 데이터베이스 파일 위치입니다. 상대경로 또는 절대경로가 가능합니다.</param>
        /// <param name="format"></param>
        public Downloader(string dataSource, NameFormat format)
        {
            dataSource = Path.GetFullPath(dataSource);
            database = new Database(dataSource);
            this.format = format;
        }

        /// <summary>
        /// 웹툰의 로컬 데이터베이스를 갱신합니다. 데이터베이스에 해당 웹툰이 존재하지 않는 경우 새로 추가합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task UpdateOrInsertWebtoonDatabase(string titleId, IProgress<(int position, int count)> progress, CancellationToken ct)
        {
            WebtoonInfo webtoonInfo = database.SelectWebtoon(titleId);
            int lastEpisodeNo = 0;
            int latestEpisodeNo = await client.GetLatestEpisodeNoAsync(titleId);
            if (webtoonInfo == null)
            {
                webtoonInfo = await client.GetWebtoonInfoAsync(titleId);
                database.InsertWebtoon(webtoonInfo);
            }
            else
            {
                var lastEpisode = database.SelectLastEpisode(titleId);
                lastEpisodeNo = lastEpisode?.No ?? 0;
            }
            for (int i = lastEpisodeNo+1; i <= latestEpisodeNo; i++)
            {
                if (ct.IsCancellationRequested)
                    return;
                EpisodeInfo episode;
                try
                {
                    episode = await client.GetEpisodeInfoAsync(titleId, i);
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(EpisodeNotFoundException))
                        continue;
                    else
                    {
                        database.DeleteEpisodes(titleId);
                        throw new Exception("회차 정보를 불러오는데 실패하였습니다.",e);
                    }
                }
                database.InsertEpisode(episode);
                progress?.Report((i, latestEpisodeNo));
            }
        }

        /// <summary>
        /// 웹툰을 다운로드합니다. 
        /// </summary>
        /// <param name="titleId">다운로드할 웹툰의 titleId입니다.</param>
        /// <param name="downloadFolder"></param>
        /// <param name="progress">[0] int position, [1] int count, [2] int imageSize</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task DownloadWebtoonAsync(string titleId, string downloadFolder, IProgress<object[]> progress, CancellationToken ct)
        {
            WebtoonInfo webtoonInfo = database.SelectWebtoon(titleId);
            var episodes = new Dictionary<int, EpisodeInfo>();
            var imagesToDownload = new List<ImageInfo>();
            database.SelectEpisodes(titleId).ToList().ForEach((episode) =>
            {
                episodes.Add(episode.No, episode);
                episode.Images.ToList().ForEach((image) => 
                {
                    imagesToDownload.Add(image);
                });
            });
            List<Task> saveFileTasks = new List<Task>();
            for(int i = 0; i < imagesToDownload.Count; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    await Task.WhenAll(saveFileTasks);
                    return;
                }
                var image = imagesToDownload[i];
                byte[] buff = await client.GetByteArrayAsync(image.Uri);
                string episodeDirectory = Path.Combine(downloadFolder, BuildWebtoonFolderName(webtoonInfo), BuildEpisodeFolderName(webtoonInfo, episodes[image.No]));
                string imageFilePath = Path.Combine(episodeDirectory, BuildImageFileName(webtoonInfo, episodes[image.No], image, ".jpg"));
                saveFileTasks.Add(Task.Run(() =>
                {
                    if (!Directory.Exists(episodeDirectory)) { Directory.CreateDirectory(episodeDirectory); }
                    File.WriteAllBytes(imageFilePath, buff);
                    image.Downloaded = 1;
                    image.Size = buff.Length;
                    database.UpdateImage(image);
                    progress?.Report(new object[] { i + 1, imagesToDownload.Count, image.Size });
                }));
            }
            await Task.WhenAll(saveFileTasks);
        }

        private readonly static string[] parallax = new string[] { "670144" };

        /// <summary>
        /// 지정한 웹툰의 다운로드 가능 여부를 확인합니다. 
        /// 다운로드 가능한 웹툰일 경우 null, 불가능한 웹툰일 경우 오류 메세지를 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<string> CanDownload(string titleId)
        {
            HtmlDocument document;
            try
            {
                document = await client.GetListPageDocumentAsync(titleId);
            }
            catch(Exception e)
            {
                if (e.GetType() == typeof(WebtoonNotFoundException))
                    return "웹툰 정보가 존재하지 않습니다.";
                else
                    throw e;
            }
            if (document.DocumentNode.InnerHtml.Contains("완결까지 정주행!"))
            {
                return "유료 웹툰은 다운로드가 불가능합니다.";
            }
            if (document.DocumentNode.InnerHtml.Contains("18세 이상 이용 가능"))
            {
                return "성인 웹툰은 다운로드가 불가능합니다.";
            }
            if (document.DocumentNode.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value.Contains("hallenge"))
            {
                return "베스트도전/도전만화는 다운로드가 불가능합니다.";
            }
            if (parallax.Contains(titleId))
            {
                return "애니매이션 효과가 적용된 웹툰은 다운로드가 불가능합니다.";
            }
            return null;
        }
        private string BuildImageFileName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo,ImageInfo imageInfo, string extension)
            => ReplaceFileName(string.Format(format.ImageFileNameFormat, episodeInfo.TitleId, episodeInfo.No, imageInfo.Index, webtoonInfo.Title, episodeInfo.Title, episodeInfo.Date) + extension);

        private string BuildEpisodeFileName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo, string extension) 
            => ReplaceFileName(string.Format(format.EpisodeDirectoryNameFormat, webtoonInfo.TitleId, episodeInfo.No, episodeInfo.Date, webtoonInfo.Title, episodeInfo.Title, webtoonInfo.Writer)+extension);
        
        private string BuildEpisodeFolderName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo)
            => ReplaceFolderName(string.Format(format.EpisodeDirectoryNameFormat, episodeInfo.TitleId, episodeInfo.No, episodeInfo.Date, webtoonInfo.Title, episodeInfo.Title, webtoonInfo.Writer));
        
        private string BuildWebtoonFolderName(WebtoonInfo webtoonInfo)
            => ReplaceFolderName(string.Format(format.WebtoonDirectoryNameFormat, webtoonInfo.TitleId, webtoonInfo.Title, webtoonInfo.Writer));
        
        private string ReplaceFolderName(string name)
        {
            if (name[name.Length - 1] == '.')
                name = name.Substring(0, name.Length - 1) + "．";
            return name.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }

        private string ReplaceFileName(string filename)
        {
            return filename.Replace('/', '／').Replace('\\', '＼').Replace('?', '？').Replace('*', '＊').Replace(':', '：').Replace('|', '｜').Replace('\"', '＂').Replace("&lt;", "＜").Replace("&gt;", "＞");
        }
    }
}
