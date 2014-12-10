using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColoredConsole;

namespace DuckyDocs.Console
{
    public class Splash
    {
        public void Print()
        {
            var duck = new string[] {
                @"   _    ",
                @" -( )__ ",
                @" (____/ ",
                @"        ",
                @"~~~~~~~~"};
            var duckColors = new ConsoleColor[] {
                ConsoleColor.Yellow,
                ConsoleColor.Yellow,
                ConsoleColor.Yellow,
                ConsoleColor.Yellow,
                ConsoleColor.Cyan};
            var hugeText = new string[] {
                @" ____          _          ____              ",
                @"|    \ _ _ ___| |_ _ _   |    \ ___ ___ ___ ",
                @"|  |  | | |  _| '_| | |  |  |  | . |  _|_ -|",
                @"|____/|___|___|_,_|_  |  |____/|___|___|___|",
                @"                  |___|                     "};
            var textSpacing = "   ";

            for (int i = 0; i < duck.Length; i++)
            {
                ColorConsole.WriteLine(duck[i].Color(duckColors[i]), textSpacing, hugeText[i].Green());
            }

            var version = typeof(Splash).Assembly.GetName().Version;
            ColorConsole.WriteLine(String.Format("Version: {0}", version).DarkMagenta());
            ColorConsole.WriteLine();
        }
    }
}
