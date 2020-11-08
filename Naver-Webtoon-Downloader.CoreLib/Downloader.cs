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
        private WebtoonDatabase database;
        private NameFormat format;

        static Downloader()
        {
            client = new NaverWebtoonHttpClient();
        }

        public Downloader(NameFormat format, string dataSource)
        {
            this.format = format;
        }

        public async Task UpdateOrCreateWebtoonDatabase(string titleId, IProgress<(int position, int count)> progress, CancellationToken ct)
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
                lastEpisodeNo = lastEpisode.No;
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
                progress?.Report((i - lastEpisodeNo , latestEpisodeNo - lastEpisodeNo));
            }
        }

        public async Task DownloadWebtoonAsync(string titleId, string downloadFolder, IProgress<(int position, int count)> progress, CancellationToken ct)
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
                    return;
                var image = imagesToDownload[i];
                byte[] buff = await client.GetByteArrayAsync(image.Uri);
                string episodeDirectory = Path.Combine(downloadFolder, BuildWebtoonFolderName(webtoonInfo), BuildEpisodeFolderName(webtoonInfo, episodes[image.No]));
                string imageFilePath = Path.Combine(episodeDirectory, BuildImageFileName(webtoonInfo, episodes[image.No], image, ".jpg"));
                saveFileTasks.Add(Task.Run(() =>
                {
                    Directory.CreateDirectory(episodeDirectory);
                    File.WriteAllBytes(imageFilePath, buff);

                },ct));
                progress?.Report((i + 1, imagesToDownload.Count));
            }
            await Task.WhenAll(saveFileTasks);
        }

        private readonly static string[] parallax = new string[] { "670144" };
        public async Task<(bool value, string message)> CanDownload(string titleId)
        {
            HtmlDocument document;
            try
            {
                document = await client.GetListPageDocumentAsync(titleId);
            }
            catch(Exception e)
            {
                if (e.GetType() == typeof(WebtoonNotFoundException))
                    return (false, "웹툰 정보가 존재하지 않습니다.");
                else
                    throw e;
            }
            if (document.DocumentNode.InnerHtml.Contains("완결까지 정주행!"))
            {
                return (false, "유료 웹툰은 다운로드가 불가능합니다.");
            }
            if (document.DocumentNode.InnerHtml.Contains("18세 이상 이용 가능"))
            {
                return (false, "성인 웹툰은 다운로드가 불가능합니다.");
            }
            if (document.DocumentNode.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value.Contains("hallenge"))
            {
                return (false, "베스트도전/도전만화는 다운로드가 불가능합니다.");
            }
            if (parallax.Contains(titleId))
            {
                return (false, "애니매이션 효과가 적용된 웹툰은 다운로드가 불가능합니다.");
            }
            return (true, "");
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
