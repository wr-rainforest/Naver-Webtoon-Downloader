using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD
{
    class Command
    {
        private delegate void CommandDelegate(params string[] args);
        Dictionary<string, CommandDelegate> commandDictionary;
        NaverWebtoonDownloader nwd;
        Parser.Agent agent;
        Parser.Parser parser;
        ConsolePage ConsolePage;
        public Command(Parser.Agent agent, Parser.Parser parser,NaverWebtoonDownloader nwd, ConsolePage consolePage)
        {
            this.agent = agent;
            this.parser = parser;
            this.nwd = nwd;
            this.ConsolePage = consolePage;
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

        string[] days = { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };
        private void Get(params string[] args)
        {
            if (args.Length==0)
            {
                ConsolePage.Error("요일을 지정해주세요. (get mon)");
                return;
            }
            if (!days.Contains(args[0]))
            {
                ConsolePage.Error("입력된 문자열은 요일이 아닙니다.");
                return;
            }
            ConsolePage.PrintWebtoonListPage(args[0]);
            Console.WriteLine();
        }
        private void Clear(params string[] args)
        {
            int currentPosition = Console.CursorTop;
            Console.SetCursorPosition(0, Program.cursorPosition);
            Console.Write(new string(' ', (Console.BufferWidth - 1) * (currentPosition-Program.cursorPosition)));
            Console.SetCursorPosition(0, Program.cursorPosition);
        }
        private void Download(params string[] args)
        {
            if (args.Length == 0)
            {
                ConsolePage.Error("titleId를 입력해주세요.");
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (!int.TryParse(args[0], out _))
                {
                    ConsolePage.Error("titleId는 숫자입니다. : " + args[i]);
                    return;
                }
                agent.LoadPage(string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", args[i]));
                var node = agent.Page.DocumentNode.SelectSingleNode("//*[@property=\"og:title\"]");
                if (node.Attributes["content"].Value == "네이버 웹툰")
                {
                    ConsolePage.Error("존재하지 않는 titleId입니다. : " + args[i]);
                    return;
                }
                ConsolePage.WriteLine(string.Format("{0}({1})", node.Attributes["content"].Value, args[i]));
            }
            for (int i = 0; i < args.Length; i++)
            {
                nwd.Download(args[i]);
            }
            Console.WriteLine();
        }
    }
}
