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
    class DownloadProgress : IProgress<object[]>
    {
        DownloadInfo DownloadInfo;

        public DownloadProgress(DownloadInfo downloadInfo)
        {
            DownloadInfo = downloadInfo;
        }

        public void Report(object[] value)
        {
            long position = (int)value[10];
            long count = (int)value[11];
            string date = (string)value[5];
            string episodeTitle = (string)value[6];
            DownloadInfo.Progress = (double)(position+1) / count;
            DownloadInfo.ProgressText = string.Format("{0:P} ({1} / {2})", (double)(position + 1) / count, (position + 1), count);
            DownloadInfo.Size = $"{(double)value[9]/1048576:0.00} MB"; 
        }
    }
}
