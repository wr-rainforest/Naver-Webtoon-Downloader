using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WRforest.NWD
{
    class Parser
    {
        HtmlDocument htmlDocument;
        /// <summary>
        /// <paramref name="htmlDocument"/>를 파싱하는 파서의 새로운 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="htmlDocument"></param>
        public Parser(Agent agent)
        {
            this.htmlDocument = agent.Page;
        }
        /// <summary>
        /// <paramref name="htmlDocument"/>에서 가장 최신 회차의 번호를 파싱합니다.
        /// <code> htmlDocument : https://comic.naver.com/webtoon/list.nhn?titleId={0}</code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public string GetLatestEpisodeNo()
        {
            string href = htmlDocument.DocumentNode.SelectSingleNode("//td[@class=\"title\"]/a").Attributes["href"].Value;
            Uri myUri = new Uri("https://comic.naver.com" + href);
            return HttpUtility.ParseQueryString(myUri.Query).Get("no");
            //HttpUtility => system.Web 참조 추가
        }

        /// <summary>
        /// <paramref name="htmlDocument"/>에서 웹툰 제목을 파싱합니다. 
        /// <code> htmlDocument : https://comic.naver.com/webtoon/list.nhn?titleId={0}</code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public string GetWebtoonTitle()
        {
            return htmlDocument.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]").Attributes["content"].Value;
        }

        /// <summary>
        /// <paramref name="htmlDocument"/>에서 회차 제목을 파싱합니다.
        /// <code> htmlDocument : https://comic.naver.com/webtoon/detail.nhn?titleId={0} no={1} </code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public string GetEpisodeTitle()
        {
            return htmlDocument.DocumentNode.SelectSingleNode("//*[@property=\"og:description\"]").Attributes["content"].Value;
        }

        /// <summary>
        /// <paramref name="htmlDocument"/>에서 회차 날짜를 파싱합니다.
        /// <code> htmlDocument : https://comic.naver.com/webtoon/detail.nhn?titleId={0} no={1} </code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public string GetEpisodeDate()
        {
            return htmlDocument.DocumentNode.SelectSingleNode("//*[@class=\"date\"]").InnerText;
        }

        /// <summary>
        /// <paramref name="htmlDocument"/>에서 이미지 소스 리스트를 파싱합니다. 
        /// <code> htmlDocument : https://comic.naver.com/webtoon/detail.nhn?titleId={0} no={1} </code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <code> htmlDocument : https://comic.naver.com/webtoon/detail.nhn?titleId={0} no={1} </code>
        /// <returns></returns>
        public string[] GetComicContentImageUrls()
        {
            List<string> urls = new List<string>();
            HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//*[@alt=\"comic content\"]");
            if (htmlNodes == null)
            {
                Console.WriteLine(htmlDocument.DocumentNode.InnerHtml);
                throw new Exception("웹툰 이미지 목록을 불러올 수 없습니다.");
            }

            for (int i = 0; i < htmlNodes.Count; i++)
            {
                urls.Add(htmlNodes[i].Attributes["src"].Value);
            }
            return urls.ToArray();
        }

        /// <summary>
        /// <paramref name="htmlDocument"/>에서 현재 회차의 episodeNo를 파싱합니다.
        /// <code> htmlDocument : https://comic.naver.com/webtoon/detail.nhn?titleId={0} no={1} </code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public string GetCurrentEpisodeNo()
        {
            string href = htmlDocument.DocumentNode.SelectSingleNode("//*[@property=\"og:url\"]").Attributes["content"].Value;
            var url = href.Replace("amp;", "&");
            Uri myUri = new Uri(url);
            return HttpUtility.ParseQueryString(myUri.Query).Get("no");
            //HttpUtility => system.Web 참조 추가
        }

        /// <summary>
        /// <paramref name="htmlDocument"/> 에서 <paramref name="weekDay"/>가 지정하는 요일의 웹툰 목록을 파싱하여 튜플 리스트를 반환합니다.
        /// <code> htmlDocument : https://comic.naver.com/webtoon/weekday.nhn </code>
        /// </summary>
        /// <param name="htmlDocument"></param>
        /// <returns></returns>
        public (string title, string titleId)[] GetWebtoonList(string weekDay)
        {
            //반환할 배열 초기화
            List<(string title, string titleId)> list = new List<(string title, string titleId)>();
            //웹툰 노드 선택
            HtmlNodeCollection htmlNodes = htmlDocument.DocumentNode.SelectNodes("//a[@class=\"title\"]");
            for (int i = 0; i < htmlNodes.Count; i++)
            {
                //노드의 href 태그에서 url 가져옴.
                string href = htmlNodes[i].Attributes["href"].Value;
                Uri myUri = new Uri("https://comic.naver.com" + href);
                //url에서titleId, weekday 추출
                var titleId = HttpUtility.ParseQueryString(myUri.Query).Get("titleId");
                var day = HttpUtility.ParseQueryString(myUri.Query).Get("weekday");
                if (weekDay != day)
                {
                    //weekday가 지정한 요일과 다르면 건너뜀
                    continue;
                }
                var title = htmlNodes[i].Attributes["title"].Value;
                var item = (title, titleId);
                if (list.Contains(item))
                {
                    continue;//이미 포함된 튜플은 추가하지 않음
                }
                list.Add(item);
            }
            return list.ToArray();
        }


    }

}
