using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WRforest.NWD;
using WRforest.NWD.Command;

namespace WRforest.NWD
{
    class Program
    {
        public static int cursorPosition;
        public static int DFDFcursorPosition;
        static void Main(string[] args)
        { 
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] assemsplit = assemblyVersion.Split('.');
            var version = $"{assemsplit[0]}.{assemsplit[1]}"; 
            var build = $"{assemsplit[2]}.{assemsplit[3]}";
            var Title = $"네이버 웹툰 다운로더 v{version} ({build})";

            Console.Title = Title;


            string configFolderPath = "Config";
            string configFileName = "config.json";
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
            CommandManager command = new CommandManager(config);



            IO.Print($" 네이버 웹툰 다운로더 v{version} (빌드 {build})",false);
            CheckUpdate(assemblyVersion);
            IO.Print($" 홈페이지 : https://nwd.wrforest.com/");
            IO.Print($" 소스코드 : https://github.com/wr-rainforest/Naver-Webtoon-Downloader");
            IO.Print($" 연락처   : contact@wrforest.com");
            IO.Print(new string('-',100));
            IO.Print(" 명령어   : download [$$titleId$green$] 웹툰을 다운로드합니다. / (단축 명령어 : d)");
            IO.Print("            예) download $$20853$green$ ");
            IO.Print("                d $$183559$green$ $$20853$green$ ");
            IO.Print("            ) comic.naver.com/webtoon/list.nhn?titleId=$$20853$green$"); 
            IO.Print("");
            IO.Print("            get [$$weekday$green$] 선택한 요일(mon/tue/wed/thu/fri/sat/sun)의 웹툰 목록을 불러옵니다.");
            IO.Print("            예) get $$mon$green$ ");
            IO.Print("");
            //IO.Print("            merge [$$titleId$green$] 다운로드된 이미지를 하나의 파일로 병합합니다. / [$$titleId$green$] :병합할 웹툰의 $$titleId$green$입니다. ");
            //IO.Print("            예) merge $$20853$green$ ");
            //IO.Print("                merge $$183559$green$ $$20853$green$ $$703846$green$ ");
            //IO.Print("            주의사항)  ");
            //IO.Print("");
            IO.Print("            setfolder [$$folder$green$] / [$$folder$green$]를 기본 다운로드 폴더로 설정합니다.");
            IO.Print("            예) setfolder $$d\\webtoons$green$ ");
            DFDFcursorPosition = Console.CursorTop;
            IO.Print($"            현재 기본 다운로드 폴더 $${config.DefaultDownloadDirectory}$green$ ");
            IO.Print("");
            IO.Print(" \r\n 키보드의 ↑ ↓ 버튼으로 이전에 입력했던 값을 불러올 수 있습니다. 프로그램 종료시 초기화됩니다.");
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
        public static void CheckUpdate(string assemblyVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(webClient.DownloadString("https://wr-rainforest.github.io/Naver-Webtoon-Downloader/info/latest_version.html"));
                //document.LoadHtml(webClient.DownloadString("https://wr-rainforest.github.io/Naver-Webtoon-Downloader/Properties/version.latest.build.txt"));
                Version latestVersion = new Version(document.DocumentNode.SelectSingleNode("/html/body").InnerText);
                if (string.IsNullOrEmpty(document.DocumentNode.SelectSingleNode("/html/body").InnerText))
                    throw new Exception();
                Version currentVersion = new Version(assemblyVersion);
                int compareResult=currentVersion.CompareTo(latestVersion);
                if(compareResult<0)
                {
                    IO.Print("\r\n");
                    IO.Print($"*$$새로운 버전이 출시되었습니다. v{latestVersion.Major}.{latestVersion.Minor} (빌드 {latestVersion.Build}.{latestVersion.Revision})$cyan$");
                    IO.Print($"*$$업데이트 다운로드 : https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases/tag/v{latestVersion}$cyan$");
                    IO.Print("");
                }
                else if (compareResult == 0)
                {
                    IO.Print(" ($$최신 버전입니다.$blue$)");
                }
                else
                {
                    IO.Print(" ($$개발 버전$blue$)");
                }

            }
            catch
            {
                IO.Print("\r\n $$버전 업데이트를 확인할 수 없습니다.$red$");
            }
        }

    }
}
