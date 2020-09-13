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
        public Command()
        {
            commandDictionary = new Dictionary<string, CommandDelegate>();
            commandDictionary.Add("get", Get);
            commandDictionary.Add("clear", Clear);
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
        }
        private void Clear(params string[] args)
        {
            int currentPosition = Console.CursorTop;
            Console.SetCursorPosition(0, Program.cursorPosition);
            Console.Write(new string(' ', (Console.BufferWidth - 1) * (currentPosition-Program.cursorPosition)));
            Console.SetCursorPosition(0, Program.cursorPosition);
        }
    }
}
