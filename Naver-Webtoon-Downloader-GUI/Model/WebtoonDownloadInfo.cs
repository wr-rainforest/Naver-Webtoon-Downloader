using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WRforest.Model
{
    class WebtoonDownloadInfo : INotifyPropertyChanged
    {
        private Task task;
        public Task Task { get => task; set => task = value; }

        private CancellationTokenSource cancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource { get => cancellationTokenSource; set => cancellationTokenSource = value; }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private string writer;
        public string Writer
        {
            get
            {
                return writer;
            }
            set
            {
                writer = value;
                OnPropertyChanged();
            }
        }

        private string lastEpisodeInfo;
        public string LastEpisodeInfo
        {
            get
            {
                return lastEpisodeInfo;
            }
            set
            {
                lastEpisodeInfo = value;
                OnPropertyChanged();
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
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
        private string progressText;
        public string ProgressText
        {
            get
            {
                return progressText;
            }
            set
            {
                progressText = value;
                OnPropertyChanged();
            }
        }

        private string size;
        public string Size
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

        public string TitleId;
        public WebtoonDownloadInfo(string title, string titleId, string writer, string lastEpisodeInfo, string status, double progress, string progressText, string size)
        {
            Title = title;
            TitleId = titleId;
            Writer = writer;
            LastEpisodeInfo = lastEpisodeInfo;
            Status = status;
            Progress = progress;
            ProgressText = progressText;
            Size = size;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}