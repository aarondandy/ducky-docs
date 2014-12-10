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
            var duck = new List<string> {
                @"   _    ",
                @" -( )__ ",
                @" (____/ ",
                @"~~~~~~~~"};
            var version = typeof(Splash).Assembly.GetName().Version;
            duck.Add(version.ToString(3).PadLeft(duck.Last().Length));
            var duckColors = new ConsoleColor[] {
                ConsoleColor.Yellow,
                ConsoleColor.Yellow,
                ConsoleColor.Yellow,
                ConsoleColor.Cyan,
                ConsoleColor.DarkMagenta};
            var hugeText = new string[] {
                @" ____          _          ____              ",
                @"|    \ _ _ ___| |_ _ _   |    \ ___ ___ ___ ",
                @"|  |  | | |  _| '_| | |  |  |  | . |  _|_ -|",
                @"|____/|___|___|_,_|_  |  |____/|___|___|___|",
                @"                  |___|                     "};
            var textSpacing = "   ";

            for (int i = 0; i < duck.Count; i++)
            {
                ColorConsole.WriteLine(duck[i].Color(duckColors[i]), textSpacing, hugeText[i].Green());
            }

            ColorConsole.WriteLine();
        }
    }
}
