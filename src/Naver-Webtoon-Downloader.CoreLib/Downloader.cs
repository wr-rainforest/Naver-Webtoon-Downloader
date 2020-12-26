using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                Image image = images[i];
                ct.ThrowIfCancellationRequested();
                if (runningTaskCount >= _config.MaxConnections)
                    await Task.WhenAny(downloadTasks);

                Task downloadTask = Task.Run(async () =>
                {
                    runningTaskCount++;
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
                            buff, ct);
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
                await context.SaveChangesAsync(ct);
            }
        }

        public async Task<int> UpdateWebtoonDbAsync(Webtoon webtoon,
                                                    IProgress<(int Pos, int Count, Episode Episode)> progress,
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
                ct.ThrowIfCancellationRequested();
                try
                {
                    var episode = await _client.GetEpisodeAsync((int)webtoon.ID, no);
                    episodes.Add(episode);
                    progress.Report((no, latestEpisodeNo, episode));
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
                await context.SaveChangesAsync(ct);
            }
            return episodes.Count;
        }
    }
}
