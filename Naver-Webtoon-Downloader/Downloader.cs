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

namespace wr_rainforest.Naver_Webtoon_Downloader
{
    class Downloader
    {
        private Config config;
        private static readonly NaverWebtoonHttpClient client;
        private SqliteConnection sqliteConnection;
        static Downloader()
        {
            client = new NaverWebtoonHttpClient();
        }

        public Downloader(Config config, string dataSource)
        {
            if (config.Version.Major != 1)
            {
                throw new Exception("지원하지 않는 설정 포맷입니다.");
            }
            this.config = config;
            this.sqliteConnection = new SqliteConnection($"Data Source = {dataSource}");
            var selectMasterCommand = sqliteConnection.CreateCommand();
            selectMasterCommand.CommandText = "SELECT name from sqlite_master where type='table';";
            var tableNameReader = selectMasterCommand.ExecuteReader();
            bool webtoonsTableExists = false, episodesTableExists = false, imagesTableExists = false;
            while (tableNameReader.Read())
            {
                string tablename = (string)tableNameReader[0];
                if (tablename == "webtoons")
                    webtoonsTableExists = true;
                else if (tablename == "episodes")
                    episodesTableExists = true;
                else if (tablename == "images")
                    imagesTableExists = true;
            }
            var createTableCommand = sqliteConnection.CreateCommand();
            createTableCommand.CommandText = "BEGIN; ";
            if (!webtoonsTableExists)
            {
                createTableCommand.CommandText += "CREATE TABLE 'webtoons' ('titleId' TEXT, 'title' TEXT, 'writer' TEXT, 'genre' TEXT, 'description' TEXT, PRIMARY KEY(titleId));";
            }
            if (!episodesTableExists)
            {
                createTableCommand.CommandText += "CREATE TABLE 'episodes' ( 'titleId' TEXT, 'no' INTEGER, 'title' INTEGER, 'date' INTEGER, 'image_count' INTEGER, PRIMARY KEY(titleId, no));";
            }
            if (!imagesTableExists)
            {
                createTableCommand.CommandText += "CREATE TABLE 'images' ('titleId' TEXT, 'no' INTEGER, 'image_index' INTEGER, 'uri' TEXT, 'size' INTEGER, 'downloaded'	INTEGER, PRIMARY KEY(titleId, no , image_index));";
            }
            createTableCommand.CommandText += "COMMIT;";
            createTableCommand.ExecuteNonQuery();
        }

        public async Task DownloadWebtoonAsync(string titleId, string downloadFolder, CancellationToken ct)
        {
            var selectWebtoonCommand = sqliteConnection.CreateCommand();
            selectWebtoonCommand.CommandText= 
                $"SELECT * from webtoons " +
                $"WHERE titleId='{titleId}';";
            var webtoonReader = selectWebtoonCommand.ExecuteReader();
            WebtoonInfo webtoonInfo;
            if(!webtoonReader.Read())
            {
                webtoonInfo = await client.GetWebtoonInfoAsync(titleId);
                var insertWebtoonCommand = sqliteConnection.CreateCommand();
                insertWebtoonCommand.CommandText = 
                    $"INSERT into webtoons (titleId, title, writer) " +
                    $"VALUES (@titleId, @title, @writer);";
                insertWebtoonCommand.Parameters.AddWithValue("@titleId", titleId);
                insertWebtoonCommand.Parameters.AddWithValue("@title", webtoonInfo.Title);
                insertWebtoonCommand.Parameters.AddWithValue("@writer", webtoonInfo.Writer);
                insertWebtoonCommand.ExecuteNonQuery();
            }
            else
            {
                webtoonInfo = new WebtoonInfo()
                {
                    TitleId = (string)webtoonReader["titleId"],
                    Title = (string)webtoonReader["title"],
                    Writer = (string)webtoonReader["writer"]
                };
            }

            var selectEpisodesCommand = sqliteConnection.CreateCommand();
            selectEpisodesCommand.CommandText = 
                $"SELECT * FROM episodes " +
                $"WHERE titleId='{titleId}' " +
                $"ORDER BY no ASC;";
            var episodeReader = selectEpisodesCommand.ExecuteReader();
            var episodes = new Dictionary<int,EpisodeInfo>();
            while (episodeReader.Read())
            {
                var episodeInfo = new EpisodeInfo()
                {
                    TitleId=(string)episodeReader["titleId"],
                    No=(int)(long)episodeReader["no"],
                    Title=(string)episodeReader["title"],
                    Date=(string)episodeReader["date"]
                };
                episodes.Add(episodeInfo.No, episodeInfo);
            }
            int lastEpisodeNo;
            if (episodes.Count == 0)
            {
                lastEpisodeNo = 0;
            }
            else
            {
                lastEpisodeNo = episodes.Keys.Max();
            }
            var selectImagesCommand = sqliteConnection.CreateCommand();
            selectImagesCommand.CommandText = 
                $"SELECT * from images " +
                $"WHERE titleId='{titleId}' " +
                $"ORDER BY no,'image_index' ASC;";
            var imagesReader = selectImagesCommand.ExecuteReader();
            var imagesToDownload = new List<ImageInfo>();
            while (imagesReader.Read())
            {
                var image = new ImageInfo()
                {
                    TitleId = (string)imagesReader["titleId"],
                    No = (int)(long)imagesReader["no"],
                    Index = (int)(long)imagesReader["image_index"],
                    Size = (int)(long)imagesReader["size"],
                    Downloaded = (int)(long)imagesReader["downloaded"]
                };
                if (image.Downloaded == 0)
                {
                    imagesToDownload.Add(image);
                }
                episodes[(int)(long)imagesReader["no"]].Images.Add(image);
            }
            var latestEpisodeNo = await client.GetLatestEpisodeNoAsync(titleId);
            for(int i = lastEpisodeNo; i<=latestEpisodeNo; i++)
            {
                EpisodeInfo episodeInfo;
                try
                {
                   episodeInfo = await client.GetEpisodeInfoAsync(titleId, i);
                }
                catch(Exception e)
                {
                    if (e.GetType() == typeof(EpisodeNotFoundException))
                        continue;
                    else
                        throw e;
                }
                var insertEpisodeCommand = sqliteConnection.CreateCommand();
                insertEpisodeCommand.CommandText = 
                    $"INSERT into episodes (titleId, no, title , date, image_count) " +
                    $"VALUES ('{episodeInfo.TitleId}', {episodeInfo.No}, @title, @date, {episodeInfo.Images.Count});";
                insertEpisodeCommand.Parameters.AddWithValue("@title", episodeInfo.Title);
                insertEpisodeCommand.Parameters.AddWithValue("@date", episodeInfo.Title);
                episodeInfo.Images.ForEach((image) => 
                {
                    insertEpisodeCommand.CommandText +=
                    $"INSERT into images (titleId, no, 'image_index' , uri, size, downloaded) " +
                    $"VALUES ('{image.TitleId}', {image.No}, {image.Index}, @uri, {image.Size} ,0);";
                    insertEpisodeCommand.Parameters.AddWithValue("@uri", image.Uri);
                    imagesToDownload.Add(image);
                });
                insertEpisodeCommand.ExecuteNonQuery();
            }
            for(int i = 0; i < imagesToDownload.Count; i++)
            {
                var image = imagesToDownload[i];
                byte[] buff = await client.GetByteArrayAsync(image.Uri);
                var episodeDirectory = Path.Combine(
                    BuildWebtoonFolderName(webtoonInfo),
                    BuildEpisodeFolderName(webtoonInfo, episodes[image.No]));
                Directory.CreateDirectory(episodeDirectory);
                var imageFilePath = Path.Combine(
                    episodeDirectory,
                    BuildImageFileName(webtoonInfo, episodes[image.No], image, 
                    ".jpg"));
                File.WriteAllBytes(imageFilePath, buff);
                var updateImageCommand = sqliteConnection.CreateCommand();
                updateImageCommand.CommandText =
                    $"UPDATE images " +
                    $"SET size='{buff.Length}', downloaded='1' " +
                    $"WHERE titleId='{image.TitleId}' AND no='{image.No}' AND image_index='{image.Index}';";
                updateImageCommand.ExecuteNonQuery();
            }
        }
        private string BuildImageFileName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo,ImageInfo imageInfo, string extension)
        {
            return ReplaceFileName(string.Format(config.ImageFileNameFormat, episodeInfo.TitleId, episodeInfo.No, imageInfo.Index, webtoonInfo.Title, episodeInfo.Title, episodeInfo.Date) + extension);
        }
        private string BuildEpisodeFileName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo, string extension)
        {
            return ReplaceFileName(string.Format(config.EpisodeDirectoryNameFormat, webtoonInfo.TitleId, episodeInfo.No, episodeInfo.Date, webtoonInfo.Title, episodeInfo.Title, webtoonInfo.Writer)+extension);
        }
        private string BuildEpisodeFolderName(WebtoonInfo webtoonInfo, EpisodeInfo episodeInfo)
        {
            return ReplaceFolderName(string.Format(config.EpisodeDirectoryNameFormat, episodeInfo.TitleId, episodeInfo.No, episodeInfo.Date, webtoonInfo.Title, episodeInfo.Title, webtoonInfo.Writer));
        }
        private string BuildWebtoonFolderName(WebtoonInfo webtoonInfo)
        {
            return ReplaceFolderName(string.Format(config.WebtoonDirectoryNameFormat, webtoonInfo.TitleId, webtoonInfo.Title, webtoonInfo.Writer));
        }
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
