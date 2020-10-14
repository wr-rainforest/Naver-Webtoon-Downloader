using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WRforest.NWD
{
    class IO
    {
        private static Dictionary<string, ConsoleColor> Color;
        static IO()
        {
            Color = new Dictionary<string, ConsoleColor>();
            Color.Add("black", ConsoleColor.Black);
            Color.Add("darkblue", ConsoleColor.DarkBlue);
            Color.Add("darkgreen", ConsoleColor.DarkGreen);
            Color.Add("darkcyan", ConsoleColor.DarkCyan);
            Color.Add("darkred", ConsoleColor.DarkRed);
            Color.Add("darkmagenta", ConsoleColor.DarkMagenta);
            Color.Add("darkyellow", ConsoleColor.DarkYellow);
            Color.Add("gray", ConsoleColor.Gray);
            Color.Add("darkgray", ConsoleColor.DarkGray);
            Color.Add("blue", ConsoleColor.Blue);
            Color.Add("green", ConsoleColor.Green);
            Color.Add("cyan", ConsoleColor.Cyan);
            Color.Add("red", ConsoleColor.Red);
            Color.Add("magenta", ConsoleColor.Magenta);
            Color.Add("yellow", ConsoleColor.Yellow);
            Color.Add("white", ConsoleColor.White);

        }
        /// <summary>
        /// $$텍스트$색$  = >  텍스트를 지정한 색으로 출력합니다.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="newline"></param>
        /// <param name="timeStamp"></param>
        public static void Print(string msg, bool newline=true, bool timeStamp=false)
        {
            if (timeStamp)
                Console.Write(DateTime.Now.ToString("[yyyy-MM-dd hh:mm:ss] : "));

            string[] split = msg.Split('$');

            if(split.Length<=1)
            {
                if (newline)
                    Console.WriteLine(msg);
                else
                    Console.Write(msg);
                return;
            }
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i]) && ((i + 2) < split.Length&& Color.ContainsKey(split[i + 2].ToLower())))
                {
                    Console.ForegroundColor = Color[split[i + 2].ToLower()];
                    Console.Write(split[i + 1]);
                    Console.ResetColor();
                    i += 2;
                    continue;
                }
                Console.Write(split[i]);
            }
            if (newline)
                Console.WriteLine();
        }

        public static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error : " + msg);
            Console.ResetColor();
        }

        public static string ReadTextFile(string directory, string filename)
        {
            string buff;
            using (FileStream fs = new FileStream(string.Format("{0}\\{1}", directory, filename), FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                    buff = sr.ReadToEnd();
            }
            return buff;
        }
        public static void WriteTextFile(string directory, string filename, string data)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            using (FileStream fs = new FileStream(string.Format("{0}\\{1}", directory, filename), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(data);
            }
        }
        /*
        /// <summary>
        /// <paramref name="directory"/>의 <paramref name="filename"/>에 <paramref name="data"/>를 Write합니다. 이미 있는 파일이라면 덮어쓰고, <paramref name="directory"/>가 존재하지 않는다면 생성합니다 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        public static void WriteAllBytes(string directory, string filename, byte[] data)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            //File.WriteAllBytes(string.Format("{0}\\{1}", directory, filename), data);
            fileQueue.Enqueue((string.Format("{0}\\{1}", directory, filename), data));

        }*/
        /// <summary>
        /// 해당 <paramref name="directory"/>에 <paramref name="filename"/>이 존재하는지 확인합니다.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool Exists(string directory, string filename)
        {
            return File.Exists(string.Format("{0}\\{1}", directory, filename));
        }

    }
}
