using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WRforest.NWD.Parser;

namespace WRforest.NWD
{
    class Program
    {
        static void Main(string[] args)
        {
            NaverWebtoonDownloader nwd = new NaverWebtoonDownloader();
            MainPage("v0.2.0-alpha", ConsoleColor.Red);
            PrintConfig();
            string titleId = ReadTitleId();
            IO.Log = Log;
            Console.Clear();
            MainPage("v0.2.0-alpha", ConsoleColor.Red);
            nwd.Download(titleId);
            Console.WriteLine("종료하려면 아무 키나 누르십시오 . . .");
            Console.ReadKey();
        }
        static void Log(string msg)
        {

        }
        static void Write(string msg , ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
        static void WriteLine(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        static void Write(string msg)
        {
            Console.Write(msg);
        }
        static void WriteLine(string msg)
        { 
            Console.WriteLine(msg);
        }
        static void Error(string msg)
        {
            WriteLine("Error : "+msg, ConsoleColor.Red);
        }
        static void MainPage(string version, ConsoleColor versioncolor)
        {
            Console.WriteLine("------------------------------------------------------------------------");
            Console.Write(" *Naver-Webtoon-Downloader ");
            WriteLine(version, versioncolor);
            Console.Write(" *Release : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases", ConsoleColor.Cyan);
            Console.Write(" *Source  : ");
            WriteLine("https://github.com/wr-rainforest/Naver-Webtoon-Downloader", ConsoleColor.Cyan);
            Console.Write(" *E-mail  : ");
            WriteLine("contact@wrforest.com", ConsoleColor.Cyan);
            Console.WriteLine("------------------------------------------------------------------------");
        }
        static void PrintConfig()
        {
            Console.WriteLine(" *설정(/Data/configs/config.json)\r\n");
            var config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
            Console.Write(" 기본 다운로드 폴더 : ");
            WriteLine(config.DefaultDownloadDirectory, ConsoleColor.Yellow);
            Console.Write("\r\n 웹툰 폴더명 포맷   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "\"titleId\"", "\"웹툰 제목\""), ConsoleColor.Yellow);
            Console.Write(" 회차 폴더명 포맷   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "\"titleId\"", "\"회차 번호\"", "\"회차 날짜\"", "\"웹툰 제목\"", "\"회차 제목\""), ConsoleColor.Yellow);
            Console.Write(" 이미지 파일명 포맷 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "\"titleId\"", "\"회차 번호\"", "\"이미지 인덱스\"", "\"웹툰 제목\"", "\"회차 제목\""), ConsoleColor.Yellow);
            Console.Write("\r\n 웹툰 폴더명 예시   : ");
            WriteLine(string.Format(config.WebtoonDirectoryNameFormat, "748105", "독립일기"));
            Console.Write(" 회차 폴더명 예시   : ");
            WriteLine(string.Format(config.EpisodeDirectoryNameFormat, "748105", "0004", "2020.06.24", "독립일기", "3화 이사 첫날"));

            Console.Write(" 이미지 파일명 예시 : ");
            WriteLine(string.Format(config.ImageFileNameFormat, "748105", "0004", "0002", "독립일기", "3화 이사 첫날"));
            Console.WriteLine("------------------------------------------------------------------------");
        }
        static string ReadTitleId()//[--]  - 
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
    }
}
