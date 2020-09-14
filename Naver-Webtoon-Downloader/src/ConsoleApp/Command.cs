﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD
{
    class Command
    {
        private delegate void CommandDelegate(params string[] args);
        Dictionary<string, CommandDelegate> commandDictionary;
        Parser.Agent agent;
        Parser.Parser parser;
        public Command()
        {
            agent = Parser.Agent.Instance;
            parser = Parser.Parser.Instance;
            commandDictionary = new Dictionary<string, CommandDelegate>();
            commandDictionary.Add("get", Get);
            commandDictionary.Add("clear", Clear);
            commandDictionary.Add("download", Download);
        }
        public bool Contains(string commandName)
        {
            return commandDictionary.ContainsKey(commandName);
        }
        public void Start(string commandName, params string[] args)
        {
            commandDictionary[commandName](args);
        }
        private void Download(params string[] args)
        {
            if (args.Length == 0)
            {
                IO.PrintError("titleId를 입력해주세요");
                IO.Print("$$download$yellow$ [$$titleId$cyan$] / [$$titleId$Cyan$] : 다운로드할 웹툰의 $$titleId$cyan$입니다.");
                IO.Print("                      : 예) $$download$yellow$ $$733766$cyan$ ", true, false);
                return;
            }
            List<WebtoonKey> keys = new List<WebtoonKey>();
            List<string> titles = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (!int.TryParse(args[0], out _))
                {
                    IO.PrintError("titleId는 숫자입니다. : " + args[i]);
                    return;
                }
                agent.LoadPage(string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", args[i]));
                string title = parser.GetWebtoonTitle();
                if (title == "네이버 웹툰")
                {
                    IO.PrintError("존재하지 않는 titleId입니다. : " + args[i]);
                    return;
                }
                IO.Print(string.Format("{1}($${0}$cyan$)", args[i], title));
                titles.Add(title);
                keys.Add(new WebtoonKey(args[i]));
            }
            for(int i=0; i < keys.Count; i++)
            {
                WebtoonInfo webtoonInfo;
                if(IO.Exists("Cache", keys[i].TitleId + ".json"))
                {
                    webtoonInfo = JsonConvert.DeserializeObject<WebtoonInfo>(IO.ReadTextFile("Cache", keys[i].TitleId + ".json"));
                    int latest = int.Parse(parser.GetLatestEpisodeNo());
                    int last = webtoonInfo.GetLastEpisodeNo();
                    IO.Print(string.Format("{0}($${1}$cyan$) 메타데이터 캐시를 불러왔습니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    IO.Print(string.Format("{0}($${1}$cyan$) 업데이트된 회차를 확인합니다.. ", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    if (latest!=last)
                    {
                        IO.Print(string.Format("{0}($${1}$cyan$) 메타데이터 캐시를 업데이트합니다.. [no($${2}$cyan$) ~ no($${3}$cyan$)]", webtoonInfo.WebtoonTitle, keys[i].TitleId, last+1,latest));
                        Downloader.UpdateWebtoonInfo(webtoonInfo, "{0}($${1}$cyan$) [{2}/{3}] ($${4:P}$green$) [{5}]");
                        IO.Print(string.Format("{0}($${1}$cyan$) 메타데이터 캐시에 업데이트된 회차를 추가하였습니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    }
                    else
                    {
                        IO.Print(string.Format("{0}($${1}$cyan$) 업데이트된 회차가 없습니다. ", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    }
                    var tuple = Downloader.GetDownloadedImagesInformation(webtoonInfo);
                    IO.Print(string.Format("{0}($${1}$cyan$) 다운로드를 시작합니다. ", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    if (tuple.downloadedImageCount != 0)
                    {
                        IO.Print(string.Format("{0}($${1}$cyan$) 이미 다운로드된 이미지 $${2}$cyan$장 ($${3:0.00}$blue$ MB)  ", webtoonInfo.WebtoonTitle, keys[i].TitleId, tuple.downloadedImageCount, (double)tuple.downloadedImagesSize/1048576));
                    }
                }
                else
                {
                    webtoonInfo = new WebtoonInfo(keys[i], titles[i]);
                    IO.Print(string.Format("{0}($${1}$cyan$) 메타데이터 캐시를 생성합니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                    Downloader.BuildWebtoonInfo(webtoonInfo, "{0}($${1}$cyan$) [{2}/{3}] ($${4:P}$green$) [{5}]");
                    IO.Print(string.Format("{0}($${1}$cyan$) 메타데이터 캐시를 생성하였습니다..", webtoonInfo.WebtoonTitle, keys[i].TitleId));
                }
                IO.WriteTextFile("Cache", keys[i].TitleId + ".json", JsonConvert.SerializeObject(webtoonInfo));

                ImageKey[] imageKeys = Downloader.BuildImageKeysToDown(webtoonInfo);
                Downloader.Download(webtoonInfo, imageKeys, "{0}($${1}$cyan$) [{2}/{3}] ($${9:0.00}$blue$ MB) ($${4:P}$green$) [{5}] {6}");
            }
            
        }
        string[] days = { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };
        private void Get(params string[] args)
        {
            if (args.Length == 0)
            {
                IO.PrintError("요일을 지정해주세요. (예 : get mon)");
                return;
            }
            if (!days.Contains(args[0]))
            {
                IO.PrintError("입력된 문자열은 요일이 아닙니다.");
                return;
            }
            agent.LoadPage("https://comic.naver.com/webtoon/weekday.nhn");
            var t = parser.GetWebtoonList(args[0]);
            int maxByteLength = 0;
            for (int i = 0; i < t.Length; i++)
            {
                int byteLength = Encoding.Default.GetBytes(string.Format("{0}({1})", t[i].title, t[i].titleId)).Length;
                if (byteLength > maxByteLength)
                {
                    maxByteLength = byteLength;
                }
            }
            for (int i = 0, j = 0; i < t.Length; i++, j++)
            {
                IO.Print(string.Format("{0}($${1}$cyan$)", t[i].title, t[i].titleId),false,false);
                if (j == 3)
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    j = -1;
                }
                else
                {
                    int byteLength = Encoding.Default.GetBytes(string.Format("{0}({1})", t[i].title, t[i].titleId)).Length;
                    Console.Write("".PadRight(maxByteLength - byteLength + 1, ' '));

                }
            }
            Console.WriteLine("");
        }
        private void Clear(params string[] args)
        {
            int currentPosition = Console.CursorTop;
            Console.SetCursorPosition(0, Program.cursorPosition);
            Console.Write(new string(' ', (Console.BufferWidth - 1) * (currentPosition - Program.cursorPosition)));
            Console.SetCursorPosition(0, Program.cursorPosition); 
        }
    }
}
