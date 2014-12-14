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
                RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_delegate.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("T:TestLibrary1.Class1.MyFunc"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>MyFunc(Int32, Int32)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_event()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_event.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("E:TestLibrary1.Class1.DoStuff"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>DoStuff</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_field()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName,"_field.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("F:TestLibrary1.Class1.SomeNullableInt"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>SomeNullableInt</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_method()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("M:TestLibrary1.Class1.DoubleStatic(System.Int32)"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>DoubleStatic(Int32)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_instance_constructor()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("M:TestLibrary1.Class1.#ctor(System.String)"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>Class1(String)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_operator()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_method.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>operator +(Class1, Class1)</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_namespace()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_namespace.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("N:TestLibrary1"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>TestLibrary1</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_property()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_property.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("P:TestLibrary1.Class1.SomeProperty"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>SomeProperty</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_indexer()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_property.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("P:TestLibrary1.Class1.Item(System.Int32)"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>Item[Int32]</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_type()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_type.cshtml"),
                    @"<html><h1>@Model.ShortName</h1></html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("T:TestLibrary1.Class1"));

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                File.ReadAllText(result.FullName).Should().Be(@"<html><h1>Class1</h1></html>");
            }
        }

        [Fact]
        public static void doc_page_link_to_others()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_type.cshtml"),
@"@using DuckyDocs.CodeDoc
@using DuckyDocs.SiteBuilder
<html><h1>@Model.ShortName</h1>@foreach(var field in Model.Fields.Cast<CodeDocField>()){
    <div><a href=""@(StaticApiPageGenerator.CreateSlugName(field.CRefText)).html"">@field.ShortName</a></div>
}</html>");
                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var result = apiPageGen.GenerateForTarget(new CRefIdentifier("T:TestLibrary1.Class1"));
                var resultText = File.ReadAllText(result.FullName);

                result.Should().NotBeNull();
                result.Exists.Should().BeTrue();
                resultText.Should().StartWith(@"<html><h1>Class1</h1>");
                resultText.Should().Contain(@"<a href=""TestLibrary1.Class1.SomeField-F.html"">SomeField</a>");
                resultText.Should().EndWith(@"</html>");
            }
        }

        [Fact]
        public static void dump_entire_repository()
        {
            RazorEngine.Razor.SetTemplateService(new RazorEngine.Templating.TemplateService());
            using (var testFolder = DirectoryLifetimeManager.Create())
            {
                var templateDirectory = testFolder.Directory.CreateSubdirectory("templates");
                var outputDirectory = testFolder.Directory.CreateSubdirectory("out");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_type.cshtml"),
@"@using DuckyDocs.CodeDoc
@using DuckyDocs.SiteBuilder
<html><h1>@Model.ShortName</h1>@foreach(var field in (Model.Fields ?? Enumerable.Empty<ICodeDocMember>()).Cast<CodeDocField>()){
    <div><a href=""@(StaticApiPageGenerator.CreateSlugName(field.CRefText)).html"">@field.ShortName</a></div>
}</html>");
                File.WriteAllText(
                    Path.Combine(templateDirectory.FullName, "_namespace.cshtml"),
@"@using DuckyDocs.CodeDoc
@using DuckyDocs.SiteBuilder
<html><h1>@Model.ShortName</h1>@foreach(var typeCRef in Model.TypeCRefs){
    var typeModel = ViewBag.GetTargetPreviewModel(typeCRef);
    <div><a href=""@(StaticApiPageGenerator.CreateSlugName(typeModel.CRefText)).html"">@typeModel.ShortName</a></div>
}</html>");
                foreach (var fileName in new[] {
                    "_delegate.cshtml",
                    "_method.cshtml",
                    "_field.cshtml",
                    "_property.cshtml",
                    "_event.cshtml"
                }) {
                    File.WriteAllText(
                        Path.Combine(templateDirectory.FullName, fileName),
                        @"<html><h1>@Model.ShortName</h1></html>");
                }

                var apiPageGen = new StaticApiPageGenerator
                {
                    TemplateDirectory = templateDirectory,
                    OutputDirectory = outputDirectory,
                    TargetRepository = CreateTestLibrary1Repository()
                };

                var results = apiPageGen.GenerateForAllTargets();
                var resultShortNames = results.Select(x => x.Name);

                resultShortNames.Should().Contain("InGlobal.Nope-M.html");
                resultShortNames.Should().Contain("-N.html");
                resultShortNames.Should().Contain("Test.Annotations-N.html");
                resultShortNames.Should().Contain("TestLibrary1.Class1.Inner-T.html");
                resultShortNames.Should().Contain("TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32!,System.Int32!)-M.html");
                resultShortNames.Should().Contain("TestLibrary1.Generic1!2.AMix!!1(!0,!!0)-M.html");
                resultShortNames.Should().Contain("TestLibrary1.PublicExposedTestClass.ProtectedEvent-E.html");
                resultShortNames.Should().Contain("TestLibrary1.ClassWithContracts.Stuff-P.html");
                resultShortNames.Should().Contain("TestLibrary1.Class1.MyFunc-T.html");

                resultShortNames.Should().NotContain("System.Attribute.GetHashCode-M.html");
                resultShortNames.Should().NotContain("System.Object.ToString-M.html");
            }
        }

    }
}
