using Newtonsoft.Json;
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
        ConsolePage ConsolePage;
        public Command()
        {
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
                IO.Print(string.Format("{0} : {0}($${1}$cyan$)", args[i], title));
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
                    if(latest!=last)
                    {
                        Downloader.UpdateWebtoonInfo(webtoonInfo,null);
                    }
                }
                else
                {
                    webtoonInfo = new WebtoonInfo(keys[i], titles[i]);
                    Downloader.BuildWebtoonInfo(webtoonInfo, null);
                }
                ImageKey[] imageKeys = Downloader.BuildImageKeysToDown(webtoonInfo);
                Downloader.Download(webtoonInfo, imageKeys, null);
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
        }
        private void Clear(params string[] args)
        {
            /*            int currentPosition = Console.CursorTop;
                        Console.SetCursorPosition(0, Program.cursorPosition);
                        Console.Write(new string(' ', (Console.BufferWidth - 1) * (currentPosition-Program.cursorPosition)));
                        Console.SetCursorPosition(0, Program.cursorPosition);*/
        }
    }
}
