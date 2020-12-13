using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NaverWebtoonDownloader.CoreLib.Database.Model;

namespace NaverWebtoonDownloader.CoreLib
{

    public class NaverWebtoonHttpClient : HttpClient
    {
        public NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string userAgent =
                $"NaverWebtoonDownloader/{assemblyVersion.Major}.{assemblyVersion.Minor} " +
                $"(Windows NT 10.0;" +
                $".Net Core {Environment.Version.Major}.{Environment.Version.Minor})";
            DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        /// <summary>
        /// 지정한 웹툰의 목록 페이지(list.nhn)를 <seealso cref="HtmlDocument"/>로 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<HtmlDocument> GetListPageDocumentAsync(string titleId)
        {
            var uri = $"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}";
            var response = await GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("main.nhn"))
            {
                throw new WebtoonNotFoundException(titleId);
            }
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }

        /// <summary>
        /// 지정한 회차의 페이지(detail.nhn)를 <seealso cref="HtmlDocument"/>로 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="EpisodeNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<HtmlDocument> GetDetailPageDocumentAsync(string titleId, int episodeNo)
        {
            var uri = $"https://comic.naver.com/webtoon/detail.nhn?titleId={titleId}&no={episodeNo}";
            var response = await GetAsync(uri);
            // 존재하지 않는 titleId일 경우 메인페이지로 리다이렉트
            if(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("main.nhn"))
            {
                throw new WebtoonNotFoundException(titleId);
            }
            // 존재하지 않는 no일 경우 최신 회차 페이지로 리다이렉트
            else if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("detail.nhn"))
            {
                throw new EpisodeNotFoundException(titleId, episodeNo);
            }
            // 유료화된 회차에 접근시 목록으로 리다이렉트
            else if (response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location.OriginalString.Contains("list.nhn"))
            {
                throw new EpisodeNotFoundException(titleId, episodeNo);
            }
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }

        /// <summary>
        /// 지정한 웹툰의 정보를 가져옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<Webtoon> GetWebtoonAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            var writer = await GetWebtoonWriterAsync(titleId);
            var webtoon = new Webtoon() 
            {
                TitleId = titleId,
                Title = title,
                Writer = writer,
                Description = null,
                Genre = null,
            };
            return webtoon;
        }

        /// <summary>
        /// 웹툰 작가를 불러옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="EpisodeNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        private async Task<string> GetWebtoonWriterAsync(string titleId)
        {
            var document = await GetDetailPageDocumentAsync(titleId, 1);
            var writer = document.DocumentNode.SelectSingleNode("//*[@name=\"itemWriterId\"]").Attributes["value"].Value;
            return writer;
        }

        /// <summary>
        /// 웹툰의 마지막 회차 번호를 불러옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<int> GetLatestEpisodeNoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var relativeUri = document.DocumentNode.SelectSingleNode("//td[@class=\"title\"]/a").Attributes["href"].Value;
            var absoluteUri = $"https://comic.naver.com{relativeUri}";
            return int.Parse(HttpUtility.ParseQueryString(new Uri(absoluteUri).Query).Get("no"));
        }

        /// <summary>
        /// 지정한 회차의 정보를 가져옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="EpisodeNotFoundException"></exception>
        public async Task<Episode> GetEpisodeInfoAsync(string titleId, int episodeNo)
        {
            var document = await GetDetailPageDocumentAsync(titleId, episodeNo);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
            var date = document.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
            List<Image> images = new List<Image>();
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            for (int i = 0; i < nodes.Count; i++)
            {
                images.Add(new Image()
                {
                    TitleId = titleId,
                    EpisodeNo = episodeNo,
                    IsDownloaded = false,
                    ImageNo = i + 1,
                    Size = 0,
                    Url = nodes[i].Attributes["src"].Value
                });
            }
            var episodeInfo = new EpisodeInfo(titleId, episodeNo, title, date, images.ToArray());
            return episodeInfo;
        }

    }
}