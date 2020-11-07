using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using NaverWebtoonDownloader.CoreLib;

namespace NaverWebtoonDownloader
{
    class DownloadStatusViewModel : INotifyPropertyChanged
    {
        private WebtoonInfo webtoonInfo;
        public string Title
        {
            get
            {
                return webtoonInfo.Title;
            }
        }
        public string TitleId
        {
            get
            {
                return webtoonInfo.TitleId;
            }
        }
        public string Writer
        {
            get
            {
                return webtoonInfo.Writer;
            }
        }

        private string latestEpisodeTitle;
        public string LatestEpisodeTitle
        {
            get
            {
                return latestEpisodeTitle;
            }
            set
            {
                latestEpisodeTitle = value;
                OnPropertyChanged();
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        private double progress;
        public double Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        private string progressMessage;
        public string ProgressMessage
        {
            get
            {
                return progressMessage;
            }
            set
            {
                progressMessage = value;
                OnPropertyChanged();
            }
        }

        private long size;
        public long Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
