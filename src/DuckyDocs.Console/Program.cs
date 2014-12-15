using DuckyDocs.SiteBuilder;
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
            [ArgDescription("The target assemblies to create API documentation for.")]
            public string TargetAssemblies { get; set; }

            [ArgDescription("Locations of XML API documentation files.")]
            public string XmlDocFiles { get; set; }

            [ArgDescription("Disable the splash message")]
            [DefaultValue(false)]
            public bool NoSplash { get; set; }

            [HelpHook]
            public bool Help { get; set; }

            public SiteBuilderRequest CreateBuilderRequest()
            {
                var result = new SiteBuilderRequest();
                return result;
            }
        }

        static int Main(string[] args)
        {
            var parsedArgs = Args.Parse<ProgramArgs>(args);
            if (parsedArgs == null)
            {
                return (int)ExitCodes.QuackQuackQuackQuackQuack;
            }

            if (!parsedArgs.NoSplash)
            {
                new Splash().Print();
            }

            return (int)ExitCodes.LuckyDuck;
        }
    }
}
