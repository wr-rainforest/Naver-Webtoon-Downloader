using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            Console.WriteLine("Program Start");
            IO.Log = Log;
            NaverWebtoonDownloader nwd = new NaverWebtoonDownloader();
            nwd.Download(args[0]);
        }
        static void Log(string msg)
        {

        }
    }
}
