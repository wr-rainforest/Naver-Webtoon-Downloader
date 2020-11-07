using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Microsoft.Data.Sqlite;
namespace NaverWebtoonDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirectoryInfo appDataFolder = Directory.CreateDirectory("AppData");
        private Dictionary<string, string> configs = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
            var appDbDataSource = Path.GetFullPath("appdata.db", appDataFolder.FullName);
            var appDbConnection = new SqliteConnection($"Data Source = {appDbDataSource};Mode=ReadWriteCreate;");
            appDbConnection.Open();
            var selectTableNameCommand = appDbConnection.CreateCommand();
            selectTableNameCommand.CommandText = "SELECT name from sqlite_master where type='table';";
            var tableNameReader = selectTableNameCommand.ExecuteReader();
            bool configTableExists = false;
            while (tableNameReader.Read())
            {
                string tablename = (string)tableNameReader[0];
                if (tablename == "config")
                    configTableExists = true;
            }
            if (!configTableExists)
            {
                var createTableCommand = appDbConnection.CreateCommand();
                createTableCommand.CommandText =
                    $"BEGIN;" +
                    $"CREATE TABLE 'configs' ('key', 'value', PRIMARY KEY(key));" +
                    $"INSERT INTO configs ('key','value') VALUES ('WebtoonFolderNameFormat', '{{1}}');" +
                    $"INSERT INTO configs ('key','value') VALUES ('EpisodeFolderNameFormat', '[{{2}}] {{4}}');" +
                    $"INSERT INTO configs ('key','value') VALUES ('ImageFileNameFormat', '[{{5}}] {{3}} - {{4}}({{2:D3}})');" +
                    $"COMMIT;";
                createTableCommand.ExecuteNonQuery();
            }
            var selectConfigsCommand = appDbConnection.CreateCommand();
            selectConfigsCommand.CommandText = "SELECT key, value FROM configs;";
            var configReader = selectConfigsCommand.ExecuteReader();
            while (configReader.Read())
            {
                string key = (string)configReader["key"];
                string value = (string)configReader["value"];
                configs.Add(key, value);
            }
        }

        private void OpenDownloadFoleder_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Merge_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SetFolder_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Information_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void GitHub_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UriTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
