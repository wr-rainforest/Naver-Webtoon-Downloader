using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace NaverWebtoonDownloader.CoreLib
{

    public class NaverWebtoonHttpClient : HttpClient
    {
        internal NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string userAgent =
                $"{assemblyName}/{assemblyVersion.Major}.{assemblyVersion.Minor} " +
                $"(Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor};" +
                $".Net Core {Environment.Version.Major}.{Environment.Version.Minor})";
            DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        /// <summary>
        /// 구현되지 않은 기능
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
            keyValuePairs.Add("bvsd", JsonSerializer.Serialize(bvsd));
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

        /// <summary>
        /// <paramref name="titleId"/>가 지정하는 웹툰의 목록 페이지(webtoon/list.nhn?titleId=)를 <seealso cref="HtmlDocument"/>로 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="HttpRequestException">
        public async Task<HtmlDocument> GetListPageDocumentAsync(string titleId)
        {
            var uri = $"https://comic.naver.com/webtoon/list.nhn?titleId={titleId}";
            var response = await GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Redirect)
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
        /// <paramref name="titleId"/>, <paramref name="episodeNo"/>가 지정하는 회차의 페이지(https://comic.naver.com/webtoon/detail.nhn?titleId=amp;no=)의 <seealso cref="HtmlDocument"/>를 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        public async Task<HtmlDocument> GetDetailPageDocumentAsync(string titleId, int episodeNo)
        {
            var uri = $"https://comic.naver.com/webtoon/detail.nhn?titleId={titleId}&no={episodeNo}";
            var response = await GetAsync(uri);
            if(response.StatusCode==HttpStatusCode.Redirect)
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
        /// <seealso cref="WebtoonInfo"/>를 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        public async Task<WebtoonInfo> GetWebtoonInfoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            var writer = await GetWebtoonWriterAsync(titleId);
            var webtoonInfo = new WebtoonInfo(titleId, title, writer, "", "");
            return webtoonInfo;
        }

        /// <summary>
        /// 웹툰 제목을 불러옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public async Task<string> GetWebtoonTitleAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            return title;
        }

        /// <summary>
        /// 웹툰 작가를 불러옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public async Task<string> GetWebtoonWriterAsync(string titleId)
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
        public async Task<int> GetLatestEpisodeNoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var relativeUri = document.DocumentNode.SelectSingleNode("//td[@class=\"title\"]/a").Attributes["href"].Value;
            var absoluteUri = $"https://comic.naver.com{relativeUri}";
            return int.Parse(HttpUtility.ParseQueryString(new Uri(absoluteUri).Query).Get("no"));
        }
        /// <summary>
        /// <seealso cref="EpisodeInfo"/>를 불러옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        public async Task<EpisodeInfo> GetEpisodeInfoAsync(string titleId, int episodeNo)
        {
            var document = await GetDetailPageDocumentAsync(titleId, episodeNo);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
            var date = document.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
            List<ImageInfo> images = new List<ImageInfo>();
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            for (int i = 0; i < nodes.Count; i++)
            {
                images.Add(new ImageInfo()
                {
                    TitleId = titleId,
                    No = episodeNo,
                    Downloaded = 0,
                    Index = i,
                    Size = -1,
                    Uri = nodes[i].Attributes["src"].Value
                });
            }
            var episodeInfo = new EpisodeInfo(titleId, episodeNo, title, date, images.ToArray());
            return episodeInfo;
        }

        public async Task<string> GetEpisodeTitleAsync(string titleId, int episodeNo)
        {
            var document = await GetDetailPageDocumentAsync(titleId, episodeNo);
            return document.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
        }

        public async Task<string> GetEpisodeDateAsync(string titleId, int episodeNo)
        {
            var document = await GetDetailPageDocumentAsync(titleId, episodeNo);
            return document.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
        }

        /// <summary>
        /// 회차의 이미지 uri을 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        public async Task<string[]> GetEpisodeImageUriListAsync(string titleId, int episodeNo)
        {
            var document = await GetDetailPageDocumentAsync(titleId, episodeNo);
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