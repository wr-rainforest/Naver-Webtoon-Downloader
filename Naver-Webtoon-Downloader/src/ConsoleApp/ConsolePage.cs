using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.Parser;

namespace WRforest.NWD
{
    class ConsolePage
    {
        public static void PrintMainPage(string version, ConsoleColor versioncolor)
        {
            Console.Write(" *Naver-Webtoon-Downloader ");
            WriteLine(version, versioncolor);
            Console.Write(" *Release : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases", ConsoleColor.Cyan);
            Console.Write(" *Source  : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader", ConsoleColor.Cyan);
            Console.Write(" *E-mail  : ");
            WriteLine("contact@wrforest.com", ConsoleColor.Cyan);
        }

        public static void PrintConfigInfoPage(ConsoleColor color)
        {
            Console.WriteLine(" *설정(/Data/configs/config.json)");
            var config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
            Console.Write(" *기본 다운로드 폴더 : ");
            WriteLine(config.DefaultDownloadDirectory, color);
            Console.Write(" *웹툰 폴더명 포맷   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "\"titleId\"", "\"웹툰 제목\""), color);
            Console.Write(" *회차 폴더명 포맷   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "\"titleId\"", "\"회차 번호\"", "\"회차 날짜\"", "\"웹툰 제목\"", "\"회차 제목\""), color);
            Console.Write(" *이미지 파일명 포맷 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "\"titleId\"", "\"회차 번호\"", "\"이미지 인덱스\"", "\"웹툰 제목\"", "\"회차 제목\""), color);
            /*Console.Write("\r\n 웹툰 폴더명 예시   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "748105", "독립일기"));
            Console.Write(" 회차 폴더명 예시   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "748105", "0004", "2020.06.24", "독립일기", "3화 이사 첫날"));

            Console.Write(" 이미지 파일명 예시 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "748105", "0004", "0002", "독립일기", "3화 이사 첫날"));*/
        }
        public static void PrintLine() => Console.WriteLine("------------------------------------------------------------------------");

        /*public static string ReadTitleId()//[--]  - 
        {
            string titleId;
            Agent agent = new Agent();
            Console.WriteLine(" *titleId => ?\r\n");
            Write(" 마음의소리          : https://comic.naver.com/webtoon/list.nhn?titleId=");
            WriteLine("20853", ConsoleColor.Green);
            Write("                            titleId => ");
            WriteLine("20853", ConsoleColor.Green);
            Write(" 유미의 세포들 502화 : https://comic.naver.com/webtoon/detail.nhn?titleId=");
            Write("651673", ConsoleColor.Green);
            WriteLine("&no=505&weekday=sat");
            Write("                            titleId => ");
            WriteLine("651673", ConsoleColor.Green);
            WriteLine("------------------------------------------------------------------------");
            while (true)
            {
                Console.Write(DateTime.Now.ToString("[yyyy-MM-dd hh:mm:ss] "));
                Console.Write("titleId : ");
                Console.ForegroundColor = ConsoleColor.Green;
                titleId = Console.ReadLine();
                Console.ResetColor();
                if (string.IsNullOrWhiteSpace(titleId))
                {
                    Error("titleId를 입력해주세요.");
                    continue;
                }
                if (!int.TryParse(titleId, out _))
                {
                    Error("titleId는 숫자입니다.");
                    continue;
                }
                agent.LoadPage(string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", titleId));
                var node = agent.Page.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]");
                if (node.Attributes["content"].Value == "네이버 웹툰")
                {
                    Error("존재하지 않는 titleId입니다.");
                    continue;
                }
                else
                {
                    break;
                }

            }
            return titleId;
        }
       */
        public static void PrintWebtoonListPage(string weekDay)
        {
            Agent agent = new Agent();
            Parser.Parser parser = new Parser.Parser(agent, new XPath());
            agent.LoadPage("https://comic.naver.com/webtoon/weekday.nhn");
            var t =parser.GetWebtoonList(weekDay);
            int maxByteLength=0;
            for (int i = 0; i < t.Length; i++)
            {
                int byteLength = GetByteLength(string.Format("{0}({1})", t[i].title, t[i].titleId));
                if (byteLength>maxByteLength)
                {
                    maxByteLength = byteLength;
                }
            }
            WriteLine("요일별 웹툰 : " + weekDay);
            for (int i = 0, j = 0; i < t.Length; i++, j++)
            {
                Write(t[i].title);
                Write("(");
                Write(t[i].titleId, ConsoleColor.Green);
                if (j == 3)
                {
                    WriteLine(")");
                    WriteLine("");
                    j = -1;
                }
                else
                {
                    int byteLength = GetByteLength(string.Format("{0}({1})", t[i].title, t[i].titleId));
                    Write(")".PadRight(maxByteLength-byteLength+1,' '));
                    
                }
            }
            WriteLine("");
        }
        
        public static void PrintCommandInfoPage()
        {
            WriteLine("웹툰 titleid 목록 불러오기 : get [mom/tue/wed/thu/fri/sat/sun]");
            WriteLine("예                         : get mon");
        }
        
        
        
        
        
        
        private static int GetByteLength(string text)
        {
            return Encoding.Default.GetBytes(text).Length;
        }




        public static void Write(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
        public static void WriteLine(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public static void Write(string msg)
        {
            Console.Write(msg);
        }
        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
        public static void Error(string msg)
        {
            WriteLine("Error : " + msg, ConsoleColor.Red);
        }
    }
}
