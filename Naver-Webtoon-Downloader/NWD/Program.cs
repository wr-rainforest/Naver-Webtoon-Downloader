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
        public static int cursorPosition;
        static void Main(string[] args)
        { 
            string configFolderPath = "config";
            string configFileName = "config.json";
            string xPathConfigFileName = "xpath.json";


            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] assemsplit = assemblyVersion.Split('.');
            var version = $"{assemsplit[0]}.{assemsplit[1]}";
            var build = $"{assemsplit[2]}.{assemsplit[3]}";
            var Title = $"네이버 웹툰 다운로더 v{version} ({build})";
            Console.Title = Title;
            IO.Print($" 네이버 웹툰 다운로더 v{version} (빌드 {build})");


            CheckUpdate(assemsplit);

            IO.Print($" 홈페이지 : https://nwd.wrforest.com/");
            IO.Print($" 소스코드 : https://github.com/wr-rainforest/Naver-Webtoon-Downloader");
            IO.Print($" 연락처   : contact@wrforest.com");
            
            IO.Print(new string('-',100));

            Parser.XPath xPath;
            Config config;
            
            if (IO.Exists(configFolderPath, configFileName))
            {
                config = new Config(IO.ReadTextFile(configFolderPath, configFileName));
            }
            else
            {
                config = new Config();
                IO.WriteTextFile(configFolderPath, configFileName, config.ToJsonString());
            }
            if (IO.Exists(configFolderPath, xPathConfigFileName))
            {
                xPath = new Parser.XPath(IO.ReadTextFile(configFolderPath, xPathConfigFileName));
            }
            else
            {
                xPath = new Parser.XPath();
                IO.WriteTextFile(configFolderPath, xPathConfigFileName, xPath.ToJsonString());
            }
            Downloader.SetConfig(config);

            Downloader.ProgressChangedEvent += PrintProgess;

            Parser.Parser.Instance.SetXPath(xPath);
            Command command = new Command();
            var commands = command.GetCommandList(); ;
            cursorPosition = Console.CursorTop;
            IO.Print(" 명령어   : download [$$titleId$green$] 웹툰을 다운로드합니다. / [$$titleId$green$] : 다운로드할 웹툰의 $$titleId$green$입니다. ");
            IO.Print("            예) download $$20853$green$ ");
            IO.Print("                download $$183559$green$ $$20853$green$ $$703846$green$ ");
            IO.Print("");
            IO.Print("            get [$$weekday$green$] 선택한 요일(mon/tue/wed/thu/fri/sat/sun)의 웹툰 목록을 불러옵니다.");
            IO.Print("            예) get $$mon$green$ ");
            IO.Print(" \r\n 키보드의 ↑ ↓ 버튼으로 이전에 입력했던 명령어를 불러올 수 있습니다. ");
            IO.Print(new string('-', 100));
            cursorPosition = Console.CursorTop;
            while (true)
            {
                Console.Write("[Command] : ");
                string userInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    IO.PrintError("명령어를 입력해주세요.");
                    continue;
                }
                string[] split = userInput.Trim().Split(' ');
                List<string> list = new List<string>(split);
                if (command.Contains(list[0]))
                {
                    list.RemoveAt(0);
                    command.Start(split[0], list.ToArray());
                }
                else
                {
                    IO.PrintError("존재하지 않는 명령어입니다.");
                }
            }
        }
        public static void CheckUpdate(string[] assemsplit)
        {
            try
            {
                WebClient webClient = new WebClient();
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(webClient.DownloadString("https://wr-rainforest.github.io/Naver-Webtoon-Downloader/info/latest_version.html"));
                string latestVersion = document.DocumentNode.SelectSingleNode("/html/body").InnerText;

                string[] ltsplit = latestVersion.Split('.');
                var version = $"{ltsplit[0]}.{ltsplit[1]}";
                var build = $"{ltsplit[2]}.{ltsplit[3]}";
                if (int.Parse(ltsplit[0]) <= int.Parse(assemsplit[0]))
                {
                    return;
                }
                if (int.Parse(ltsplit[1]) <= int.Parse(assemsplit[1]))
                {
                    return;
                }
                if (int.Parse(ltsplit[2]) <= int.Parse(assemsplit[2]))
                {
                    return;
                }
                IO.Print("");
                IO.Print($"*$$새로운 버전이 출시되었습니다. v{version} (빌드 {build})$cyan$");
                IO.Print($"*$$업데이트 다운로드 : https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases/tag/v{latestVersion}$cyan$");
                IO.Print("");
            }
            catch
            {
                IO.Print(" $$버전 업데이트를 확인할 수 없습니다.$red$");
            }
        }
        public static void PrintProgess(string ProgressText)
        {
            int currentPosition = Console.CursorTop;
            Console.SetCursorPosition(0, currentPosition);
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            
            IO.Print(ProgressText, false, true);//i = 0;
            Console.SetCursorPosition(0, currentPosition);
        }
    }
}
