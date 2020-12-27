using HtmlAgilityPack;
using NaverWebtoonDownloader.CoreLib.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NaverWebtoonDownloader.CoreLib
{
    public class NaverWebtoonClient
    {
        private static HttpClient _client;

        private static CookieContainer _cookieContainer;

        static NaverWebtoonClient()
        {
            _cookieContainer = new CookieContainer() { };
            var handler = new SocketsHttpHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip,
                CookieContainer = _cookieContainer
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            string userAgent =
                $"NaverWebtoonDownloader/{v.Major}{v.Minor} " +
                $"(Windows NT 10.0;" +
                $".Net Core {Environment.Version.Major}.{Environment.Version.Minor})";
            _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        public NaverWebtoonClient()
        {
            
        }

        public async Task<string> SetCookieAsync(string nid_aut, string nid_ses)
        {
            _cookieContainer.Add(new Cookie("NID_AUT", nid_aut, "/", ".naver.com"));
            _cookieContainer.Add(new Cookie("NID_SES", nid_ses, "/", ".naver.com"));
            var responseString = await _client.GetStringAsync("https://www.naver.com");
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            string isLogin = null;
            foreach(var x in document.DocumentNode.SelectNodes("//script"))
            {
                var innerText = x.InnerText.Replace(" ", string.Empty)
                                           .Replace("\"", string.Empty)
                                           .Replace("{", string.Empty)
                                           .Replace("}", string.Empty);
                if (innerText.StartsWith("window.nmain.gv=isLogin"))
                {
                    isLogin = innerText.Split(',')[0].Replace("window.nmain.gv=isLogin:", string.Empty);
                    break;
                };
            };
            return isLogin == "false" ? null : isLogin;
        }

        public async Task<Webtoon> GetWebtoonAsync(int titleID)
        {
            var listPageDocument = await GetListPageDocumentAsync(titleID);
            var detailPageDocument = await GetDetailPageDocumentAsync(titleID, 1);
            Webtoon webtoon = new Webtoon()
            {
                ID = titleID,
                Title = listPageDocument.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value,
                Writer = detailPageDocument.DocumentNode.SelectSingleNode("//*[@name=\"itemWriterId\"]").Attributes["value"].Value
            };
            return webtoon;
        }

        public async Task<Episode> GetEpisodeAsync(int titleID, int episodeNo)
        {
            var detailPageDocument = await GetDetailPageDocumentAsync(titleID, episodeNo);
            Episode episode = new Episode()
            {
                WebtoonID = titleID,
                No = episodeNo,
                Title = detailPageDocument.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value,
                Date = DateTime.ParseExact(detailPageDocument.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText, "yyyy.MM.dd", new CultureInfo("ko-KR")),
                Images = new List<Image>()
            };
            HtmlNodeCollection nodes = detailPageDocument.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            for (int i = 0; i < nodes.Count; i++)
            {
                episode.Images.Add(new Image()
                {
                    WebtoonID = titleID,
                    EpisodeNo = episodeNo,
                    IsDownloaded = false,
                    No = i + 1,
                    Size = 0,
                    ImageUrl = nodes[i].Attributes["src"].Value
                });
            }
            return episode;
        }

        public async Task<int> GetLatestEpisodeNoAsync(int titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var relativeUri = document.DocumentNode.SelectSingleNode("//td[@class=\"title\"]/a").Attributes["href"].Value;
            var absoluteUri = $"https://comic.naver.com{relativeUri}";
            return int.Parse(HttpUtility.ParseQueryString(new Uri(absoluteUri).Query).Get("no"));
        }

        public async Task<byte[]> GetImageFileAsync(Image image)
        {
            var buff = await _client.GetByteArrayAsync(image.ImageUrl);
            return buff;
        }

        private async Task<HtmlDocument> GetListPageDocumentAsync(int titleID)
        {
            var uri = $"https://comic.naver.com/webtoon/list.nhn?titleId={titleID}";
            var response = await _client.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("main.nhn"))
            {
                throw new WebtoonNotFoundException();
            }
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }

        private async Task<HtmlDocument> GetDetailPageDocumentAsync(int titleID, int episodeNo)
        {
            var uri = $"https://comic.naver.com/webtoon/detail.nhn?titleId={titleID}&no={episodeNo}";
            var response = await _client.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("main.nhn"))
            {
                throw new WebtoonNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("detail.nhn"))
            {
                throw new EpisodeNotFoundException();
            }
            else if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("list.nhn"))
            {
                throw new EpisodeNotFoundException();
            }
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }
    }
}
