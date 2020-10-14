using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        Thread thread;
        Thread mainThread;
        public MainWindow()
        {
            InitializeComponent();
            config = new Config();
            unAvailTitleIds.Add("670144", "애니매이션 효과가 적용된 웹툰은 다운로드가 불가능합니다.");

            thread = new Thread(TaskManager);
            thread.Start();
            mainThread = Thread.CurrentThread;
        }

        private List<(Task task, WebtoonDownloadInfo info)> taskList = new List<(Task task, WebtoonDownloadInfo info)>();
        private async void TaskManager()
        {
            while (true)
            {
                if(taskList.Count==0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    taskList[0].task.Start();
                    await RefreshTaskList(taskList[0].task);
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, "Exception");
                }
            }
        }
        private async Task RefreshTaskList(Task currentTask)
        {
            while (!currentTask.IsCompleted)
            {
                if (!mainThread.IsAlive)
                {
                    thread.Abort();
                }
                for (int i = 1; i < taskList.Count; i++)
                {
                    if (taskList[i].info.CancellationTokenSource.IsCancellationRequested)
                    {
                        taskList.RemoveAt(i);
                        for (int j = 1; j < taskList.Count; j++)
                        {
                            taskList[i].info.Status = $"작업 대기중...({i})";
                        }
                    }
                }
            }
            taskList.RemoveAt(0);
        }
        private List<Task> tasks = new List<Task>();
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            Loading.Visibility = Visibility.Visible;

            string uriText = UriTextBox.Text.Trim();
            UriTextBox.Text = "";
            if (!uriText.StartsWith("http"))
            {
                uriText = "https://" + uriText;
            }
            var result = await CheckUserInput(uriText);
            if (result != null)
            {
                MessageBox.Show(result);
                button.IsEnabled = true;
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

            WebtoonDownloadInfo downloadInfo = new WebtoonDownloadInfo(title, titleId, writer, lastEpisodeInfo, status, progress, progressText, size);
            bool isGray = false;
            if ((WebtoonDownloadPanel.Children.Count + 1) % 2 == 0)
            {
                isGray = true;
            }
            WebtoonDownloadPanelItem panelItem = new WebtoonDownloadPanelItem(downloadInfo, isGray, RowInfoGrid.ColumnDefinitions, RunButton_Click, PauseButton_Click, StopButton_Click, DeleteButton_Click);
            WebtoonDownloadPanel.Children.Add(panelItem);
            button.IsEnabled = true;
            Loading.Visibility = Visibility.Hidden;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            downloadInfo.CancellationTokenSource = cancellationTokenSource;
            Task task = new Task(
                async() => {
                    WebtoonInfo webtoonInfo = new WebtoonInfo(new WebtoonKey(titleId), title);
                    webtoonInfo.WebtoonWriter = writer;
                    Downloader downloader = new Downloader(webtoonInfo, config);

                    downloadInfo.Status = "URL 캐시 생성중...";
                    WebtoonUpdateProgress updateProgress = new WebtoonUpdateProgress(downloadInfo);
                    await downloader.UpdateWebtoonInfoAsync(updateProgress, downloadInfo.CancellationTokenSource.Token);
                    if (downloadInfo.CancellationTokenSource.IsCancellationRequested)
                    {
                        downloadInfo.Status = "작업이 취소되었습니다.";
                        return;
                    }
                    else
                    {
                        downloadInfo.Status = "URL 캐시 생성 완료...";
                        IO.WriteTextFile("Cache", titleId + ".json", JsonConvert.SerializeObject(webtoonInfo));
                    }

                    ImageKey[] imageKeys = downloader.BuildImageKeysToDown();
                    downloadInfo.Status = "다운로드중...";
                    WebtoonDownloadProgress downloadProgress = new WebtoonDownloadProgress(downloadInfo);
                    downloadInfo.CancellationTokenSource = new CancellationTokenSource();
                    await downloader.DownloadAsync(imageKeys, downloadProgress, downloadInfo.CancellationTokenSource.Token);
                    if (downloadInfo.CancellationTokenSource.IsCancellationRequested)
                    {
                        downloadInfo.Status = "작업이 취소되었습니다.";
                        return;
                    }
                    else
                    {
                        downloadInfo.Status = "다운로드 완료...";
                    }

                });
            taskList.Add((task, downloadInfo));
        }//private async void AddButton_Click(object sender, RoutedEventArgs e)

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
                var item = WebtoonDownloadPanel.Children[i] as WebtoonDownloadPanelItem;
                var info = item.DataContext as WebtoonDownloadInfo;
                if (info.TitleId == titleId)
                {
                    return "이미 추가된 웹툰입니다.";
                }
            }
            return null;
        }

        private async Task<string> CheckUserInput(string uriText)
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
            

        }//private async void RunButton_Click(object sender, RoutedEventArgs e)

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            
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
            webtoonDownloadInfo.Status = "취소된 작업(삭제가능)";
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
            for(int i = 0; i < WebtoonDownloadPanel.Children.Count; i++)
            {
                var pitem = WebtoonDownloadPanel.Children[i] as WebtoonDownloadPanelItem;
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

    }
 
}
