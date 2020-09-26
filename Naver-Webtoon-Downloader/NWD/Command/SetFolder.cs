using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Command
{
    class SetFolder : Command
    {
        public SetFolder(Config config):base(config)
        {

        }

        public override void Start(params string[] args)
        {
            var path = string.Join("",args);
            if (System.IO.Directory.Exists(path))
            {
                config.DefaultDownloadDirectory = path;
                IO.Print($"$${config.DefaultDownloadDirectory}$green$를 기본 다운로드 폴더로 설정합니다.");
                IO.Print($"해당 폴더에 접근 권한이 없을 경우 다운로드 도증 오류가 발생합니다.");
                IO.Print($"설정을 초기화하려면 프로그램 실행파일이 있는 폴더의 Config폴더를 삭제하세요.");
            }
            else
            {
                System.IO.DirectoryInfo info;
                try
                {
                    IO.Print($"존재하지 않는 폴더입니다.  생성합니다");
                    info = System.IO.Directory.CreateDirectory(path);
                }
                catch(Exception e)
                {
                    IO.Print(e.Message);
                    IO.Print("실패하였습니다. 경로가 잘못되었거나 접근 권한이 존재하지 않습니다.");
                    return;
                }

                config.DefaultDownloadDirectory = info.FullName;
                IO.Print($"$${config.DefaultDownloadDirectory}$green$를 기본 다운로드 폴더로 설정합니다.");
            }
            Console.Write(" ");
            int currpos = Console.CursorTop;
            Console.SetCursorPosition(0, Program.DFDFcursorPosition);
            IO.Print($"            현재 기본 다운로드 폴더$${config.DefaultDownloadDirectory}$green$\r\n\r\n");
            Console.SetCursorPosition(0, currpos);
            Console.WriteLine();
            IO.WriteTextFile("Config", "config.json", config.ToJsonString());
        }
    }
}
