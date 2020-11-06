using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using wr_rainforest.Model;
using wr_rainforest.View;
using wr_rainforest.Naver_Webtoon_Downloader;

namespace wr_rainforest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Config config;
        string configFolderPath = "Config";
        string configFileName = "config.json";

        Thread thread;
        Thread mainThread;

        Downloader downloader;
        public MainWindow()
        {
            InitializeComponent();
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] assemsplit = assemblyVersion.Split('.');
            var version = $"{assemsplit[0]}.{assemsplit[1]}";
            var build = $"{assemsplit[2]}.{assemsplit[3]}";
            var Title = $"Naver-Webtoon-Downloader-GUI v{version} (Build {build})";
            this.Title = Title;
            Footer3.Content = $" Build {build}";
            Directory.CreateDirectory(configFolderPath);
            if (File.Exists(Path.Combine(configFolderPath, configFileName)))
            {
                config = new Config(File.ReadAllText(Path.Combine(configFolderPath, configFileName)));
            }
            else
            {
                config = new Config();
                File.WriteAllText(Path.Combine(configFolderPath, configFileName), config.ToJsonString());
            }
            config = new Config();
            downloader = new Downloader(config, "webtoon.db");
            mainThread = Thread.CurrentThread;
        }
        private void SetLoadingLabelVisibility(Visibility visibility)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => Loading.Visibility = visibility);
            }
            else
            {
                Loading.Visibility = visibility;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            Loading.Visibility = Visibility.Visible;
            string uriText = UriTextBox.Text.Trim();
            UriTextBox.Text = "";
            UriTextBox.Focus();
            if (string.IsNullOrEmpty(uriText))
            {
                MessageBox.Show("웹툰 링크를 입력해 주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) ;
                button.IsEnabled = true;
                SetLoadingLabelVisibility(Visibility.Hidden);
                return;
            }
            if (!uriText.Contains("comic.naver.com/"))
            {
                MessageBox.Show("웹툰 링크가 아닙니다.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                SetLoadingLabelVisibility(Visibility.Hidden);
                return;
            }
            if (!uriText.StartsWith("https://"))
                uriText = "https://" + uriText;
            Uri uri;
            try
            {
                uri = new Uri(uriText);
            }
            catch(Exception uriex)
            {
                MessageBox.Show($"URI 분석에 실패하였습니다.\r\n Error: {uriex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                SetLoadingLabelVisibility(Visibility.Hidden);
                return;
            }
            string titleId;
            titleId = HttpUtility.ParseQueryString(uri.Query).Get("titleId");
            if (string.IsNullOrEmpty(titleId))
            {
                MessageBox.Show("링크에서 웹툰 정보를 확인할 수 없습니다.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            var tuple = await downloader.CanDownload(titleId);
            if (!tuple.value)
            {
                MessageBox.Show($"다운로드 불가능한 웹툰입니다. ({tuple.message})","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                SetLoadingLabelVisibility(Visibility.Hidden);
                return;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                await downloader.UpdateOrCreateWebtoonDatabase(titleId, null, cts.Token);
                await downloader.DownloadWebtoonAsync(titleId, "download", null, cts.Token);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message+ex.StackTrace);
            }
            button.IsEnabled = true;
            SetLoadingLabelVisibility(Visibility.Hidden);
            return;
        }
        private void GitHub_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/wr-rainforest/Naver-Webtoon-Downloader-GUI");
        }
        private void Information_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InformationWindow informationWindow = new InformationWindow();
            informationWindow.ShowDialog();
        }
        private void UriTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddButton_Click(AddButton, new RoutedEventArgs(e.RoutedEvent));
            }
        }
    } 
}
