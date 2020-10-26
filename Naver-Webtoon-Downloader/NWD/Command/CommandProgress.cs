using System;

namespace WRforest.NWD.Command
{
    class CommandProgress : IProgress<string>
    {
        public void Report(string value)
        {
            Console.Write("\r" + new string(' ', Console.BufferWidth - 1) + "\r");
            IO.Print(value, false, true);//i = 0;
            Console.Write("\r");
        }
    }
}
