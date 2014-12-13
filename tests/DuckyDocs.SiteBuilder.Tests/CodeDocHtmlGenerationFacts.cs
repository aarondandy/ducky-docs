using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckyDocs.CodeDoc;
using DuckyDocs.CRef;
using DuckyDocs.Reflection;
using DuckyDocs.XmlDoc;
using FluentAssertions;
using Xunit;

namespace DuckyDocs.SiteBuilder.Tests
{
    public static class CodeDocHtmlGenerationFacts
    {

        private static ICodeDocMemberRepository CreateTestLibrary1Repository()
        {
            var testLib1Asm = typeof(TestLibrary1.Class1).Assembly;
            var testLib1AsmPath = testLib1Asm.GetFilePath();
            var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
            return new ReflectionCodeDocMemberRepository(
                new ReflectionCRefLookup(testLib1Asm),
                new XmlAssemblyDocument(testLib1XmlPath));
        }

        [Fact]
        public static void doc_page_delegate()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_delegate.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("T:TestLibrary1.Class1.MyFunc"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>MyFunc(Int32, Int32)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_event()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_event.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("E:TestLibrary1.Class1.DoStuff"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>DoStuff</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_field()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName,"_field.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest {
                    Member = repository.GetMemberModel(new CRefIdentifier("F:TestLibrary1.Class1.SomeNullableInt"))
                };
                var apiPageGen = new StaticApiPageGenerator {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>SomeNullableInt</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_method()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("M:TestLibrary1.Class1.DoubleStatic(System.Int32)"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>DoubleStatic(Int32)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_instance_constructor()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("M:TestLibrary1.Class1.#ctor(System.String)"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>Class1(String)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_operator()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>operator +(Class1, Class1)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_namespace()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_namespace.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("N:TestLibrary1"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>TestLibrary1</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_property()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_property.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("P:TestLibrary1.Class1.SomeProperty"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>SomeProperty</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_indexer()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_property.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("P:TestLibrary1.Class1.Item(System.Int32)"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>Item[Int32]</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_type()
        {
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_type.cshtml"),
                    @"</html><h1>@Model.ShortName</h1></html>");
                var repository = CreateTestLibrary1Repository();
                var request = new StaticApiPageRequest
                {
                    Member = repository.GetMemberModel(new CRefIdentifier("T:TestLibrary1.Class1"))
                };
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory
                };

                var result = apiPageGen.Generate(request);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"</html><h1>Class1</h1></html>");
            }
        }

    }
}
