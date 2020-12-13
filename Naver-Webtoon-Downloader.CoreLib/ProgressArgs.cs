using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib
{
    public class ProgressArgs
    {
        public string StatusMessage { get; set; }

        public int Position { get; set; }

        public int Count { get; set; }

        public ProgressArgs(int position, int count, string msg)
        {
            Position = position;
            Count = count;
            StatusMessage = msg;
        }
    }
}
