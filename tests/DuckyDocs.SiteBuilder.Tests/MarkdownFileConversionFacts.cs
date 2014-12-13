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

        [Fact]
        public static void convert_empty_directory()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var sourceFolder = testFolder.Directory.CreateSubdirectory("source");
                var targetFolder = testFolder.Directory.CreateSubdirectory("target");
                var request = new StaticBuilderRequest
                {
                    Recursive = true,
                    Source = sourceFolder.FullName,
                    RelativeDestination = targetFolder.FullName
                };
                var converter = new StaticPageConverter();

                var resultingFiles = converter.Convert(request);

                resultingFiles.Should().BeEmpty();
            }
        }

        [Fact]
        public static void convert_nested_directories()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var sourceFolder = testFolder.Directory.CreateSubdirectory("source");
                var poopFolder = sourceFolder.CreateSubdirectory("poop");
                File.WriteAllText(Path.Combine(sourceFolder.FullName, "a.md"), "# poop");
                File.WriteAllText(Path.Combine(sourceFolder.FullName, "poop", "b.md"), "`poop`");
                var targetFolder = testFolder.Directory.CreateSubdirectory("target");
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

        [Fact]
        public static void convert_with_template()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var sourceFolder = testFolder.Directory.CreateSubdirectory("source");
                File.WriteAllText(Path.Combine(sourceFolder.FullName, "a.md"), "# poop");
                File.WriteAllText(
                    Path.Combine(sourceFolder.FullName, "_template.cshtml"),
                    @"<html>@Raw(Model.ContentHtml)</html>");
                var targetFolder = testFolder.Directory.CreateSubdirectory("target");
                var request = new StaticBuilderRequest
                {
                    Recursive = true,
                    Source = sourceFolder.FullName,
                    RelativeDestination = targetFolder.FullName
                };
                var converter = new StaticPageConverter();

                var resultingFiles = converter.Convert(request);
                var resultingFileText = File.ReadAllText(resultingFiles.Single().ResultFile.FullName);

                resultingFiles.Select(x => x.ResultFile.FullName).Should().OnlyContain(x => x == Path.Combine(targetFolder.FullName, "a.html"));
                resultingFileText.Should().StartWith("<html>");
                resultingFileText.Should().EndWith("</html>");
                resultingFileText.Should().Contain("<h1>poop</h1>");
            }
        }

    }
}
