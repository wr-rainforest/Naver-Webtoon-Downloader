using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Parser
{
    class Agent
    {
        private WebClient client;

        /// <summary>
        /// 현재 agent의 html 페이지입니다.
        /// </summary>
        public HtmlDocument Page { get; private set; }

        /// <summary>
        /// <seealso cref="Agent"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public Agent()
        {
            Page = new HtmlDocument();
            client = new WebClient();
            client.Encoding = Encoding.UTF8;
            AddHeader("User-Agent", "Mozilla / 5.0(Windows NT 10.0) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 85.0.4183.102 Safari / 537.36");
        }

        /// <summary>
        /// <paramref name="url"/>의 HTML 페이지를 불러옵니다.
        /// </summary>
        /// <param name="url"></param>
        public void LoadPage(string url)
        {
            Page.LoadHtml(client.DownloadString(url));
        }

        /// <summary>
        /// <paramref name="url"/>의 데이터를 다운로드합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns><seealso cref="byte[]"/>data</returns>
        public byte[] DownloadData(string url)
        {
            return client.DownloadData(url);//
        }

        /// <summary>
        /// <seealso cref="WebClient.Headers"/>에 <paramref name="name"/>헤더를 추가합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            client.Headers.Add(name + ":" + value);
        }
        /// <summary>
        /// <seealso cref="WebClient.Headers"/>의 <paramref name="name"/>헤더를 제거합니다.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveHeader(string name)
        {
            client.Headers.Remove(name);
        }
        /// <summary>
        /// <seealso cref="WebClient.Headers"/>의 <paramref name="name"/>헤더를 <paramref name="value"/>로 설정합니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetHeader(string name, string value)
        {
            client.Headers.Set(name, value);
        }
    }
}
