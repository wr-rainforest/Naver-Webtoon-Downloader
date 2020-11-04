
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace wr_rainforest.WebtoonDownloader
{
    class NaverWebtoonHttpClient : HttpClient
    {
        public NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DefaultRequestHeaders.Add("User-Agent",$"NaverWebtoonDownloader/{version.Major}.{version.Minor}");
        }
        private async Task<HtmlDocument> GetListPageDocumentAsync(string titleId)
        {
            var uri = $"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}";
            var responseString = await GetStringAsync(uri);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }

        private async Task<HtmlDocument> GetEpisodePageDocumentAsync(string titleId, int episodeNo)
        {
            var uri = $"https://comic.naver.com/webtoon/detail.nhn?titleId={titleId}&no={episodeNo}";
            var response = await GetAsync(uri);
            if(response.StatusCode==HttpStatusCode.Redirect)
            {
                throw new EpisodeNotFoundException(titleId, episodeNo);
            }
            var responseString = await GetStringAsync(uri);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseString);
            return document;
        }

        public async Task<WebtoonInfo> GetWebtoonInfoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            var writer = await GetWebtoonWriterAsync(titleId);
            var webtoonInfo = new WebtoonInfo(titleId, title, writer, "", "");
            return webtoonInfo;
        }

        public async Task<string> GetWebtoonTitleAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            return title;
        }

        public async Task<string> GetWebtoonWriterAsync(string titleId)
        {
            var document = await GetEpisodePageDocumentAsync(titleId, 1);
            var writer = document.DocumentNode.SelectSingleNode("//*[@name=\"itemWriterId\"]").Attributes["value"].Value;
            return writer;
        }

        public async Task<int> GetLatestEpisodeNoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var relativeUri = document.DocumentNode.SelectSingleNode("//td[@class=\"title\"]/a").Attributes["href"].Value;
            var absoluteUri = $"https://comic.naver.com{relativeUri}";
            return int.Parse(HttpUtility.ParseQueryString(new Uri(absoluteUri).Query).Get("no"));
        }
        // Episode
        public async Task<EpisodeInfo> GetEpisodeInfoAsync(string titleId, int episodeNo)
        {
            var document = await GetEpisodePageDocumentAsync(titleId, episodeNo);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
            var date = document.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
            List<string> uris = new List<string>();
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            for (int i = 0; i < nodes.Count; i++)
            {
                uris.Add(nodes[i].Attributes["src"].Value);
            }
            var imageUris = uris.ToArray();
            var episodeInfo = new EpisodeInfo(titleId, episodeNo, title, date, imageUris);
            return episodeInfo;
        }

        public async Task<string> GetEpisodeTitleAsync(string titleId, int episodeNo)
        {
            var document = await GetEpisodePageDocumentAsync(titleId, episodeNo);
            return document.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
        }

        public async Task<string> GetEpisodeDateAsync(string titleId, int episodeNo)
        {
            var document = await GetEpisodePageDocumentAsync(titleId, episodeNo);
            return document.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
        }

        public async Task<string[]> GetEpisodeImageUriListAsync(string titleId, int episodeNo)
        {
            var document = await GetEpisodePageDocumentAsync(titleId, episodeNo);
            List<string> uris = new List<string>();
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            for (int i = 0; i < nodes.Count; i++)
            {
                uris.Add(nodes[i].Attributes["src"].Value);
            }
            return uris.ToArray();
        }

    }
}