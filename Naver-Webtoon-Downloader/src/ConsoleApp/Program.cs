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
        public static int mainPageCursorPosition;
        static void Main(string[] args)
        {
            IO.Log = Log;
            Parser.XPath xPath;
            Config config;
            if (IO.Exists("data\\configs", "config.json"))
            {
                config = new Config(IO.ReadTextFile("data\\configs", "config.json"));
            }
            else
            {
                config = new Config();
                IO.WriteTextFile("data\\configs", "config.json", config.ToJsonString());
            }
            if (IO.Exists("data\\configs", "xpath_config.json"))
            {
                xPath = new Parser.XPath(IO.ReadTextFile("data\\configs", "xpath_config.json"));
            }
            else
            {
                xPath = new Parser.XPath();
                IO.WriteTextFile("data\\configs", "xpath_config.json", xPath.ToJsonString());
            }
            Parser.Agent agent = new Agent();
            Parser.Parser parser = new Parser.Parser(agent, xPath);
            NaverWebtoonDownloader nwd = new NaverWebtoonDownloader(agent, parser, config, xPath);
            ConsolePage ConsolePage = new ConsolePage(agent, parser);
            Command command = new Command(agent, parser, nwd, ConsolePage);

            ConsoleColor lineColor = ConsoleColor.Gray;
            int lineCharCount = 100;
            //ConsolePage.PrintLine('',lineCharCount, lineColor);
            ConsolePage.PrintMainPage(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), ConsoleColor.Blue);
            ConsolePage.PrintLine('-',lineCharCount, lineColor);
            mainPageCursorPosition = Console.CursorTop;
            ConsolePage.PrintConfigInfoPage(ConsoleColor.Yellow);
            ConsolePage.PrintLine('-',lineCharCount, lineColor);
            ConsolePage.PrintCommandInfoPage(ConsoleColor.Yellow, ConsoleColor.Green);
            ConsolePage.PrintLine('-',lineCharCount, lineColor);
            cursorPosition = Console.CursorTop;
            int positionCount = 0;
            Console.SetCursorPosition(0, cursorPosition);
            Console.Write(new string(' ', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, cursorPosition);
            while (true)
            {
                Console.Write("[Command] : ");
                string userInput = Console.ReadLine();
                positionCount++;
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    ConsolePage.Error("명령어를 입력해주세요.");
                    positionCount++;
                    continue;
                }
                string[] split = userInput.Trim().Split(' ');
                List<string> list = new List<string>(split);
                if(command.Contains(list[0]))
                {
                    list.RemoveAt(0);
                    command.Start(split[0], list.ToArray());
                }
                else
                {
                    ConsolePage.Error("존재하지 않는 명령어입니다.");
                }
            }
        }
        static void Log(string msg)
        {

        }
    }
}
