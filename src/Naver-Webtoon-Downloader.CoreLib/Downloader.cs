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
        private static NaverWebtoonClient _client;

        public Config Config { get; }

        static Downloader()
        {
            _client = new NaverWebtoonClient();
        }

        public Downloader(Config config)
        {
            Config = config;
        }

        public async Task UpdateDbAsync(Webtoon webtoon,
                                        int from,
                                        Action<string> log,
                                        IProgress<(int Pos, int Count, Episode episode)> progress,
                                        CancellationToken ct)
        {
            List<Episode> episodes = new List<Episode>();
            int to = await _client.GetLatestEpisodeNoAsync((int)webtoon.ID);
            log("이미지 URI 추출중..");
            foreach (int no in Enumerable.Range(from, to - from + 1))
            {
                try
                {
                    var episode = await _client.GetEpisodeAsync((int)webtoon.ID, no);
                    episodes.Add(episode);
                    progress.Report((no, to, episode));
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
            log("이미지 URI 추출 완료.");
        }

        public async Task DownloadAsync(Webtoon webtoon,
                                        Action<string> log,
                                        IProgress<(int Pos, int Count, int Size)> progress,
                                        CancellationToken ct)
        {
            Image[] images;
            using (var context = new WebtoonDbContext())
            {
                images =
                    (from img in context.Images.Include(x => x.Episode).Include(x => x.Webtoon)
                     where !img.IsDownloaded && img.WebtoonID == webtoon.ID
                     select img).ToArray();
            }
            if (images.Length == 0)
            {
                log($"다운로드할 이미지가 존재하지 않습니다.");
                return;
            }

            log("다운로드중..");
            List<Task> downloadTasks = new List<Task>();
            for (int i = 0, runningTaskCount = 0, position = 0, totalSize = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (ct.IsCancellationRequested)
                    break;
                if (runningTaskCount >= Config.MaxConnections)
                    await Task.WhenAny(downloadTasks);

                Task downloadTask = Task.Run(async () =>
                {
                    runningTaskCount++;
                    byte[] buff = await _client.GetImageFileAsync(image);
                    string folder =
                        $"{Config.DownloadFolder}\\" +
                        $"{Config.NameFormat.BuildWebtoonFolderName(webtoon)}\\" +
                        $"{Config.NameFormat.BuildEpisodeFolderName(image.Episode)}";
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    await File.WriteAllBytesAsync(
                            folder + "\\" +
                            $"{Config.NameFormat.BuildImageFileName(image)}",
                            buff);
                    using (var context = new WebtoonDbContext())
                    {
                        image.Size = buff.Length;
                        image.IsDownloaded = true;
                        context.Update(image);
                        await context.SaveChangesAsync();
                    }

                    progress.Report((++position, images.Length, totalSize += buff.Length));
                    runningTaskCount--;
                });

                downloadTasks.Add(downloadTask);
            }
            log("마무리 작업 진행중..");
            await Task.WhenAll(downloadTasks);
            log("다운로드 완료");
        }
    }
}
