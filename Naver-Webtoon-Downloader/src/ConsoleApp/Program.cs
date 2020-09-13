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
            IO.Log = Log;
            Command command = new Command();
            NaverWebtoonDownloader nwd = new NaverWebtoonDownloader();
            ConsolePage.PrintLine();
            ConsolePage.PrintMainPage("v0.3.0-alpha", ConsoleColor.Red);
            ConsolePage.PrintLine();
            ConsolePage.PrintConfigInfoPage(ConsoleColor.Cyan);
            ConsolePage.PrintLine();
            ConsolePage.PrintCommandInfoPage();
            ConsolePage.PrintLine();
            cursorPosition = Console.CursorTop;
            int positionCount = 0;
            Console.SetCursorPosition(0, cursorPosition);
            Console.Write(new string(' ', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, cursorPosition);
            while (true)
            {
                Console.Write(" Command : ");
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
            Console.Clear();
            ConsolePage.PrintMainPage("v0.3.0-alpha", ConsoleColor.Red);
            //nwd.Download(titleId);
            Console.WriteLine("종료하려면 아무 키나 누르십시오 . . .");
            Console.ReadKey();
        }
        static void Log(string msg)
        {

        }
    }
}
