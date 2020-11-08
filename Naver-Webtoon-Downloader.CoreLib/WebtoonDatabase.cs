using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;

namespace NaverWebtoonDownloader.CoreLib
{
    internal class WebtoonDatabase
    {
        private SqliteConnection sqliteConnection;

        public WebtoonDatabase(string dataSource)
        {
            //dataSource를 절대경로로 변환합니다.
            dataSource = Path.GetFullPath(dataSource);
            sqliteConnection = new SqliteConnection($"Data Source = {dataSource};Mode=ReadWriteCreate;");
            sqliteConnection.Open();
            //master 테이블에서 존재하는 테이블 name 컬럼을 불러옵니다.
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
            //테이블이 존재하지 않을 경우 생성
            if (!webtoonsTableExists && !episodesTableExists && !imagesTableExists)
            {
                var createTableCommand = sqliteConnection.CreateCommand();
                createTableCommand.CommandText =
                    "BEGIN;" +
                    "CREATE TABLE 'webtoons' ('titleId' TEXT, 'title' TEXT, 'writer' TEXT, 'genre' TEXT, 'description' TEXT, PRIMARY KEY(titleId));" +
                    "CREATE TABLE 'episodes' ( 'titleId' TEXT, 'no' INTEGER, 'title' INTEGER, 'date' INTEGER, PRIMARY KEY(titleId, no));" +
                    "CREATE TABLE 'images' ('titleId' TEXT, 'no' INTEGER, 'image_index' INTEGER, 'uri' TEXT, 'size' INTEGER, 'downloaded' INTEGER, PRIMARY KEY(titleId, no , image_index));" +
                    "COMMIT";
                createTableCommand.ExecuteNonQuery();
            }
            else if(webtoonsTableExists && episodesTableExists && imagesTableExists)
            {

            }
            else
            {
                throw new Exception("알 수 없는 오류");
            }

        }

        public void InsertWebtoon(WebtoonInfo webtoonInfo)
        {
            var insertCommand = sqliteConnection.CreateCommand();
            insertCommand.CommandText =
                $"INSERT into webtoons (titleId, title, writer) " +
                $"VALUES (@titleId, @title, @writer);";
            insertCommand.Parameters.AddWithValue("@titleId", webtoonInfo.TitleId);
            insertCommand.Parameters.AddWithValue("@title", webtoonInfo.Title);
            insertCommand.Parameters.AddWithValue("@writer", webtoonInfo.Writer);
            insertCommand.ExecuteNonQuery();
        }

        public void InsertEpisode(EpisodeInfo episodeInfo)
        {
            var insertEpisodeCommand = sqliteConnection.CreateCommand();
            insertEpisodeCommand.CommandText =
                $"BEGIN;";
            insertEpisodeCommand.CommandText +=
                $"INSERT into episodes (titleId, no, title , date) " +
                $"VALUES ('{episodeInfo.TitleId}', {episodeInfo.No}, @title_{episodeInfo.No}, @date_{episodeInfo.No});";
            insertEpisodeCommand.Parameters.AddWithValue($"@title_{episodeInfo.No}", episodeInfo.Title);
            insertEpisodeCommand.Parameters.AddWithValue($"@date_{episodeInfo.No}", episodeInfo.Date);
            episodeInfo.Images.ToList().ForEach((image) =>
            {
                insertEpisodeCommand.CommandText +=
                $"INSERT into images (titleId, no, 'image_index' , uri, size, downloaded) " +
                $"VALUES ('{image.TitleId}', {image.No}, {image.Index}, @uri_{image.No}_{image.Index}, {image.Size} ,{image.Downloaded});";
                insertEpisodeCommand.Parameters.AddWithValue($"@uri_{image.No}_{image.Index}", image.Uri);
            });
            insertEpisodeCommand.CommandText +=
                "COMMIT;";
            insertEpisodeCommand.ExecuteNonQuery();
        }

        public WebtoonInfo SelectWebtoon(string titleId)
        {
            var selectCommand = sqliteConnection.CreateCommand();
            selectCommand.CommandText =
                $"SELECT titleId, title, writer from webtoons " +
                $"WHERE titleId='{titleId}';";
            var reader = selectCommand.ExecuteReader();
            if (!reader.Read())
                return null;
            var webtoonInfo = new WebtoonInfo()
            {
                TitleId = (string)reader["titleId"],
                Title = (string)reader["title"],
                Writer = (string)reader["writer"]
            };
            return webtoonInfo;
        }

        public EpisodeInfo[] SelectEpisodes(string titleId)
        {
            var selectEpisodesCommand = sqliteConnection.CreateCommand();
            selectEpisodesCommand.CommandText =
                $"SELECT titleId, no, title, date FROM episodes " +
                $"WHERE titleId='{titleId}' " +
                $"ORDER BY no ASC;";
            var episodeReader = selectEpisodesCommand.ExecuteReader();
            List<EpisodeInfo> episodes = new List<EpisodeInfo>();
            while (episodeReader.Read())
            {
                var episodeInfo = new EpisodeInfo()
                {
                    TitleId = (string)episodeReader["titleId"],
                    No = (int)(long)episodeReader["no"],
                    Title = (string)episodeReader["title"],
                    Date = (string)episodeReader["date"],
                };
            }
            var selectImagesCommand = sqliteConnection.CreateCommand();
            selectImagesCommand.CommandText =
                $"SELECT no, image_index, uri, size, downloaded FROM images " +
                $"WHERE titleId='{titleId}' " +
                $"ORDER BY no, image_index ASC;";
            var imageReader = selectImagesCommand.ExecuteReader();
            List<ImageInfo> images = new List<ImageInfo>();
            while (imageReader.Read())
            {
                var image = new ImageInfo()
                {
                    TitleId = titleId,
                    No = (int)(long)imageReader["no"],
                    Index = (int)(long)imageReader["image_index"],
                    Uri = (string)imageReader["uri"],
                    Downloaded = (int)(long)imageReader["downloaded"],
                    Size = (int)(long)imageReader["size"]
                };
                images.Add(image);
                episodes[image.No].Images = images.ToArray();
            }
            return episodes.ToArray();
        }

        public EpisodeInfo SelectLastEpisode(string titleId)
        {
            var selectEpisodeCommand = sqliteConnection.CreateCommand();
            selectEpisodeCommand.CommandText =
                $"SELECT titleId, no, title, date FROM episodes " +
                $"WHERE titleId='{titleId}' " +
                $"ORDER BY no DESC" +
                $"LIMIT 1;";
            var episodeReader = selectEpisodeCommand.ExecuteReader();
            EpisodeInfo episode;
            if (!episodeReader.Read())
            {
                return null;
            }
            else 
            {
                episode = new EpisodeInfo()
                {
                    TitleId = (string)episodeReader["titleId"],
                    No = (int)(long)episodeReader["no"],
                    Title = (string)episodeReader["title"],
                    Date = (string)episodeReader["date"],
                };
            }
            var selectImagesCommand = sqliteConnection.CreateCommand();
            selectImagesCommand.CommandText =
                $"SELECT image_index, uri, size, downloaded FROM images " +
                $"WHERE titleId='{titleId}' AND no='{episode.No}' " +
                $"ORDER BY image_index ASC;";
            var imageReader = selectImagesCommand.ExecuteReader();
            List<ImageInfo> images = new List<ImageInfo>();
            while(imageReader.Read())
            {
                var image = new ImageInfo()
                {
                    TitleId = titleId,
                    No = episode.No,
                    Index = (int)(long)imageReader["image_index"],
                    Uri = (string)imageReader["uri"],
                    Downloaded = (int)(long)imageReader["downloaded"],
                    Size = (int)(long)imageReader["size"]
                };
                images.Add(image);
            }
            episode.Images = images.ToArray();
            return episode;
        }

        public void UpdateImage(ImageInfo image)
        {
            var updateImageCommand = sqliteConnection.CreateCommand();
            updateImageCommand.CommandText =
                $"UPDATE images " +
                $"SET size='{image.Size}', downloaded='{image.Downloaded}' " +
                $"WHERE titleId='{image.TitleId}' AND no='{image.No}' AND image_index='{image.Index}';";
            updateImageCommand.ExecuteNonQuery();
        }

        public void DeleteWebtoon(string titleId)
        {
            var deleteCommand = sqliteConnection.CreateCommand();
            deleteCommand.CommandText =
                $"DELETE FROM webtoons WHERE titleId='{titleId}';" +
                $"DELETE FROM episodes WHERE titleId='{titleId}';" +
                $"DELETE FROM images WHERE titleId='{titleId}';";
        }

        public void DeleteEpisodes(string titleId)
        {
            var deleteCommand = sqliteConnection.CreateCommand();
            deleteCommand.CommandText =
                $"DELETE FROM episodes WHERE titleId='{titleId}';" +
                $"DELETE FROM images WHERE titleId='{titleId}';";
        }

    }
}
