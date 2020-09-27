using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Command
{
    class Get : Command
    {
        string[] days = { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };
        Agent agent;
        Parser parser;
        public Get(Config config) : base(config)
        {
            agent = new Agent();
            parser = new Parser(agent);
        }
        public override void Start(params string[] args)
        {
            if (args.Length == 0)
            {
                IO.PrintError("요일을 지정해주세요. ");
                return;
            }
            if (!days.Contains(args[0]))
            {
                IO.PrintError(string.Format("입력된 문자열은 요일이 아닙니다. : \"{0}\"", args[0]));
                return;
            }
            agent.LoadPage("https://comic.naver.com/webtoon/weekday.nhn");
            var t = parser.GetWebtoonList(args[0]);
            int maxByteLength = 0;
            for (int i = 0; i < t.Length; i++)
            {
                int byteLength = Encoding.Default.GetBytes(string.Format("{0}({1})", t[i].title, t[i].titleId)).Length;
                if (byteLength > maxByteLength)
                {
                    maxByteLength = byteLength;
                }
            }
            for (int i = 0, j = 0; i < t.Length; i++, j++)
            {
                IO.Print(string.Format("{0}($${1}$cyan$)", t[i].title, t[i].titleId), false, false);
                if (j == 3)
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    j = -1;
                }
                else
                {
                    int byteLength = Encoding.Default.GetBytes(string.Format("{0}({1})", t[i].title, t[i].titleId)).Length;
                    Console.Write("".PadRight(maxByteLength - byteLength + 1, ' '));

                }
            }
            Console.WriteLine("");
        }
    }
}
