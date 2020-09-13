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
        Agent agent;
        Parser.Parser parser;
        public ConsolePage(Agent agent, Parser.Parser parser)
        {
            this.agent = agent;
            this.parser = parser;
        }
        public void PrintMainPage(string version, ConsoleColor versioncolor)
        {
            string latestVersion;
            try
            {
                agent.LoadPage("https://wr-rainforest.github.io/Naver-Webtoon-Downloader/info/latest_version.html");
                latestVersion=agent.Page.DocumentNode.SelectSingleNode("/html/body").InnerText;
            }
            catch
            {
                latestVersion = "v*.*.*";
            }
            Write(" *");
            Write("Naver-Webtoon-Downloader", ConsoleColor.Blue);
            Write(" [Version : ");
            Write(version, versioncolor);
            Write(" ] [Latest : ");
            Write(latestVersion, versioncolor);
            WriteLine(" ]");
            Console.Write(" -Release : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases");
            Console.Write(" -Source  : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader");
            Console.Write(" -E-mail  : ");
            WriteLine("contact@wrforest.com");
        }

        public void PrintConfigInfoPage(ConsoleColor color)
        {
            Write(" *");
            Write("설정", ConsoleColor.Cyan);
            Write("(");
            Write("/Data/configs/config.json", ConsoleColor.Green);
            WriteLine(")");
            var config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
            Console.Write(" -기본 다운로드 폴더 : ");
            WriteLine(config.DefaultDownloadDirectory, color);
            Console.Write(" -웹툰 폴더명 포맷   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "\"titleId\"", "\"웹툰 제목\""), color);
            Console.Write(" -회차 폴더명 포맷   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "\"titleId\"", "\"회차 번호\"", "\"회차 날짜\"", "\"웹툰 제목\"", "\"회차 제목\""), color);
            Console.Write(" -이미지 파일명 포맷 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "\"titleId\"", "\"회차 번호\"", "\"이미지 인덱스\"", "\"웹툰 제목\"", "\"회차 제목\""), color);
            /*Console.Write("\r\n 웹툰 폴더명 예시   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "748105", "독립일기"));
            Console.Write(" 회차 폴더명 예시   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "748105", "0004", "2020.06.24", "독립일기", "3화 이사 첫날"));

            Console.Write(" 이미지 파일명 예시 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "748105", "0004", "0002", "독립일기", "3화 이사 첫날"));*/
        }
        public void PrintLine(char c, int count, ConsoleColor color) => WriteLine(new string(c, count), color);

        /*public  string ReadTitleId()//[--]  - 
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
                }s
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
        public void PrintWebtoonListPage(string weekDay)
        {
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
            WriteLine("\r\n요일별 웹툰 : " + weekDay);
            WriteLine("");
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
        
        public void PrintCommandInfoPage(ConsoleColor function, ConsoleColor option)
        {
            WriteLine(" *명령어 정보 : ",ConsoleColor.Cyan);
            Write(" ");
            Write("get       ",function);
            Write(" : ");
            Write("get", function);
            Write(" [");
            Write("weekday", option);
            Write("]      /       ");
            Write("weekday", option);
            WriteLine(" : 요일입니다. (mon/tue/wed/thu/fri/sat/sun)");
            WriteLine("   선택한 요일의 웹툰 목록을 가져옵니다.");

            Write(" ");
            Write("download  ", function);
            Write(" : ");
            Write("download", function);
            Write(" [");
            Write("titleId", option);
            Write("] /       ");
            Write("titleId", option);
            WriteLine(" : 다운로드할 웹툰의 titleId 입니다.");
            WriteLine("   선택한 웹툰을 다운로드합니다.");
            

            Write(" ");
            Write("clear     ", function);
            Write(" : ");
            WriteLine("clear", function);
            WriteLine("   콘솔을 비웁니다.");
        }
        
        
        
        
        
        
        private int GetByteLength(string text)
        {
            return Encoding.Default.GetBytes(text).Length;
        }




        public  void Write(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
        public  void WriteLine(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public  void Write(string msg)
        {
            Console.Write(msg);
        }
        public  void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
        public  void Error(string msg)
        {
            WriteLine("Error : " + msg, ConsoleColor.Red);
            Console.WriteLine();
        }
    }
}
