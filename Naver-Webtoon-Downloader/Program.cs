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
            Console.WriteLine("program");
            IO.Log = Log;
            NaverWebtoonDownloader nwd = new NaverWebtoonDownloader();
        }
        static void Log(string msg)
        {

        }
    }
}
