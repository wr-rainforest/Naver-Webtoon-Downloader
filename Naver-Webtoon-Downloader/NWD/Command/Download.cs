using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WRforest.NWD.DataType;
using WRforest.NWD.Key;

namespace WRforest.NWD.Command
{
    class Download : Command
    {
        Agent agent;
        Parser parser;

        public Download(Config config) : base(config)
        {
            agent = new Agent();
            parser = new Parser(agent);
        }

        public override void Start(params string[] args)
        {
            if (args.Length == 0)
            {
                IO.PrintError("titleId를 입력해주세요");
                return;
            }
            List<WebtoonKey> keys = new List<WebtoonKey>();
            List<string> titles = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (!int.TryParse(args[i], out _))
                {
                    IO.PrintError("titleId는 숫자입니다. : " + args[i]);
                    return;
                }
                agent.LoadPage(string.Format("https://comic.naver.com/webtoon/list.nhn?titleId={0}", args[i]));
                string title = parser.GetWebtoonTitle();
                if (title == "네이버 웹툰")
                {
                    IO.PrintError("존재하지 않는 titleId입니다. : " + args[i]);
                    return;
                }
                IO.Print(string.Format("{2}. {1}($${0}$cyan$) 대기..", args[i], title, i + 1), true, true);
                keys.Add(new WebtoonKey(args[i]));
                titles.Add(title);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                Downloader downloader;
                IO.Print("");
                WebtoonInfo webtoonInfo;

                if (IO.Exists("Cache", keys[i].TitleId + ".json"))
                {
                    webtoonInfo = JsonConvert.DeserializeObject<WebtoonInfo>(IO.ReadTextFile("Cache", keys[i].TitleId + ".json"));
                    downloader = new Downloader(webtoonInfo, config);
                    int latest = int.Parse(parser.GetLatestEpisodeNo());
                    int last = webtoonInfo.GetLastEpisodeNo();
                    IO.Print(string.Format("{2}. {0}($${1}$cyan$) URl 캐시를 불러왔습니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    IO.Print(string.Format("{2}. {0}($${1}$cyan$) 업데이트된 회차를 확인합니다.. ", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    if (latest != last)
                    {
                        IO.Print(string.Format("{4}. {0}($${1}$cyan$) URl 캐시를 업데이트합니다.. [no($${2}$cyan$) ~ no($${3}$cyan$)]", webtoonInfo.WebtoonTitle, keys[i].TitleId, last + 1, latest, i + 1), true, true);
                        downloader.UpdateWebtoonInfo((i + 1).ToString() + ". {0}($${1}$cyan$) [{2}/{3}] ($${4:P}$green$) [{5}]", progress);
                        Console.WriteLine();
                        IO.Print(string.Format("{2}. {0}($${1}$cyan$) URl 캐시에 업데이트된 회차를 추가하였습니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    }
                    else
                    {
                        IO.Print(string.Format("{2}. {0}($${1}$cyan$) 업데이트된 회차가 없습니다. ", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    }
                    var tuple = downloader.GetDownloadedImagesInformation();

                    if (tuple.downloadedImageCount != 0)
                    {
                        IO.Print(string.Format("{4}. {0}($${1}$cyan$) 이미 다운로드된 이미지 $${2}$cyan$장 ($${3:0.00}$blue$ MB)  ", webtoonInfo.WebtoonTitle, keys[i].TitleId, tuple.downloadedImageCount, (double)tuple.downloadedImagesSize / 1048576, i + 1), true, true);
                    }
                }
                else
                {
                    webtoonInfo = new WebtoonInfo(keys[i], titles[i]);
                    downloader = new Downloader(webtoonInfo, config);
                    IO.Print(string.Format("{2}. {0}($${1}$cyan$) URl 캐시를 생성합니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    downloader.UpdateWebtoonInfo((i + 1).ToString() + ". {0}($${1}$cyan$) [{2}/{3}] ($${4:P}$green$) [{5}]", progress);
                    Console.WriteLine(""); 
                    IO.Print(string.Format("{2}. {0}($${1}$cyan$) URl 캐시를 생성하였습니다..", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    
                }

                if (string.IsNullOrWhiteSpace(webtoonInfo.WebtoonWriter))
                {
                    EpisodeKey episodeKey = new EpisodeKey(keys[i].TitleId, 1);
                    agent.LoadPage(episodeKey.BuildUrl());
                    string webtoonWriter = parser.GetWebtoonWriter();
                    webtoonInfo.WebtoonWriter = webtoonWriter;
                }

                IO.WriteTextFile("Cache", keys[i].TitleId + ".json", JsonConvert.SerializeObject(webtoonInfo));

                ImageKey[] imageKeys = downloader.BuildImageKeysToDown();
                if (imageKeys.Length == 0)
                {
                    IO.Print(string.Format("{2}. {0}($${1}$cyan$) 모든 이미지가 다운로드되었습니다..추가로 다운로드할 이미지가 존재하지 않습니다.", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                    return;
                }
                IO.Print(string.Format("{2}. {0}($${1}$cyan$) 다운로드를 시작합니다. ", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
                var task = Task.Run(() => downloader.DownloadAsync(imageKeys, (i + 1).ToString() + ". {0}($${1}$cyan$) [{2}/{3}] ($${9:0.00}$blue$ MB) ($${4:P}$green$) [{5}]", progress));
                task.Wait();
                Console.WriteLine();
                IO.Print(string.Format("{2}. {0}($${1}$cyan$) 다운로드 완료", webtoonInfo.WebtoonTitle, keys[i].TitleId, i + 1), true, true);
            }

        }
    }
}
