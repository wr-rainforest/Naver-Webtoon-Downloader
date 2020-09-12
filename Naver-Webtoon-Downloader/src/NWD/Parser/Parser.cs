using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WRforest.NWD.Parser
{
    /// <summary>
    /// <seealso cref="Agent.Page"/>를 파싱하는 클래스입니다.
    /// </summary>
    class Parser
    {
        /// <summary>
        /// 생성자 매개변수인 <seealso cref="Agent"/>의 <seealso cref="Agent.Page"/> 객체 참조입니다.
        /// </summary>
        private HtmlDocument Page { get; set; }
        private XPath XPath;
        /// <summary>
        /// <seealso cref="Agent.Page"/>를 파싱하는 Parser 객체를 생성합니다.
        /// </summary>
        /// <param name="Page">파싱할 Page를 가진 Agent 인스턴스입니다.</param>
        public Parser(Agent agent, XPath xPath)
        {
            Page = agent.Page;
            XPath = xPath;
        }
        /// <summary>
        /// 현재 로딩된 페이지에서 가장 최신 회차의 번호를 파싱합니다. 로딩된 페이지가 webtoon/list.nhn?titleId={0}가 아닐 경우 원하는 동작을 하지 않을 수 있습니다.
        /// </summary>
        /// <returns></returns>
        public string GetLatestEpisodeNo()
        {
            string href = Page.DocumentNode.SelectSingleNode(XPath.LatestEpisode).Attributes["href"].Value;
            Uri myUri = new Uri("https://comic.naver.com" + href);
            return HttpUtility.ParseQueryString(myUri.Query).Get("no");
            //HttpUtility => system.Web 참조 추가
        }
        /// <summary>
        /// 현재 로딩된 페이지에서 웹툰 제목을 파싱합니다. 타이틀페이지
        /// </summary>
        /// <returns></returns>
        public string GetWebtoonTitle()
        {
            return Page.DocumentNode.SelectSingleNode(XPath.WebtoonTitle).Attributes["content"].Value;
        }
        /// <summary>
        /// 현재 로딩된 페이지에서 회차 제목을 파싱합니다. 회차 페이지
        /// </summary>
        /// <returns></returns>
        public string GetEpisodeTitle()
        {
            return Page.DocumentNode.SelectSingleNode(XPath.EpisodeTitle).Attributes["content"].Value;
        }
        /// <summary>
        /// 현재 로딩된 페이지에서 회차 날짜를 파싱합니다. 회차 페이지
        /// </summary>
        /// <returns></returns>
        public string GetEpisodeDate()
        {
            return Page.DocumentNode.SelectSingleNode(XPath.Date).InnerText;
        }
        /// <summary>
        /// 현재 로딩된 페이지에서 이미지 소스 리스트를 파싱합니다. 회차 페이지
        /// </summary>
        /// <returns></returns>
        public string[] GetComicContentImageUrls()
        {
            List<string> urls = new List<string>();
            HtmlNodeCollection htmlNodes = Page.DocumentNode.SelectNodes(XPath.ComicContent);
            if (htmlNodes.Count == 0)
                throw new Exception("웹툰 이미지 목록을 불러올 수 없습니다.");
            for (int i = 0; i < htmlNodes.Count; i++)
            {
                urls.Add(htmlNodes[i].Attributes["src"].Value);
            }
            return urls.ToArray();
        }
        /// <summary>
        /// 웹툰작가 이름을 불러옵니다. 아직 구현되지 않았습니다.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetWebtoonWriterNickName()
        {
            throw new NotImplementedException();//todo
        }
        /// <summary>
        /// 웹툰 장르를 불러옵니다. 아직 구현되지 않았습니다.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetWebtoonGenre()
        {
            throw new NotImplementedException();//todo
        }
        /// <summary>
        /// 웹툰 설명을 불러옵니다. 아직 구현되지 않았습니다.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetWebtoonDescription()
        {
            throw new NotImplementedException();//todo
        }
        /// <summary>
        /// 현재 페이지의 베스트 댓글을 파싱합니다. 아직 구현되지 않았습니다.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] GetEpsiodeBestComments()
        {
            throw new NotImplementedException();//todo
        }

    }

}
