using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ColoredConsole;
using DuckyDocs.CodeDoc;
using DuckyDocs.CRef;
using DuckyDocs.SiteBuilder;
using DuckyDocs.XmlDoc;
using Fclp;
using Humanizer;

namespace DuckyDocs.Console
{
    public class Program
    {

        public class ProgramArgs
        {
            public static ProgramArgs Parse(string[] args)
            {
                var result = new ProgramArgs();
                var parser = new FluentCommandLineParser();
                parser.Setup<List<string>>('a', "apitargets")
                    .Callback(items => result.TargetAssemblies = items)
                    .WithDescription("Assemblies to create documentation for.");
                parser.Setup<string>('t', "templates")
                    .Callback(text => result.TemplatesDirectory = text)
                    .WithDescription("Folder containing razor templates.");
                parser.Setup<List<string>>('x', "apixml")
                    .Callback(items => result.XmlDocLocations = items)
                    .WithDescription("XML doc files and folders containing them.");
                parser.Setup<bool>('s', "nosplash")
                    .Callback(b => result.NoSplash = b)
                    .WithDescription("Enable to hide the splash message.");
                parser.Setup<string>('o', "output")
                    .Callback(text => result.OutputFolder = text)
                    .WithDescription("The folder to output the resulting files into.");
                parser.Setup<List<string>>('d', "docs")
                    .Callback(items => result.DocsSources = items)
                    .WithDescription("The documentation sources to convert to HTML.");
                parser.SetupHelp("?", "help")
                    .Callback(help => System.Console.WriteLine(help));
                parser.Parse(args);
                return result;
            }

            public List<string> TargetAssemblies { get; set; }

            public List<string> XmlDocLocations { get; set; }

            public string OutputFolder { get; set; }

            public List<string> DocsSources { get; set; }

            public string TemplatesDirectory { get; set; }

            public bool NoSplash { get; set; }
        }

        static int Error(string message)
        {
            ColorConsole.WriteLine(("[Error]: " + message).Red());
            return (int)ExitCodes.QuackQuackQuaaack;
        }

        static int Main(string[] args)
        {
            var parsedArgs = ProgramArgs.Parse(args);
            if (parsedArgs == null)
            {
                return (int)ExitCodes.QuackQuackQuaaack;
            }

            if (!parsedArgs.NoSplash)
            {
                new Splash().Print();
            }

            if (parsedArgs.OutputFolder == null)
            {
                return Error("output folder is required.");
            }

            var outputDirectory = new DirectoryInfo(parsedArgs.OutputFolder);
            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
                Thread.Sleep(100);
            }

            if (parsedArgs.DocsSources != null && parsedArgs.DocsSources.Count > 0)
            {
                var converter = new StaticPageConverter();
                foreach (var docSource in parsedArgs.DocsSources)
                {
                    var request = new StaticPageConverterRequest {
                         Recursive = true,
                         RelativeDestination = outputDirectory.FullName,
                         Source = docSource
                    };
                    var result = converter.Convert(request).ToList();

                    ColorConsole.WriteLine("[Complete]: ".Green(), "batch content file".ToQuantity(result.Count));
                }
            }

            if (parsedArgs.TargetAssemblies != null && parsedArgs.TargetAssemblies.Count > 0)
            {
                var targetAssemblies = parsedArgs.TargetAssemblies
                    .Select(f => Assembly.ReflectionOnlyLoadFrom(new FileInfo(f).FullName))
                    .ToList();
                var xmlFiles = (parsedArgs.XmlDocLocations ?? Enumerable.Empty<string>())
                    .SelectMany(loc => {
                        var fi = new FileInfo(loc);
                        if (fi.Exists)
                        {
                            return new[] { fi };
                        }

                        var di = new DirectoryInfo(loc);
                        if (di.Exists)
                        {
                            return di.EnumerateFiles("*.xml");
                        }

                        return Enumerable.Empty<FileInfo>();
                    })
                    .Select(fi => new XmlAssemblyDocument(fi.FullName))
                    .ToList();

                var repository = new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(targetAssemblies),
                    xmlFiles);

                var supportRepository = new MsdnCodeDocMemberRepository();

                var apiOutputDirectory = new DirectoryInfo(Path.Combine(parsedArgs.OutputFolder, "api"));
                if (!apiOutputDirectory.Exists)
                {
                    apiOutputDirectory.Create();
                    Thread.Sleep(100);
                }

                var generator = new StaticApiPageGenerator {
                    OutputDirectory = apiOutputDirectory,
                    TemplateDirectory = new DirectoryInfo(parsedArgs.TemplatesDirectory ?? "./"),
                    TargetRepository = repository,
                    SupportingRepository = supportRepository
                };

                var results = generator.GenerateForAllTargets().ToList();

                ColorConsole.WriteLine("[Complete]: ".Green(), "api doc file".ToQuantity(results.Count));
            }

            return (int)ExitCodes.LuckyDuck;
        }
    }
}
