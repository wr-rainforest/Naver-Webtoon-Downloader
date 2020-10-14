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
    class UpdateProgress : IProgress<object[]>
    {
        DownloadInfo DownloadInfo;

        public UpdateProgress(DownloadInfo downloadInfo)
        {
            DownloadInfo = downloadInfo;
        }

        public void Report(object[] value)
        {
            long position = (int)value[7];
            long count = (int)value[8];
            string date = (string)value[5];
            string episodeTitle = (string)value[6];
            DownloadInfo.LastEpisodeInfo = $"[{date}] {episodeTitle}";
            DownloadInfo.Progress = (double)(position+1) / count;
            DownloadInfo.ProgressText = string.Format("{0:P} ({1} / {2})", (double)(position) / count,(position), count);
        }
    
    }
}
