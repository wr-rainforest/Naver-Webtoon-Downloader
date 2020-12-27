using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using NaverWebtoonDownloader.CoreLib.Database;

namespace NaverWebtoonDownloader.CoreLib
{
    public class Downloader
    {
        private static readonly NaverWebtoonClient _client = new NaverWebtoonClient();

        private readonly Config _config;

        public Downloader(Config config)
        {
            _config = config;
        }

        public async Task<string> CanDownload(int titleId)
        {
            HtmlDocument document;
            try
            {
                document = await _client.GetListPageDocumentAsync(titleId);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(WebtoonNotFoundException))
                    return "웹툰 정보가 존재하지 않습니다.";
                else
                    throw;
            }
            if (document.DocumentNode.InnerHtml.Contains("완결까지 정주행!"))
            {
                return "유료 웹툰은 다운로드가 불가능합니다.";
            }
            if (document.DocumentNode.InnerHtml.Contains("18세 이상 이용 가능") && !NaverWebtoonClient.IsLogined)
            {
                return "쿠키 적용 후 다운로드를 진행해 주세요.";
            }
            if (document.DocumentNode.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value.Contains("hallenge"))
            {
                return "베스트도전/도전만화는 다운로드가 불가능합니다.";
            }
            return null;
        }

        public async Task DownloadAsync(Webtoon webtoon,
                                        IProgress<(int Pos, int Count, Image Image)> progress,
                                        CancellationToken ct)
        {
            int count;
            List<Image> images;
            using (var context = new WebtoonDbContext())
            {
                var countQuery = from i in context.Images
                                 where i.WebtoonID == webtoon.ID
                                 select 0;
                count = countQuery.Count();
                var query = from i in context.Images.Include(x => x.Webtoon).Include(x => x.Episode)
                            where i.WebtoonID == webtoon.ID && !i.IsDownloaded
                            select i;
                images = await query.ToListAsync();
            }
            if (images.Count == 0)
                return;

            List<Task> downloadTasks = new List<Task>();
            for (int i = 0, runningTaskCount = 0, pos = count - images.Count; i < images.Count; i++)
            {
                if (ct.IsCancellationRequested)
                    break;
                if (runningTaskCount >= _config.MaxConnections)
                {
                    try
                    {
                        await Task.WhenAny(downloadTasks);
                    }
                    catch
                    {
                        using (var context = new WebtoonDbContext())
                        {
                            context.Images.UpdateRange(images);
                            await context.SaveChangesAsync();
                        }
                        throw;
                    }
                    i--;
                    continue;
                }
                Image image = images[i];
                runningTaskCount++;
                Task downloadTask = Task.Run(async () =>
                {
                    if (ct.IsCancellationRequested)
                        return;
                    byte[] buff = await _client.GetImageFileAsync(image);
                    string directory =
                        $"{_config.DownloadFolder}\\" +
                        $"{_config.NameFormat.BuildWebtoonFolderName(webtoon)}\\" +
                        $"{_config.NameFormat.BuildEpisodeFolderName(image.Episode)}";
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    await File.WriteAllBytesAsync(
                            directory + "\\" +
                            $"{_config.NameFormat.BuildImageFileName(image)}",
                            buff);
                    image.Size = buff.Length;
                    image.IsDownloaded = true;
                    progress.Report((++pos, count, image));
                    runningTaskCount--;
                });
                downloadTasks.Add(downloadTask);
            }
            await Task.WhenAll(downloadTasks);
            using (var context = new WebtoonDbContext())
            {
                context.Images.UpdateRange(images);
                await context.SaveChangesAsync();
            }
            ct.ThrowIfCancellationRequested();
        }

        public async Task<int> UpdateWebtoonDbAsync(Webtoon webtoon,
                                                    IProgress<Episode> progress,
                                                    CancellationToken ct)
        {
            int lastEpisodeNo = await Task.Run(() =>
            {
                var context = new WebtoonDbContext();
                var query = from e in context.Episodes
                            where e.WebtoonID == webtoon.ID
                            select e.No;
                if (!query.Any())
                    return 0;
                else
                    return (int)query.Max();
            });
            int latestEpisodeNo = await _client.GetLatestEpisodeNoAsync((int)webtoon.ID);

            if (latestEpisodeNo == lastEpisodeNo)
                return 0;
            else if (latestEpisodeNo < lastEpisodeNo)
                return -1;
            List<Episode> episodes = new List<Episode>();
            foreach (int no in Enumerable.Range(lastEpisodeNo + 1, latestEpisodeNo - lastEpisodeNo))
            {
                if (ct.IsCancellationRequested)
                    break;
                try
                {
                    var episode = await _client.GetEpisodeAsync((int)webtoon.ID, no);
                    episodes.Add(episode);
                    progress.Report(episode);
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(EpisodeNotFoundException))
                        continue;
                    else
                        throw;
                }
            }
            using (var context = new WebtoonDbContext())
            {
                await context.Episodes.AddRangeAsync(episodes);
                await context.SaveChangesAsync();
            }
            ct.ThrowIfCancellationRequested();
            return episodes.Count;
        }
    }
}
