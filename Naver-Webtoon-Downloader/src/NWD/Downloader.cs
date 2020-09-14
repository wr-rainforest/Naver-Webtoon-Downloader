using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD
{
    class Downloader
    {
        public delegate void ProgressDelegate(string progress);
        private static ProgressDelegate PrintProgress;
        public static void SetProgressDelegate(ProgressDelegate progressDelegate)
        {
            PrintProgress = progressDelegate;
        }

        public static void UpdateWebtoonInfo(WebtoonInfo webtoonInfo, string ProgressTextFormat)
        {

        }
        public static void BuildWebtoonInfo(WebtoonInfo webtoonInfo, string ProgressTextFormat)
        {

        }
        public static ImageKey[] BuildImageKeysToDown(WebtoonInfo webtoonInfo, string ProgressTextFormat)
        {
            return null;
        }
        public static void Download(WebtoonInfo webtoonInfo, ImageKey[] imageKeys, string ProgressTextFormat)
        {

        }
    }
}
