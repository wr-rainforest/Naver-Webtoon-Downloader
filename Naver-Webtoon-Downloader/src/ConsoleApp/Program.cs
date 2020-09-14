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
            
            Parser.XPath xPath;
            Config config;
            
            if (IO.Exists("config", "config.json"))
            {
                config = new Config(IO.ReadTextFile("config", "config.json"));
            }
            else
            {
                config = new Config();
                IO.WriteTextFile("config", "config.json", config.ToJsonString());
            }
            if (IO.Exists("config", "xpath_config.json"))
            {
                xPath = new Parser.XPath(IO.ReadTextFile("config", "xpath_config.json"));
            }
            else
            {
                xPath = new Parser.XPath();
                IO.WriteTextFile("config", "xpath_config.json", xPath.ToJsonString());
            }
            Downloader.SetConfig(config);
            Downloader.SetProgressDelegate(PrintProgess);
            Parser.Parser.Instance.SetXPath(xPath);
            Command command = new Command();
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
        public static void PrintProgess(string ProgressText)
        {
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            IO.Print(ProgressText, false, true);
        }
    }
}
