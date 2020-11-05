using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace wr_rainforest.Naver_Webtoon_Downloader
{
    class NaverWebtoonHttpClient : HttpClient
    {
        public NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DefaultRequestHeaders.Add("User-Agent",$"NaverWebtoonDownloader/{version.Major}.{version.Minor}");
        }
        public async Task<bool> Login(string id, string password)
        {
            throw new NotImplementedException("구현되지 않은 기능");
            var responseString = await GetStringAsync("https://nid.naver.com/login/ext/keys.nhn");
            var sessionSplited = responseString.Split(',');
            var sessionKey = sessionSplited[0];
            var sessionName = sessionSplited[1];
            var modulusKey = sessionSplited[2];
            var exponentKey = sessionSplited[3];
            var plainText = 
                $"{Convert.ToChar(sessionKey.Length)}" +
                $"{sessionKey}" +
                $"{Convert.ToChar(id.Length)}" +
                $"{id}" +
                $"{Convert.ToChar(password.Length)}" +
                $"{password}";
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var rsaParams = new RSAParameters()
            {
                Modulus = Enumerable.Range(0, modulusKey.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(modulusKey.Substring(x, 2), 16))
                .ToArray(),
                Exponent = Enumerable.Range(0, exponentKey.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(exponentKey.Substring(x, 2), 16))
                       .ToArray()
            };
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            var encrytedBytes = rsa.Encrypt(plainBytes, false);
            var encpw = BitConverter.ToString(encrytedBytes).Replace("-", "").ToLower();
            var loginRequest = new HttpRequestMessage();
            var keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("localechange", encpw);
            keyValuePairs.Add("encpw", encpw);
            keyValuePairs.Add("enctp", "1");
            keyValuePairs.Add("svctype", "1");
            var bvsd = new 
            {
                uuid = "b7763c36-2a93-4a96-a4df-a85679a575e1-0",
                encData= ""
            };
            keyValuePairs.Add("bvsd", JsonConvert.SerializeObject(bvsd));
            keyValuePairs.Add("smart_LEVEL", "-1");
            keyValuePairs.Add("encnm", sessionName);
            keyValuePairs.Add("locale", "ko_KR");
            keyValuePairs.Add("url", "https://www.naver.com");
            keyValuePairs.Add("id", "");
            keyValuePairs.Add("pw", "1");
            loginRequest.Content = new FormUrlEncodedContent(keyValuePairs);
            loginRequest.Headers.Add("Referer", "https://nid.naver.com/nidlogin.login");
            //미구현
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