using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD.Command
{
    abstract class Command
    {
        protected IProgress<string> progress;
        protected Config config;
        public Command(Config config)
        {
            this.config = config;
            progress = new Progress<string>(msg => PrintProgess(msg));
        }
        public abstract void Start(params string[] args);

        private void PrintProgess(string ProgressText)
        {
            Console.Write(" ");
            var curp = Console.CursorTop;
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            IO.Print(ProgressText, false, true);//i = 0;
            Console.SetCursorPosition(0, curp);
        }
    }
}
