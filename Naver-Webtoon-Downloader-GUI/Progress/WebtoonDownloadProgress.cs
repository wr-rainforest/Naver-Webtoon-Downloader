using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WRforest.Model;
using WRforest.NWD;
using WRforest.View;

namespace WRforest.Progress
{
    class WebtoonDownloadProgress : IProgress<object[]>
    {
        WebtoonDownloadInfo DownloadInfo;

        public WebtoonDownloadProgress(WebtoonDownloadInfo downloadInfo)
        {
            DownloadInfo = downloadInfo;
        }

        public void Report(object[] value)
        {
            long position = (int)value[2];
            long count = (int)value[3];
            string date = (string)value[5];
            string episodeTitle = (string)value[6];
            DownloadInfo.LastEpisodeInfo = $"[{date}] {episodeTitle}";
            DownloadInfo.Progress = (double)(position) / count;
        }
    }
}
