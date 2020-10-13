using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using WRforest.Model;
using WRforest.NWD;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;
using WRforest.Progress;
using WRforest.View;

namespace WRforest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Config config;
        Dictionary<string, string> unAvailTitleIds = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
            config = new Config();
            unAvailTitleIds.Add("670144", "애니매이션 효과가 적용된 웹툰은 다운로드가 불가능합니다.");
            Load("Cache");
        }

        private async void Load(string cacheFolderPath)
        {
            Loading.Visibility = Visibility.Visible;
            string[] cacheFiles = Directory.GetFiles(cacheFolderPath, "*.json");
            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            for (int i = 0; i < cacheFiles.Length; i++)
            {
                await Task.Run(()=>Thread.Sleep(100));
                try
                {

                    string json = File.ReadAllText(cacheFiles[i]);
                    WebtoonInfo webtoonInfo = JsonConvert.DeserializeObject<WebtoonInfo>(json);
                    if (new FileInfo(cacheFiles[i]).Name != webtoonInfo.WebtoonTitleId + ".json")
                    {
                        File.Delete(cacheFiles[i]);
                        continue;
                    }
                    Downloader downloader = new Downloader(webtoonInfo, config);
                    var tuple = downloader.GetDownloadedImagesInformation();
                    string title = webtoonInfo.WebtoonTitle;
                    string lastEpisodeInfo = $"[{webtoonInfo.Episodes[webtoonInfo.Episodes.Keys.Max()].EpisodeDate}] {webtoonInfo.Episodes[webtoonInfo.Episodes.Keys.Max()].EpisodeTitle}";
                    string writer = webtoonInfo.WebtoonWriter;
                    string status = "";
                    double progress = (double)tuple.downloadedImageCount / webtoonInfo.GetImageCount();
                    string progressText = $"{progress:P}";
                    string size = $"{(double)tuple.downloadedImagesSize/1048576:0:00} MB";
                    WebtoonDownloadInfo downloadInfo = new WebtoonDownloadInfo(title, writer, lastEpisodeInfo, status, progress, progressText, size);
                    downloadInfo.TitleId = webtoonInfo.WebtoonTitleId;
                    downloadInfo.CancellationTokenSource = new CancellationTokenSource();
                    WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
                    bool isGray = false;
                    if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
                    {
                        isGray = true;
                    }
                    WebtoonDownloadPanelItem panelItem = new WebtoonDownloadPanelItem
    (downloadInfo, isGray, RowInfoGrid.ColumnDefinitions, RunButton_Click, PauseButton_Click, StopButton_Click, DeleteButton_Click);
                    WebtoonDownloadPanel.Children.Add(panelItem);
                    await agent.LoadPageAsync(webtoonKey.BuildUrl());
                    int latest = int.Parse(parser.GetLatestEpisodeNo());
                    int last = webtoonInfo.GetLastEpisodeNo();
                    if (latest > last)
                    {
                        downloadInfo.Status = "URL캐시 업데이트를 시작합니다.";
                        WebtoonUpdateProgress updateProgress = new WebtoonUpdateProgress(downloadInfo);
                        await downloader.UpdateWebtoonInfoAsync(updateProgress, downloadInfo.CancellationTokenSource.Token);
                    }
                }
                catch
                {
                    File.Delete(cacheFiles[i]);
                }
            }
            Loading.Visibility = Visibility.Hidden;
        }
        private List<Task> tasks = new List<Task>();
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //유효성 체크 메서드로 분리
            Button button = sender as Button;
            button.IsEnabled = false;
            string uriText = UriTextBox.Text.Trim();
            UriTextBox.Text = "";
            if (!uriText.Contains("comic.naver.com/webtoon/"))
            {
                MessageBox.Show("잘못된 uri입니다.");
                button.IsEnabled = true;
                return;
            }
            if (!uriText.StartsWith("http"))
            {
                uriText = "https://" + uriText;
            }
            Uri uri = new Uri(uriText);
            string titleId;
            if (!HttpUtility.ParseQueryString(uri.Query).AllKeys.Contains("titleId"))
            {
                MessageBox.Show("잘못된 uri입니다. (웹툰정보 확인 불가능)");
                button.IsEnabled = true;
                return;
            }
            titleId = HttpUtility.ParseQueryString(uri.Query).Get("titleId");

            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            await agent.LoadPageAsync($"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}");
            var cururl = parser.GetUrl();
            if (agent.Page.DocumentNode.InnerHtml.Contains("완결까지 정주행!"))
            {
                MessageBox.Show("다운로드 불가능한 웹툰입니다. (유료 웹툰)", "Error");
                button.IsEnabled = true;
                return;
            }
            if (agent.Page.DocumentNode.InnerHtml.Contains("18세 이상 이용 가능"))
            {
                MessageBox.Show("다운로드 불가능한 웹툰입니다. (성인 웹툰)", "Error");
                button.IsEnabled = true;
                return;
            }
            if (cururl.Contains("hallenge"))
            {
                MessageBox.Show("다운로드 불가능한 웹툰입니다. (베스트도전/도전만화)", "Error");
                button.IsEnabled = true;
                return;
            }
            if (unAvailTitleIds.ContainsKey(titleId))
            {
                MessageBox.Show($"{unAvailTitleIds[titleId]}", "Error");
                button.IsEnabled = true;
                return;
            }
            for(int i = 0; i < WebtoonDownloadPanel.Children.Count; i++)
            {
                var item = WebtoonDownloadPanel.Children[i] as WebtoonDownloadPanelItem;
                var info = item.DataContext as WebtoonDownloadInfo;
                if(info.TitleId == titleId)
                {
                    MessageBox.Show("이미 추가된 웹툰입니다.", "Error");
                    button.IsEnabled = true;
                    return;
                }
            }

            string title = parser.GetWebtoonTitle();
            string lastEpisodeInfo = "";
            await agent.LoadPageAsync($"https://comic.naver.com/webtoon/detail.nhn?titleId={titleId}&no=1");
            string writer = parser.GetWebtoonWriter();
            string status = "";
            double progress = 0d;
            string progressText = "";
            string size = "0.00 MB";

            WebtoonDownloadInfo downloadInfo = new WebtoonDownloadInfo(title, writer, lastEpisodeInfo, status, progress, progressText, size);
            downloadInfo.TitleId = titleId;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            
            bool isGray = false;
            if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
            {
                isGray = true;
            }
            WebtoonDownloadPanelItem panelItem = new WebtoonDownloadPanelItem
                (downloadInfo, isGray, RowInfoGrid.ColumnDefinitions, RunButton_Click, PauseButton_Click, StopButton_Click, DeleteButton_Click);
            WebtoonUpdateProgress webtoonDownloadProgress = new WebtoonUpdateProgress(downloadInfo);
            WebtoonInfo webtoonInfo = new WebtoonInfo(new NWD.Key.WebtoonKey(titleId), title);
            Downloader downloader = new Downloader(webtoonInfo, config);
            WebtoonDownloadPanel.Children.Add(panelItem);
            button.IsEnabled = true;
            Task task = new Task(async() => await downloader.UpdateWebtoonInfoAsync(webtoonDownloadProgress, cancellationTokenSource.Token));
            downloadInfo.CancellationTokenSource = cancellationTokenSource;
            int taskCount = tasks.Count;
            tasks.Add(task);
            while (true)
            {
                downloadInfo.Status = $"작업 대기중...({taskCount})";
                if (tasks[0] == task)
                    break;
                else
                {
                    //대기중 작업 취소
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        tasks.Remove(task);
                        return;
                    }
                    await tasks[0];
                }
                taskCount--;
            }
            downloadInfo.Status = "URL 캐시 생성중...";
            task.Start();
            await task.ContinueWith((t) => tasks.Remove(t));
            //작업중 작업 취소
            if(cancellationTokenSource.IsCancellationRequested)
            {
                downloadInfo.Status = "작업이 취소되었습니다.";
                return;
            }
            else
            {
                downloadInfo.Status = "URL 캐시 생성 완료...";
            }
            IO.WriteTextFile("Cache", titleId + ".json", JsonConvert.SerializeObject(webtoonInfo));
        }//private async void AddButton_Click(object sender, RoutedEventArgs e)
        
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is WebtoonDownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            WebtoonDownloadPanelItem item = (WebtoonDownloadPanelItem)dependencyObject;
            item.SetRunPauseButtonMode(RunPauseButtonMode.Pause);
            item.SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);
            WebtoonDownloadInfo webtoonDownloadInfo = item.DataContext as WebtoonDownloadInfo;

        }//private async void RunButton_Click(object sender, RoutedEventArgs e)

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is WebtoonDownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            WebtoonDownloadPanelItem item = (WebtoonDownloadPanelItem)dependencyObject;
            item.SetRunPauseButtonMode(RunPauseButtonMode.Pause);
            item.SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);
            WebtoonDownloadInfo webtoonDownloadInfo = item.DataContext as WebtoonDownloadInfo;
        }
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is WebtoonDownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            WebtoonDownloadPanelItem item = (WebtoonDownloadPanelItem)dependencyObject;
            item.SetRunPauseButtonMode(RunPauseButtonMode.Run);
            WebtoonDownloadInfo webtoonDownloadInfo = item.DataContext as WebtoonDownloadInfo;
            webtoonDownloadInfo.CancellationTokenSource.Cancel();
            item.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is WebtoonDownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            WebtoonDownloadPanelItem item = (WebtoonDownloadPanelItem)dependencyObject;
            WebtoonDownloadInfo webtoonDownloadInfo = item.DataContext as WebtoonDownloadInfo;
            File.Delete($"Cache/{webtoonDownloadInfo.TitleId}.json");
            WebtoonDownloadPanel.Children.Remove(item);
        }

    }
 
}
