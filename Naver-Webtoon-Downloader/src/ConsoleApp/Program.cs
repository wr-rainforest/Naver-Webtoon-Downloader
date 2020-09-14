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
        static void Main(string[] args)
        {
            Downloader.SetProgressDelegate(PrintProgess);
        }
        public static void PrintProgess(string ProgressText)
        {

        }
    }
}
