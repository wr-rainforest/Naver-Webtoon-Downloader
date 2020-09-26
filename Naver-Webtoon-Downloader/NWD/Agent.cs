
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD
{
    class Agent
    {
        private WebClient webClient;

        /// <summary>
        /// 현재 agent의 html 페이지입니다.
        /// </summary>
        public HtmlDocument Page { get; set; }

        /// <summary>
        /// <seealso cref="Agent"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public Agent()
        {
            Page = new HtmlDocument();
            webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
        }
        /// <summary>
        /// <paramref name="url"/>의 HTML 페이지를 불러옵니다.
        /// </summary>
        /// <param name="url"></param>
        public void LoadPage(string url)
        {
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; rv:80.0) Gecko/20100101 Firefox/80.0");
            Page.LoadHtml(webClient.DownloadString(url));
        }

        /// <summary>
        /// <paramref name="url"/>의 데이터를 다운로드합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns><seealso cref="byte[]"/>data</returns>
        public async Task<byte[]> DownloadWebtoonImageAsync(string url)
        {
            webClient.Headers.Add("Host: image-comic.pstatic.net");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; rv:80.0) Gecko/20100101 Firefox/80.0");
            webClient.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            webClient.Headers.Add("Accept-Language: ko-KR,ko;q=0.8,en-US;q=0.5,en;q=0.3");
            webClient.Headers.Add("Accept-Encoding: gzip, deflate, br");
            webClient.Headers.Add("Cache-Control: max-age=0");
            webClient.Headers.Add("TE: Trailers");
            var buff = await webClient.DownloadDataTaskAsync(url);
            return buff;
        }
    }
}