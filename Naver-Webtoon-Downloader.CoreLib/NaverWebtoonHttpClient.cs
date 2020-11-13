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

namespace NaverWebtoonDownloader.CoreLib
{

    public class NaverWebtoonHttpClient : HttpClient
    {
#if !DEBUG
        internal NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string userAgent =
                $"NaverWebtoonDownloader/{assemblyVersion.Major}.{assemblyVersion.Minor} " +
                $"(Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor};" +
                $".Net Core {Environment.Version.Major}.{Environment.Version.Minor})";
            DefaultRequestHeaders.Add("User-Agent", userAgent);
        }
#endif

#if DEBUG
        public NaverWebtoonHttpClient() : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string userAgent =
                $"NaverWebtoonDownloader/{assemblyVersion.Major}.{assemblyVersion.Minor} " +
                $"(Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor};" +
                $".Net Core {Environment.Version.Major}.{Environment.Version.Minor})";
            DefaultRequestHeaders.Add("User-Agent", userAgent);
        }
#endif

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
        /// 지정한 회차의 페이지(detail.nhn)를 <seealso cref="HtmlDocument"/>로 반환합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="episodeNo"></param>
        /// <returns></returns>
        /// <exception cref="EpisodeNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
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
        /// 지정한 웹툰의 정보를 가져옵니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        /// <exception cref="WebtoonNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<WebtoonInfo> GetWebtoonInfoAsync(string titleId)
        {
            var document = await GetListPageDocumentAsync(titleId);
            var title = document.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
            var writer = await GetWebtoonWriterAsync(titleId);
            var webtoonInfo = new WebtoonInfo(titleId, title, writer, "", "");
            return webtoonInfo;
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
        /// <exception cref="EpisodeNotFoundException"></exception>
        /// <exception cref="HttpRequestException"></exception>
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
                    Size = 0,
                    Uri = nodes[i].Attributes["src"].Value
                });
            }
            var episodeInfo = new EpisodeInfo(titleId, episodeNo, title, date, images.ToArray());
            return episodeInfo;
        }

        public async Task<NaverComment[]> GetCommentsAsync(string titleId, int episodeNo)
        {
            string baseUri = 
                $"https://apis.naver.com/commentBox/cbox/web_naver_list_jsonp.json" +
                $"?ticket=comic" +
                $"&templateId=webtoon" +
                $"&pool=cbox3" +
                $"&_callback=jQuery112407502628653793267_1605270066791" +
                $"&lang=ko" +
                $"&country=KR" +
                $"&objectId={titleId}_{episodeNo}" +
                $"&categoryId=" +
                $"&pageSize=100" +
                $"&indexSize=10" +
                $"&groupId=" +
                $"&listType=OBJECT" +
                $"&pageType=default" +
                $"&page={{0}}" +
                $"&refresh=true" +
                $"&sort=NEW" +
                $"&_=1605270066793";
            HttpRequestMessage firstRequest = new HttpRequestMessage(HttpMethod.Get, string.Format(baseUri, 1));
            firstRequest.Headers.Add("Referer", $"https://comic.naver.com/detail.nhn?titleId={titleId}&no={episodeNo}");
            HttpResponseMessage firstResponse = await SendAsync(firstRequest);
            if (firstResponse.IsSuccessStatusCode)
            {
                firstResponse.EnsureSuccessStatusCode();
            }
            string firstResponseString = await firstResponse.Content.ReadAsStringAsync();
            firstResponseString = firstResponseString.Replace("jQuery112407502628653793267_1605270066791(", "").Replace("});", "}");
            JObject firstResponseJObject = JsonConvert.DeserializeObject<JObject>(firstResponseString);
            if(!(bool)firstResponseJObject["success"])
            {

            }
            int totalPages = (int)firstResponseJObject["result"]["pageModel"]["totalPages"];
            var comments = new List<NaverComment>();
            comments.AddRange(JsonConvert.DeserializeObject<List<NaverComment>>(firstResponseJObject["result"]["commentList"].ToString()));
            for (int i = 2; i <= totalPages; i++) 
            {
                var request = new HttpRequestMessage(HttpMethod.Get, string.Format(baseUri, i));
                request.Headers.Add("Referer", $"https://comic.naver.com/detail.nhn?titleId={titleId}&no={episodeNo}");
                var response = await SendAsync(request);
                if (response.IsSuccessStatusCode)
                {

                }
                var responseString = await response.Content.ReadAsStringAsync();
                responseString = responseString.Replace("jQuery112407502628653793267_1605270066791(", "").Replace("});", "}");
                var responseJObject = JsonConvert.DeserializeObject<JObject>(responseString);
                if (!(bool)responseJObject["success"])
                {

                }
                List<NaverComment> pageComments = JsonConvert.DeserializeObject<List<NaverComment>>(responseJObject["result"]["commentList"].ToString());
                comments.AddRange(pageComments);
            }
            return comments.ToArray();
        }
    }
}