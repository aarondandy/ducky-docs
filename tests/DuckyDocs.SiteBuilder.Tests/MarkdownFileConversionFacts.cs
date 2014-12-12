using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.Threading;


namespace DuckyDocs.SiteBuilder.Tests
{
    public static class MarkdownFileConversionFacts
    {
        private static DirectoryInfo PrepareDirectory(string path)
        {
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                directory.Delete(true);
                Thread.Sleep(10);
            }

            directory.Create();
            Thread.Sleep(10);
            return directory;
        }


        [Fact]
        public static void convert_empty_directory()
        {
            var sourceFolder = PrepareDirectory("./source");
            var targetFolder = PrepareDirectory("./target");
            var request = new StaticBuilderRequest {
                Recursive = true,
                Source = sourceFolder.FullName,
                RelativeDestination = targetFolder.FullName
            };
            var converter = new StaticPageConverter();

            var resultingFiles = converter.Convert(request);

            resultingFiles.Should().BeEmpty();
        }

        [Fact]
        public static void convert_nested_directories()
        {
            var sourceFolder = PrepareDirectory("./source");
            Directory.CreateDirectory(Path.Combine(sourceFolder.FullName, "poop"));
            File.WriteAllText(Path.Combine(sourceFolder.FullName, "a.md"), "# poop");
            File.WriteAllText(Path.Combine(sourceFolder.FullName, "poop", "b.md"), "`poop`");
            var targetFolder = PrepareDirectory("./target");
            var request = new StaticBuilderRequest
            {
                Recursive = true,
                Source = sourceFolder.FullName,
                RelativeDestination = targetFolder.FullName
            };
            var converter = new StaticPageConverter();

            var resultingFiles = converter.Convert(request);

            resultingFiles.Select(x => x.ResultFile.FullName).Should().Contain(Path.Combine(targetFolder.FullName, "a.html"));
            resultingFiles.Select(x => x.ResultFile.FullName).Should().Contain(Path.Combine(targetFolder.FullName, @"poop\b.html"));
        }

    }
}
