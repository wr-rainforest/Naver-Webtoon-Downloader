using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wr_rainforest
{
    /// <summary>
    /// InformationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow()
        {
            InitializeComponent();
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string[] assemsplit = assemblyVersion.Split('.');
            var version = $"{assemsplit[0]}.{assemsplit[1]}";
            var build = $"{assemsplit[2]}.{assemsplit[3]}";
            var Title = $"Naver-Webtoon-Downloader-GUI v{version} ({build})";
            this.Title = Title;
            InfoTextBox.Text = 
                $"Naver-Webtoon-Downloader-GUI v{version} (Build {build})\r\n" +
                $"Source : https://github.com/wr-rainforest/Naver-Webtoon-Downloader-GUI\r\n" +
                $"E-Mail : contact@wrforest.com" +
                $"\r\n" +
                $"" +
                Properties.Resources.License;
            LicenseTextBox.Text = Properties.Resources.OpenSourceLicence;
        }
    }
}
