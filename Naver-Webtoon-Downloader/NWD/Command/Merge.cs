using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD.Command
{
    class Merge : Command
    {
        public Merge(Config config) : base(config)
        {
        }

        public override void Start(params string[] args)
        {
            if (args.Length == 0)
            {
                IO.PrintError("titleId를 입력해주세요");
                return;
            }
            if (!int.TryParse(args[0], out _))
            {
                IO.PrintError("titleId는 숫자입니다. : " + args[0]);
                return;
            }
            if (!IO.Exists("Cache", args[0] + ".json"))
            {
                IO.Print($"Cache/{args[0]}.json 파일을 찾을 수 없습니다.") ;
                IO.Print($"Merge는 이미 다운로드된 이미지를 합쳐서 새로운 파일로 저장하는 커맨드입니다.");
                IO.Print($"download $${args[0]}$green$ 로 먼저 이미지를 다운로드 한 후에 merge $${ args[0]}$green$ 을/를 사용해 주세요.");
                IO.Print($"이미 다운로드된 웹툰임에도 이 메세지가 나타난다면 download $${args[0]}$green$ 로 캐시파일 생성 후에 merge $${ args[0]}$green$ 을/를 사용해 주세요.");
                return;
            }
            var webtoonInfo = JsonConvert.DeserializeObject<WebtoonInfo>(IO.ReadTextFile("Cache", args[0] + ".json"));
            var downloader = new Downloader(webtoonInfo, config);
            int last = webtoonInfo.GetLastEpisodeNo();
            IO.Print(string.Format("{0}($${1}$cyan$) 기본 다운로드 폴더 $${2}$green$ 에서 병합할 파일 확인..", webtoonInfo.WebtoonTitle, args[0],config.DefaultDownloadDirectory), true, true);
            var tuple = downloader.GetDownloadedImagesInformation();
            if(tuple.downloadedImageCount!=webtoonInfo.GetImageCount())
            {
                IO.Print(string.Format("{0}($${1}$cyan$) 누락된 이미지 $${2}$blue$장이 존재합니다. download $${1}$green$ 로 다운로드하세요.  ", webtoonInfo.WebtoonTitle, args[0], tuple.downloadedImageCount, webtoonInfo.GetImageCount(), true, true));
                return;
            }
            IO.Print(string.Format("{0}($${1}$cyan$) 총 $${2}$cyan$장 ($${3:0.00}$blue$ MB) 병합을 시작합니다.  ", webtoonInfo.WebtoonTitle, args[0], tuple.downloadedImageCount, (double)tuple.downloadedImagesSize / 1048576), true, true);

            FileNameBuilder fileNameBuilder = new FileNameBuilder(webtoonInfo, config);
            var episodeNoList = webtoonInfo.Episodes.Keys.ToArray();
            string webtoonDirectory = config.DefaultDownloadDirectory + "\\" + fileNameBuilder.BuildWebtoonDirectoryName(new WebtoonKey(args[0])) + " - merged";
            if (!Directory.Exists(webtoonDirectory))
            {
                Directory.CreateDirectory(webtoonDirectory);
            }
            else
            {

            }
            for (int i = 0; i < episodeNoList.Length; i++)
            {
                var imageCount = webtoonInfo.Episodes[episodeNoList[i]].EpisodeImageUrls.Length;
                Key.EpisodeKey episodeKey = new Key.EpisodeKey(args[0], episodeNoList[0]);
                string episodeFilePath = webtoonDirectory + "\\" + fileNameBuilder.BuildEpisodeDirectoryName(episodeKey)+".jpg" ;
                if (File.Exists(episodeFilePath))
                {
                    continue;
                }
                progress.Report(string.Format("{0}($${1}$cyan$ $${2}$cyan$ 장 병합 )", webtoonInfo.WebtoonTitle, args[0], webtoonInfo.Episodes[episodeNoList[i]].EpisodeImageUrls.Length+1));
                if (imageCount==1)
                {
                    File.Copy(fileNameBuilder.BuildImageFileFullPath(new ImageKey(args[0], episodeNoList[i], 0)), episodeFilePath, true) ;
                    continue;
                }
                List<string> webtoonImagePathList = new List<string>();
                for (int j = 0; j < imageCount; j++)
                {
                    webtoonImagePathList.Add(fileNameBuilder.BuildImageFileFullPath(new ImageKey(args[0], episodeNoList[i], j)));
                }
                byte[]buff = MergeImages(webtoonImagePathList);
                File.WriteAllBytes(episodeFilePath, buff);
            }

        }
        private byte[] MergeImages(List<string> webtoonImagePathList)
        {
            ImageConverter imageConverter = new ImageConverter();
            List<Bitmap> images = new List<Bitmap>();
            for(int i=0;i< webtoonImagePathList.Count;i++)
            {
                images.Add(new Bitmap(webtoonImagePathList[i]));
            }
            int width = images.First().Width;
            int height = 0;
            for (int i = 0; i < images.Count; i++)
            {
                height += images[i].Height;
            }
            Bitmap bitmap = new Bitmap(width, height);
            bitmap.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                height = 0;
                for (int i = 0; i < images.Count; i++)
                {
                    Bitmap image = images[i];
                    image.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution);
                    g.DrawImage(image, 0, height);
                    height += image.Height;
                    images[i].Dispose();//램
                }
            }
            return (byte[])imageConverter.ConvertTo(bitmap, typeof(byte[]));
        }
        private byte[] MergeImages(List<byte[]> webtoonimages)
        {
            ImageConverter imageConverter = new ImageConverter();
            List<Bitmap> images = new List<Bitmap>();
            while(webtoonimages.Count==0)
            {
                images.Add(new Bitmap((Image)imageConverter.ConvertFrom(webtoonimages[0])));
            }
            int width = images.First().Width;
            int height = 0;
            for (int i = 0; i < images.Count; i++)
            {
                height += images[i].Height;
            }
            Bitmap bitmap2 = new Bitmap(width, height);
            bitmap2.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution); // <-- Set explicit resolution on bitmap2
                                                                                                 // Always put Graphics objects in a 'using' block.
            using (Graphics g = Graphics.FromImage(bitmap2))
            {
                height = 0;
                for (int i = 0; i < images.Count; i++)
                {
                    Bitmap image = images[i];
                    image.SetResolution(images[0].HorizontalResolution, images[0].VerticalResolution); // <-- Set resolution equal to bitmap2
                    g.DrawImage(image, 0, height);
                    height += image.Height;
                    images[i].Dispose();//램? dispose?
                }
            }
            return (byte[])imageConverter.ConvertTo(bitmap2, typeof(byte[]));
        }
    }
}
