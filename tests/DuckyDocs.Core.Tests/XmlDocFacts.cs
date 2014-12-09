using System;
using System.IO;
using System.Linq;
using DuckyDocs.Reflection;
using DuckyDocs.XmlDoc;
using TestLibrary1;
using Xunit;

namespace DuckyDocs.Core.Tests
{
    public class XmlDocFacts
    {

        public XmlDocFacts() {
            var testDllLocation = typeof(TestLibrary1.FlagsEnum).Assembly.Location;
            var textXmlLocation = Path.ChangeExtension(testDllLocation, "XML");
            Docs = new XmlAssemblyDocument(textXmlLocation);
        }

        public XmlAssemblyDocument Docs { get; private set; }

        [Fact]
        public void can_load_xml_from_assembly_reflection() {
            var assembly = typeof(Class1).Assembly;
            var xmlDoc = new XmlAssemblyDocument(Path.ChangeExtension(assembly.GetFilePath(), "XML"));
            Assert.NotNull(xmlDoc);
        }

        [Fact]
        public void read_no_docs_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.NoDocs");
            Assert.Null(docs);
        }

        [Fact]
        public void read_summary_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1");
            Assert.NotNull(docs);
            Assert.NotNull(docs.SummaryElement);
            Assert.True(docs.HasSummaryContents);
            Assert.Equal(1, docs.SummaryContents.Count);
            Assert.Equal(
                "This class is just for testing and has no real use outside of generating some documentation.",
                ((XmlDocTextNode)(docs.SummaryContents[0])).HtmlDecoded
            );
        }

        [Fact]
        public void read_blank_summary_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.Inner");
            Assert.NotNull(docs);
            Assert.Null(docs.SummaryElement);
            Assert.Equal(0, docs.SummaryContents.Count);
        }

        [Fact]
        public void read_remarks_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.Inner");
            Assert.NotNull(docs);
            Assert.NotNull(docs.RemarksElements);
            Assert.True(docs.HasRemarksElements);
            Assert.Equal(1, docs.RemarksElements.Count);
            Assert.Equal("This is just some class.", docs.RemarksElements.First().Node.InnerXml);
        }

        [Fact]
        public void read_blank_remarks_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.NoRemarks");
            Assert.NotNull(docs);
            Assert.NotNull(docs.RemarksElements);
            Assert.Empty(docs.RemarksElements);
        }

        [Fact]
        public void read_summary_and_blank_remarks_from_static_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#cctor");
            Assert.NotNull(docs);
            Assert.Empty(docs.RemarksElements);
            Assert.False(docs.HasRemarksElements);
            Assert.NotNull(docs.SummaryElement);
            Assert.Equal(1, docs.SummaryContents.Count);
            Assert.Equal("The static constructor.", docs.SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void read_docs_from_parameterized_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String)");
            Assert.NotNull(docs);
            Assert.NotNull(docs.SummaryElement);
            Assert.Equal("The instance constructor.", docs.SummaryContents[0].Node.OuterXml);
            Assert.NotNull(docs.RemarksElements);
            Assert.Equal("A remark.", docs.RemarksElements[0].Node.InnerXml);
            Assert.NotNull(docs.GetParameterSummary("crap"));
            Assert.Equal("Whatever.", docs.GetParameterSummary("crap").Node.InnerXml);
            Assert.True(docs.HasParameterSummaries);
            Assert.Equal(1, docs.ParameterSummaries.Count);
        }

        [Fact]
        public void read_two_param_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
            Assert.NotNull(docs);
            Assert.True(docs.HasParameterSummaries);
            Assert.Equal(2, docs.ParameterSummaries.Count);
            Assert.NotNull(docs.GetParameterSummary("crap"));
            Assert.Equal("Crap param.", docs.GetParameterSummary("crap").Node.InnerXml);
            Assert.NotNull(docs.GetParameterSummary("dookie"));
            Assert.Equal("Dookie param.", docs.GetParameterSummary("dookie").Node.InnerXml);
        }

        [Fact]
        public void read_operator_docs() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
            Assert.NotNull(docs);
            Assert.Equal("The left hand parameter.", docs.GetParameterSummary("a").Node.InnerXml);
            Assert.Equal("The right hand parameter.", docs.GetParameterSummary("b").Node.InnerXml);
            Assert.NotNull(docs.ReturnsElement);
            Assert.True(docs.HasReturnsContents);
            Assert.Equal(1, docs.ReturnsContents.Count);
            Assert.Equal("Nope!", docs.ReturnsContents[0].Node.OuterXml);
        }

        [Fact]
        public void read_indexer_docs() {
            var docs = Docs.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.NotNull(docs);
            Assert.Equal("Just your average indexer.", docs.SummaryElement.Node.InnerXml);
            Assert.Equal("an index", docs.GetParameterSummary("n").Node.InnerXml);
            Assert.Equal("a number", docs.ReturnsElement.Node.InnerXml);
            Assert.Equal("Some number.", docs.ValueElement.Node.InnerXml);
        }

        [Fact]
        public void read_const_docs() {
            var docs = Docs.GetMember("F:TestLibrary1.Class1.MyConst");
            Assert.NotNull(docs);
            Assert.Equal("just a const", docs.SummaryElement.Node.InnerXml);
            Assert.Equal("1", docs.ValueElement.Node.InnerXml);
        }

        [Fact]
        public void read_delegate_docs() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.MyFunc");
            Assert.NotNull(docs);
            Assert.Equal("My delegate.", docs.SummaryElement.Node.InnerXml);
            Assert.Equal("param a", docs.GetParameterSummary("a").Node.InnerXml);
            Assert.Equal("param b", docs.GetParameterSummary("b").Node.InnerXml);
            Assert.Equal("some int", docs.ReturnsElement.Node.InnerXml);
        }

        [Fact]
        public void read_event_docs() {
            var docs = Docs.GetMember("E:TestLibrary1.Class1.DoStuff");
            Assert.NotNull(docs);
            Assert.Equal("My event!", docs.SummaryElement.Node.InnerXml);
        }

        [Fact]
        public void read_exception_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
            Assert.NotNull(docs);
            Assert.Equal(2, docs.ExceptionElements.Count);
            Assert.Equal("This is not implemented.", docs.ExceptionElements[0].Node.InnerXml);
            Assert.True(docs.ExceptionElements.Select(x => x.CRef).All(x => x == "T:System.NotImplementedException"));
        }

        [Fact]
        public void read_inline_code_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.NotNull(docs);
            var codeElement = docs.SummaryContents.OfType<XmlDocCodeElement>().Single();
            Assert.True(codeElement.IsInline);
            Assert.Equal("result = value + value", codeElement.Children[0].Node.OuterXml);
        }

        [Fact]
        public void read_text_nodes_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.NotNull(docs);
            var codeElement = docs.SummaryContents.OfType<XmlDocTextNode>().ToList();
            Assert.Equal(2, codeElement.Count);
            Assert.Equal("Doubles the given value like so: ", codeElement[0].HtmlDecoded);
            Assert.Equal(".", codeElement[1].HtmlDecoded);
        }

        [Fact]
        public void read_code_block_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.NotNull(docs);
            var codeBlock = docs.RemarksElements
                .First()
                .Children
                .OfType<XmlDocCodeElement>()
                .FirstOrDefault();
            Assert.NotNull(codeBlock);
            Assert.False(codeBlock.IsInline);
            Assert.Equal("This\n is\n  some\n   text.", codeBlock.Node.InnerXml);
        }

        [Fact]
        public void read_examples_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.NotNull(docs);
            var examples = docs.ExampleElements;
            Assert.NotNull(examples);
            Assert.Equal(2, examples.Count);
            Assert.Equal("Example 1", examples[0].Node.InnerXml);
            Assert.Equal("Example 2", examples[1].Node.InnerXml);
        }

        [Fact]
        public void read_list_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.NotNull(docs);
            Assert.NotNull(docs.SummaryElement);
            Assert.True(docs.HasSummaryContents);
            var list = docs.SummaryContents.OfType<XmlDocDefinitionList>().Single();
            Assert.NotNull(list);
            Assert.True("bullet".Equals(list.ListType, StringComparison.OrdinalIgnoreCase));
            var items = list.Items.ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("Col 1", items[0].TermContents.First().Node.OuterXml);
            Assert.Equal("Col 2", items[0].DescriptionContents.First().Node.OuterXml);
            Assert.True(items[0].IsHeader);
            Assert.Equal("A term.", items[1].TermContents.First().Node.OuterXml);
            Assert.Equal("A description.", items[1].DescriptionContents.First().Node.OuterXml);
            Assert.False(items[1].IsHeader);
        }

        [Fact]
        public void read_para_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.Finalize");
            Assert.NotNull(docs);
            var paragraphs = docs.RemarksElements.First()
                .Children
                .OfType<XmlDocElement>()
                .Where(x => "PARA".Equals(x.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Equal(3, paragraphs.Count);
            Assert.Equal("a paragraph", paragraphs[0].Node.InnerXml.Trim());
            Assert.Equal("and another", paragraphs[1].Node.InnerXml.Trim());
            Assert.Equal("a third", paragraphs[2].Node.InnerXml.Trim());
        }

        [Fact]
        public void paramref_from_property() {
            var docs = Docs.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.NotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.Equal("index", paramref.Node.InnerXml);
            Assert.Equal("n", paramref.TargetName);
        }

        [Fact]
        public void paramref_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
            Assert.NotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.Equal("crap", paramref.TargetName);
        }

        [Fact]
        public void paramref_from_delegate() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.MyFunc");
            Assert.NotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.Equal("a", paramref.TargetName);
        }

        [Fact]
        public void permission_from_field() {
            var docs = Docs.GetMember("F:TestLibrary1.Class1.MyConst");
            Assert.Equal(1, docs.PermissionElements.Count);
            Assert.Equal("T:System.Security.PermissionSet", docs.PermissionElements[0].CRef);
            Assert.Equal("I have no idea what this is for.", docs.PermissionElements[0].Node.InnerXml);
        }

        [Fact]
        public void see_and_seealso_from_event() {
            var docs = Docs.GetMember("E:TestLibrary1.Class1.DoStuff");
            var see = docs.RemarksElements.First().Children.OfType<XmlDocRefElement>().Single();
            Assert.Equal("T:TestLibrary1.Class1", see.CRef);
            Assert.Equal(2, docs.SeeAlsoElements.Count);
            Assert.Equal("T:TestLibrary1.Class1.MyFunc", docs.SeeAlsoElements[0].CRef);
            Assert.Equal("The delegate.", docs.SeeAlsoElements[0].Node.InnerXml);
            Assert.Equal("M:TestLibrary1.Class1.DoubleStatic(System.Int32)", docs.SeeAlsoElements[1].CRef);
        }

        [Fact]
        public void typeparamref_on_self_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.Equal("TA", typeparamref.TargetName);
        }

        [Fact]
        public void typeparamref_to_parent_class() {
            var docs = Docs.GetMember(
                "M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.Equal("TA", typeparamref.TargetName);
        }

        [Fact]
        public void typeparamref_to_delegate_generic() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2.MyFunc`1");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.Equal("TX", typeparamref.TargetName);
        }

        [Fact]
        public void typeparam_on_generic_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2");
            Assert.Equal(2, docs.TypeParameterSummaries.Count);
            Assert.Equal("TA", docs.TypeParameterSummaries[0].TargetName);
            Assert.Equal("TB", docs.TypeParameterSummaries[1].TargetName);
            Assert.Equal("B", docs.TypeParameterSummaries[1].Node.InnerXml);
            var tbParam = docs.GetTypeParameterSummary("TB");
            Assert.Equal("TB", tbParam.TargetName);
            Assert.Equal("B", tbParam.Node.InnerXml);
            Assert.Null(docs.GetTypeParameterSummary("XYZ"));
        }

    }
}
