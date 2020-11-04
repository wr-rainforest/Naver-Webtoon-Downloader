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
using System.Windows.Shapes;
using System.Xml;
using wr_rainforest.Model;
using wr_rainforest.NWD;
using wr_rainforest.NWD.DataType;
using wr_rainforest.NWD.Key;
using wr_rainforest.Progress;
using wr_rainforest.View;

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
        string cacheFolderPath = "Cache";
        Dictionary<string, string> unAvailTitleIds = new Dictionary<string, string>();
        Thread thread;
        Thread mainThread;
        Thread loadThread;
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

            if (IO.Exists(configFolderPath, configFileName))
            {
                config = new Config(IO.ReadTextFile(configFolderPath, configFileName));
            }
            else
            {
                config = new Config();
                IO.WriteTextFile(configFolderPath, configFileName, config.ToJsonString());
            }
            if (!Directory.Exists(cacheFolderPath))
            {
                Directory.CreateDirectory(cacheFolderPath);
            }
            InitializeComponent();
            config = new Config();
            unAvailTitleIds.Add("670144", "애니매이션 효과가 적용된 웹툰은 다운로드가 불가능합니다.");

            thread = new Thread(()=>TaskManager());
            thread.Start();
            mainThread = Thread.CurrentThread;
            loadThread =new Thread (()=>Load());
            loadThread.Start();
        }
        private void SetLoadStatus(Visibility visibility)
        {
            if(Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => Loading.Visibility = visibility);
            }
            else
            {
                Loading.Visibility = visibility;
            }
        }
        private async void Load()
        {
            Task cacheTask = LoadCache();
            UpdateCheck();
            await cacheTask;
        }
        private void UpdateCheck()
        {
            WebClient webClient = new WebClient();
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                var xmlText = webClient.DownloadString("https://wr-rainforest.github.io/Naver-Webtoon-Downloader-GUI/Pages/version.info.xml");
                xmlDocument.LoadXml(xmlText);
                Version latestVersion = new Version(xmlDocument.SelectSingleNode("/Document/Version[@latest=\"true\"]").Attributes["version"].Value);
                Version currentVersion = new Version(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                int compareResult = currentVersion.CompareTo(latestVersion);
                if (compareResult < 0)
                {
                    var versionInfo = xmlDocument.SelectSingleNode("/Document/Version[@Latest=\"true\"]").Attributes["info"].Value;
                    MessageBox.Show($"새로운 버전이 출시되었습니다. v{latestVersion.Major}.{latestVersion.Minor} (빌드 {latestVersion.Build}.{latestVersion.Revision})\r\n" +
                        $"\r\nhttps://github.com/wr-rainforest/Naver-Webtoon-Downloader-GUI","업데이트 확인");
                    Dispatcher.Invoke(() =>
                    {
                        Footer1.Content = $"새로운 버전이 출시되었습니다. v{latestVersion.Major}.{latestVersion.Minor} (빌드 {latestVersion.Build}.{latestVersion.Revision})";
                    });
                }
                else if (compareResult == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Footer1.Content = " 최신 버전입니다.";
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Footer1.Content = " 개발 버전입니다.";
                    });
                }
            }
            catch(Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    Footer1.Content = " 버전 정보를 불러오지 못하였습니다.";
                });
                MessageBox.Show("버전 정보를 불러오지 못하였습니다.\r\nError : "
                    +e.Message, "Error");
                return;
            }


        }
        private async Task LoadCache()
        {
            SetLoadStatus(Visibility.Visible);
            string[] cacheFiles = Directory.GetFiles(cacheFolderPath, "*.json");
            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            for (int i = 0; i < cacheFiles.Length; i++)
            {
                string json;
                WebtoonInfo webtoonInfo;
                try
                {
                    json = File.ReadAllText(cacheFiles[i]);
                    webtoonInfo = JsonConvert.DeserializeObject<WebtoonInfo>(json);
                }
                catch
                {
                    MessageBox.Show("캐시파일 로딩에 실패하였습니다. 캐시파일이 손상되었거나, 파일에 접근할 수 없습니다.");
                    continue;
                }
                if (new FileInfo(cacheFiles[i]).Name != webtoonInfo.WebtoonTitleId + ".json")
                {
                    continue;
                }
                Downloader downloader = new Downloader(webtoonInfo, config);
                WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
                int last = webtoonInfo.GetLastEpisodeNo();
                await agent.LoadPageAsync(webtoonKey.BuildUrl());
                int latest = int.Parse(parser.GetLatestEpisodeNo());
                var tuple = downloader.GetDownloadedImagesInformation();
                string status;
                if (tuple.downloadedImageCount == webtoonInfo.GetImageCount())
                {
                    status = "다운로드 완료";
                }
                else
                {
                    status = "로딩 완료";
                }
                DownloadInfo downloadInfo = new DownloadInfo(
                    webtoonInfo,
                    $"[{webtoonInfo.Episodes[last].EpisodeDate}] {webtoonInfo.Episodes[last].EpisodeTitle}",
                    status,
                    (double)tuple.downloadedImageCount / webtoonInfo.GetImageCount(),
                    $"{(double)tuple.downloadedImageCount / webtoonInfo.GetImageCount():P} ({tuple.downloadedImageCount} / {webtoonInfo.GetImageCount()})",
                    $"{(double)tuple.downloadedImagesSize / 1048576:0.00} MB");
                bool isGray = false;
                DownloadPanelItem downloadPanelItem = null;
                Dispatcher.Invoke(() =>
                {
                    if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
                    {
                        isGray = true;
                    }
                    downloadPanelItem = new DownloadPanelItem(downloadInfo, isGray, RowInfoGrid.ColumnDefinitions, RunButton_Click, PauseButton_Click, StopButton_Click, DeleteButton_Click);
                });
                downloadPanelItem.SetRunPauseButtonMode(RunPauseButtonMode.Run);
                downloadPanelItem.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
                Dispatcher.Invoke(() =>
                {
                    WebtoonDownloadPanel.Children.Add(downloadPanelItem);
                });
                if(latest>last)
                {
                    downloadInfo.CancellationTokenSource = new CancellationTokenSource();//대기작업 취소 토큰
                    downloadPanelItem.SetRunPauseButtonMode(RunPauseButtonMode.RunDisable);
                    downloadPanelItem.SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);
                    Task task = new Task(async () =>
                    {
                        var IsCancellationRequested = await CheckUpdateAsync(downloadInfo);
                        downloadInfo.CancellationTokenSource = null;
                        downloadPanelItem.SetRunPauseButtonMode(RunPauseButtonMode.Run);
                        downloadPanelItem.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
                        downloadInfo.Progress = (double)tuple.downloadedImageCount / webtoonInfo.GetImageCount();
                        downloadInfo.ProgressText = $"{(double)tuple.downloadedImageCount / webtoonInfo.GetImageCount():P} ({tuple.downloadedImageCount} / {webtoonInfo.GetImageCount()})";
                    });
                    taskList.Add((task, downloadInfo));
                }
            }
            SetLoadStatus(Visibility.Hidden);
        }

        private List<(Task task, DownloadInfo info)> taskList = new List<(Task task, DownloadInfo info)>();
        private void TaskManager()
        {
            while (true)
            {
                if (!mainThread.IsAlive)
                {
                    thread.Abort();
                }
                if (taskList.Count==0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                RefreshTaskList();
            }
        }
        //메서드 수정
        private async void RefreshTaskList()
        {
            Task currentTask = taskList[0].task;
            currentTask.Start();
            while (!currentTask.IsCompleted)
            {
                if (!mainThread.IsAlive)
                {
                    thread.Abort();
                }
                for (int j = 1; j < taskList.Count; j++)
                {
                    taskList[j].info.Status = $"작업 대기중...({j})";
                }
                for (int i = 1; i < taskList.Count; i++)
                {
                    if (taskList[i].info.CancellationTokenSource?.IsCancellationRequested ?? false)
                    {
                        taskList[i].task.Start();
                        await taskList[i].task;//
                        taskList.RemoveAt(i);
                        break;
                    }
                }
            }
            taskList.RemoveAt(0);
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            Loading.Visibility = Visibility.Visible;

            string uriText = UriTextBox.Text.Trim();
            UriTextBox.Text = "";
            UriTextBox.Focus();
            if (!uriText.StartsWith("http"))
            {
                uriText = "https://" + uriText;
            }
            var result = CheckUserInput(uriText);
            if (result != null)
            {
                MessageBox.Show(result,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                button.IsEnabled = true;
                Loading.Visibility = Visibility.Hidden;
                return;
            }

            Uri uri = new Uri(uriText);
            string titleId;
            titleId = HttpUtility.ParseQueryString(uri.Query).Get("titleId");
            var result1 = await CheckWebtoonTitleId(titleId);
            if (result1 != null)
            {
                MessageBox.Show(result1);
                button.IsEnabled = true;
                Loading.Visibility = Visibility.Hidden;
                return;
            }
            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            await agent.LoadPageAsync($"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}");
            string title = parser.GetWebtoonTitle();
            string lastEpisodeInfo = "";
            await agent.LoadPageAsync($"https://comic.naver.com/webtoon/detail.nhn?titleId={titleId}&no=1");
            string writer = parser.GetWebtoonWriter();
            string status = "준비중";
            double progress = 0d;
            string progressText = "0%";
            string size = "0.00 MB";
            WebtoonInfo webtoonInfo = new WebtoonInfo(new WebtoonKey(titleId), title);
            webtoonInfo.WebtoonWriter = writer;
            DownloadInfo downloadInfo = new DownloadInfo(webtoonInfo, lastEpisodeInfo, status, progress, progressText, size);
            bool isGray = false;
            if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
            {
                isGray = true;
            }
            DownloadPanelItem downloadPanelItem = new DownloadPanelItem(downloadInfo, isGray, RowInfoGrid.ColumnDefinitions, RunButton_Click, PauseButton_Click, StopButton_Click, DeleteButton_Click);
            downloadPanelItem.SetRunPauseButtonMode(RunPauseButtonMode.RunDisable);
            downloadPanelItem.SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);
            WebtoonDownloadPanel.Children.Add(downloadPanelItem);
            button.IsEnabled = true;
            Loading.Visibility = Visibility.Hidden;
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();//대기작업 취소 토큰
            Task task = new Task(async () =>
            {
                var IsCancellationRequested = await RunAsync(downloadInfo);
                downloadInfo.CancellationTokenSource = null;
            });
            taskList.Add((task, downloadInfo));
            await task.ContinueWith((t) =>
            {
                downloadPanelItem.SetRunPauseButtonMode(RunPauseButtonMode.Run);
                downloadPanelItem.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
            });
        }

        private async Task<string> CheckWebtoonTitleId(string titleId)
        {
            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            await agent.LoadPageAsync($"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}");
            var cururl = parser.GetUrl();
            if (agent.Page.DocumentNode.InnerHtml.Contains("완결까지 정주행!"))
            {
                return "다운로드 불가능한 웹툰입니다. (유료 웹툰)";
            }
            if (agent.Page.DocumentNode.InnerHtml.Contains("18세 이상 이용 가능"))
            {
                return "다운로드 불가능한 웹툰입니다. (성인 웹툰)";
            }
            if (cururl.Contains("hallenge"))
            {
                return "다운로드 불가능한 웹툰입니다. (베스트도전/도전만화)";
            }
            if (unAvailTitleIds.ContainsKey(titleId))
            {
                return $"{unAvailTitleIds[titleId]}";
            }
            for (int i = 0; i < WebtoonDownloadPanel.Children.Count; i++)
            {
                var item = WebtoonDownloadPanel.Children[i] as DownloadPanelItem;
                var info = item.DataContext as DownloadInfo;
                if (info.TitleId == titleId)
                {
                    return "이미 추가된 웹툰입니다.";
                }
            }
            return null;
        }

        private string CheckUserInput(string uriText)
        {
            if (uriText == "https://")
            {
                return "uri를 입력해주세요.";
            }
            Uri uri = new Uri(uriText);
            string titleId;
            if (!uriText.Contains("comic.naver.com/webtoon/"))
            {
                return "잘못된 uri입니다.";
            }
            if (!HttpUtility.ParseQueryString(uri.Query).AllKeys.Contains("titleId"))
            {
                return "잘못된 uri입니다. (웹툰정보 확인 불가능)";
            }
            titleId = HttpUtility.ParseQueryString(uri.Query).Get("titleId");
            return null;
        }
        
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is DownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            DownloadPanelItem item = (DownloadPanelItem)dependencyObject;
            item.SetRunPauseButtonMode(RunPauseButtonMode.RunDisable);
            item.SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);
            DownloadInfo downloadInfo = item.DataContext as DownloadInfo;
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();//대기작업 취소 토큰
            Task task = new Task(async () =>
            {
                var IsCancellationRequested = await RunAsync(downloadInfo);
            });
            taskList.Add((task, downloadInfo));
            await task.ContinueWith((t) =>
            {
                item.SetRunPauseButtonMode(RunPauseButtonMode.Run);
                item.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
            });
        }//private async void RunButton_Click(object sender, RoutedEventArgs e)

        private async Task<bool> RunAsync(DownloadInfo downloadInfo)
        {
            WebtoonInfo webtoonInfo = downloadInfo.WebtoonInfo;
            Downloader downloader = new Downloader(webtoonInfo, config);
            UpdateProgress updateProgress = new UpdateProgress(downloadInfo);
            DownloadProgress downloadProgress = new DownloadProgress(downloadInfo);

            downloadInfo.Status = "URL 캐시 업데이트중...";
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();
            await downloader.UpdateWebtoonInfoAsync(updateProgress, downloadInfo.CancellationTokenSource.Token);
            if (downloadInfo.CancellationTokenSource.IsCancellationRequested)
            {
                downloadInfo.Status = "작업이 취소되었습니다.";
                downloadInfo.CancellationTokenSource = null;
                IO.WriteTextFile("Cache", webtoonInfo.WebtoonTitleId + ".json", JsonConvert.SerializeObject(webtoonInfo));
                return true;
            }
            else
            {
                downloadInfo.Status = "URL 캐시 업데이트 완료...";
                IO.WriteTextFile("Cache", webtoonInfo.WebtoonTitleId + ".json", JsonConvert.SerializeObject(webtoonInfo));
            }

            downloadInfo.Status = "다운로드중...";
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();
            ImageKey[] imageKeys = downloader.BuildImageKeysToDown();
            await downloader.DownloadAsync(imageKeys, downloadProgress, downloadInfo.CancellationTokenSource.Token);
            if (downloadInfo.CancellationTokenSource.IsCancellationRequested)
            {
                downloadInfo.Status = "작업이 취소되었습니다.";
                downloadInfo.CancellationTokenSource = null;
                return true;
            }
            else
            {
                downloadInfo.Status = "다운로드 완료...";
            }
            downloadInfo.CancellationTokenSource = null;
            return false;
        }

        private async Task<bool> CheckUpdateAsync(DownloadInfo downloadInfo)
        {
            WebtoonInfo webtoonInfo = downloadInfo.WebtoonInfo;
            Downloader downloader = new Downloader(webtoonInfo, config);
            UpdateProgress updateProgress = new UpdateProgress(downloadInfo);
            Agent agent = new Agent();
            Parser parser = new Parser(agent);
            WebtoonKey webtoonKey = new WebtoonKey(webtoonInfo.WebtoonTitleId);
            downloadInfo.Status = "URL 캐시 업데이트중...";
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();
            await downloader.UpdateWebtoonInfoAsync(updateProgress, downloadInfo.CancellationTokenSource.Token);
            if (downloadInfo.CancellationTokenSource.IsCancellationRequested)
            {
                downloadInfo.Status = "작업이 취소되었습니다.";
                downloadInfo.CancellationTokenSource = null;
                return true;
            }
            else
            {
                downloadInfo.Status = "URL 캐시 업데이트 완료...";
                IO.WriteTextFile("Cache", webtoonInfo.WebtoonTitleId + ".json", JsonConvert.SerializeObject(webtoonInfo));
            }
            return false;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("throw;");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is DownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            DownloadPanelItem item = (DownloadPanelItem)dependencyObject;
            DownloadInfo webtoonDownloadInfo = item.DataContext as DownloadInfo;
            webtoonDownloadInfo.Status = "취소된 작업";
            webtoonDownloadInfo.CancellationTokenSource?.Cancel();
            item.SetRunPauseButtonMode(RunPauseButtonMode.Run);
            item.SetStopDeleteButtonMode(StopDeleteButtonMode.Delete);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
            while (!(dependencyObject is DownloadPanelItem))
            {
                try
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                catch
                {
                }
            }
            DownloadPanelItem item = (DownloadPanelItem)dependencyObject;
            DownloadInfo webtoonDownloadInfo = item.DataContext as DownloadInfo;
            var res = MessageBox.Show($"\"{webtoonDownloadInfo.Title}\"을/를 목록에서 삭제할까요?","삭제",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.Yes);
            if (res==MessageBoxResult.No)
            {
                return;
            }
            else if (res == MessageBoxResult.Yes)
            {

            }
            else
            {
                throw new Exception("");
            }
            File.Delete($"Cache/{webtoonDownloadInfo.TitleId}.json");
            WebtoonDownloadPanel.Children.Remove(item);
            for(int i = 0; i < WebtoonDownloadPanel.Children.Count; i++)
            {
                var pitem = WebtoonDownloadPanel.Children[i] as DownloadPanelItem;
                if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
                {
                    pitem.Background = new SolidColorBrush(Color.FromRgb(0xf0, 0xf0, 0xf0));
                }
                else
                {
                    pitem.Background = Brushes.White;
                }
                
            }
        }

        private void UriTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddButton_Click(AddButton, new RoutedEventArgs(e.RoutedEvent));
            }
        }

        private void GitHub_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/wr-rainforest/Naver-Webtoon-Downloader-GUI");
        }

        private void Merge_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenDownloadFoleder_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(config.DefaultDownloadDirectory))
                Directory.CreateDirectory(config.DefaultDownloadDirectory);
            Process.Start(config.DefaultDownloadDirectory);
        }

        private void SetFolder_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;
            if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var path = commonOpenFileDialog.FileName;
                config.DefaultDownloadDirectory = path;
                File.WriteAllText($"Config/config.json", config.ToJsonString());
            }
        }

        private void Information_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InformationWindow informationWindow = new InformationWindow();
            informationWindow.ShowDialog();
        }

    }
 
}
