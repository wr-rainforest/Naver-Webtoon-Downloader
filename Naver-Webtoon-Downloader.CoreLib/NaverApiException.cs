using System;
using System.Collections.Generic;
using System.Text;

namespace NaverWebtoonDownloader.CoreLib
{
    class NaverApiException : Exception
    {
        public string Code { get; }
        public NaverApiException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
