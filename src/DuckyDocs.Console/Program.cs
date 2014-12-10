using PowerArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.Console
{
    public class Program
    {

        [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
        public class ProgramArgs
        {
            [ArgDescription("The configuration file to load")]
            public string Config { get; set; }

            [ArgDescription("Disable the splash message")]
            [DefaultValue(false)]
            public bool NoSplash { get; set; }

            [HelpHook]
            public bool Help { get; set; }
        }

        static void Main(string[] args)
        {
            var parsedArgs = Args.Parse<ProgramArgs>(args);
            if (parsedArgs == null)
            {
                return;
            }

            if (!parsedArgs.NoSplash)
            {
                new Splash().Print();
            }
        }
    }
}
