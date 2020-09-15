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
            Downloader.SetProgressDelegate(PrintProgess);
            Parser.Parser.Instance.SetXPath(xPath);
            Command command = new Command();
            var commands = command.GetCommandList(); ;
            cursorPosition = Console.CursorTop;

            while (true)
            {
                IO.Print("[$$Command$cyan$] : ",false);
                int startPosition = Console.CursorLeft;
                int functionCommandTextEndPosition=0;
                bool functionCommandTextEnd=false;
                string userInput;
                List<char> list = new List<char>();
                Console.ForegroundColor = ConsoleColor.Yellow;
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == '\r')
                    {
                        Console.WriteLine();
                        break;
                    }
                    else if (key.KeyChar == '\b')
                    {
                        if (Console.CursorLeft == startPosition)
                            continue;
                        list.RemoveAt(list.Count - 1);
                        Console.Write("\b \b");
                        if (functionCommandTextEndPosition >= Console.CursorLeft)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            functionCommandTextEnd = false;
                            functionCommandTextEndPosition = 0;
                        }
                        continue;
                    }
                    else if (key.KeyChar == ' ' && !functionCommandTextEnd && !string.IsNullOrWhiteSpace(new string(list.ToArray()))) 
                    {
                        functionCommandTextEndPosition = Console.CursorLeft;
                        functionCommandTextEnd = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        list.Add(' ');
                        Console.Write(' ');
                    }
                    else
                    {
                        list.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
                userInput = new string(list.ToArray());
                Console.ResetColor();

                //userInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    IO.PrintError("명령어를 입력해주세요.");
                    continue;
                }
                string[] split = userInput.Trim().Split(' ');
                List<string> argslist = new List<string>(split);
                if (command.Contains(argslist[0]))
                {
                    argslist.RemoveAt(0);
                    command.Start(split[0], argslist.ToArray());
                }
                else
                {
                    IO.PrintError(string.Format("존재하지 않는 명령어입니다. : \"{0}\"", userInput));
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
