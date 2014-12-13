using CommonMark;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class StaticPageConverter
    {
        public class TemplateModel
        {
            public string ContentHtml { get; set; }
        }

        public string DestinationRoot { get; set; }

        public IEnumerable<StaticPageBuilderResponse> Convert(StaticBuilderRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            DirectoryInfo requestSourceRoot;
            IEnumerable<FileInfo> sourceFiles;

            var requestSourceFile = new FileInfo(request.Source);
            if (requestSourceFile.Exists)
            {
                requestSourceRoot = requestSourceFile.Directory;
                sourceFiles = new[] { requestSourceFile };
            }
            else
            {
                requestSourceRoot = new DirectoryInfo(request.Source);
                if (requestSourceRoot.Exists)
                {
                    var directories = request.Recursive
                        ? SearchAllDirectories(requestSourceRoot)
                        : new[] { requestSourceRoot };
                    sourceFiles = directories.SelectMany(d => d.EnumerateFiles("*.md"));
                }
                else
                {
                    sourceFiles = Enumerable.Empty<FileInfo>();
                }
            }

            var templateFile = requestSourceRoot.EnumerateFiles("_template.cshtml").FirstOrDefault();
            string razorTemplateCacheKey = null;
            if (templateFile != null && templateFile.Exists)
            {
                razorTemplateCacheKey = templateFile.FullName;
                Razor.Compile(File.ReadAllText(templateFile.FullName), typeof(TemplateModel), razorTemplateCacheKey);
            }

            CommonMarkSettings settings = null;
            var sourceRootUri = new Uri(requestSourceRoot.FullName + Path.DirectorySeparatorChar);
            var results = new List<StaticPageBuilderResponse>();
            foreach (var sourceFile in sourceFiles)
            {
                var localFileUri = new Uri(sourceFile.FullName);
                var relativeSourceFilePath = sourceRootUri.MakeRelative(localFileUri);
                if (Path.DirectorySeparatorChar != '/')
                {
                    relativeSourceFilePath = relativeSourceFilePath.Replace('/', Path.DirectorySeparatorChar);
                }

                var relativeTargetFilePath = Path.ChangeExtension(relativeSourceFilePath, "html");
                var targetFile = new FileInfo(Path.Combine(request.RelativeDestination, relativeTargetFilePath));

                CommonMark.Syntax.Block parsedDocument;
                using (var sourceStream = File.Open(sourceFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(sourceStream))
                {
                    parsedDocument = CommonMarkConverter.ProcessStage1(reader, settings);
                }

                CommonMarkConverter.ProcessStage2(parsedDocument, settings);

                if (!targetFile.Directory.Exists)
                {
                    targetFile.Directory.Create();
                }

                using (var targetStream = File.Open(targetFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var writer = new StreamWriter(targetStream))
                {
                    if (razorTemplateCacheKey != null)
                    {
                        var markdownText = new StringBuilder();
                        using (var markdownWriter = new StringWriter(markdownText))
                        {
                            CommonMarkConverter.ProcessStage3(parsedDocument, markdownWriter, settings);
                        }
                        var htmlResultText = Razor.Run(razorTemplateCacheKey, new TemplateModel{
                            ContentHtml = markdownText.ToString()
                        });
                        writer.Write(htmlResultText);
                    }
                    else
                    {
                        CommonMarkConverter.ProcessStage3(parsedDocument, writer, settings);
                    }
                }

                results.Add(new StaticPageBuilderResponse
                {
                    ResultFile = targetFile
                });
            }

            return results;
        }

        private IEnumerable<DirectoryInfo> SearchAllDirectories(DirectoryInfo sourceDirectory)
        {
            Contract.Requires(sourceDirectory != null);

            var searchStack = new Stack<DirectoryInfo>();
            searchStack.Push(sourceDirectory);

            while (searchStack.Count > 0)
            {
                var searchDirectory = searchStack.Pop();
                yield return searchDirectory;

                foreach (var directory in searchDirectory.GetDirectories())
                {
                    searchStack.Push(directory);
                }
            }
        }
    }
}
