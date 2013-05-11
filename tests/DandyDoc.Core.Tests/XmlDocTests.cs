using System;
using System.IO;
using System.Linq;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.Core.Tests
{
    [TestFixture]
    public class XmlDocTests
    {

        public XmlDocTests() {
            Docs = new XmlAssemblyDocumentation("./TestLibrary1.XML");
        }

        public XmlAssemblyDocumentation Docs { get; private set; }

        [Test]
        public void can_load_xml_from_assembly_reflection() {
            var assembly = typeof(Class1).Assembly;
            var xmlDoc = new XmlAssemblyDocumentation(Path.ChangeExtension(ReflectionUtilities.GetFilePath(assembly), "XML"));
            Assert.IsNotNull(xmlDoc);
        }

        [Test]
        public void read_no_docs_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.NoDocs");
            Assert.IsNull(docs);
        }

        [Test]
        public void read_summary_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1");
            Assert.IsNotNull(docs);
            Assert.IsNotNull(docs.SummaryElement);
            Assert.IsTrue(docs.HasSummaryContents);
            Assert.AreEqual(1, docs.SummaryContents.Count);
            Assert.AreEqual(
                "This class is just for testing and has no real use outside of generating some documentation.",
                ((XmlDocTextNode)(docs.SummaryContents[0])).HtmlDecoded
            );
        }

        [Test]
        public void read_blank_summary_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.Inner");
            Assert.IsNotNull(docs);
            Assert.IsNull(docs.SummaryElement);
            Assert.AreEqual(0, docs.SummaryContents.Count);
        }

        [Test]
        public void read_remarks_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.Inner");
            Assert.IsNotNull(docs);
            Assert.IsNotNull(docs.RemarksElements);
            Assert.IsTrue(docs.HasRemarksElements);
            Assert.AreEqual(1, docs.RemarksElements.Count);
            Assert.AreEqual("This is just some class.", docs.RemarksElements.First().Node.InnerXml);
        }

        [Test]
        public void read_blank_remarks_from_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.NoRemarks");
            Assert.IsNotNull(docs);
            Assert.IsNotNull(docs.RemarksElements);
            Assert.IsEmpty(docs.RemarksElements);
        }

        [Test]
        public void read_summary_and_blank_remarks_from_static_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#cctor");
            Assert.IsNotNull(docs);
            Assert.IsEmpty(docs.RemarksElements);
            Assert.IsFalse(docs.HasRemarksElements);
            Assert.IsNotNull(docs.SummaryElement);
            Assert.AreEqual(1, docs.SummaryContents.Count);
            Assert.AreEqual("The static constructor.", docs.SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void read_docs_from_parameterized_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String)");
            Assert.IsNotNull(docs);
            Assert.IsNotNull(docs.SummaryElement);
            Assert.AreEqual("The instance constructor.", docs.SummaryContents[0].Node.OuterXml);
            Assert.IsNotNull(docs.RemarksElements);
            Assert.AreEqual("A remark.", docs.RemarksElements[0].Node.InnerXml);
            Assert.IsNotNull(docs.GetParameterSummary("crap"));
            Assert.AreEqual("Whatever.", docs.GetParameterSummary("crap").Node.InnerXml);
            Assert.IsTrue(docs.HasParameterSummaries);
            Assert.AreEqual(1, docs.ParameterSummaries.Count);
        }

        [Test]
        public void read_two_param_constructor() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
            Assert.IsNotNull(docs);
            Assert.IsTrue(docs.HasParameterSummaries);
            Assert.AreEqual(2, docs.ParameterSummaries.Count);
            Assert.IsNotNull(docs.GetParameterSummary("crap"));
            Assert.AreEqual("Crap param.", docs.GetParameterSummary("crap").Node.InnerXml);
            Assert.IsNotNull(docs.GetParameterSummary("dookie"));
            Assert.AreEqual("Dookie param.", docs.GetParameterSummary("dookie").Node.InnerXml);
        }

        [Test]
        public void read_operator_docs() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
            Assert.IsNotNull(docs);
            Assert.AreEqual("The left hand parameter.", docs.GetParameterSummary("a").Node.InnerXml);
            Assert.AreEqual("The right hand parameter.", docs.GetParameterSummary("b").Node.InnerXml);
            Assert.IsNotNull(docs.ReturnsElement);
            Assert.IsTrue(docs.HasReturnsContents);
            Assert.AreEqual(1, docs.ReturnsContents.Count);
            Assert.AreEqual("Nope!", docs.ReturnsContents[0].Node.OuterXml);
        }

        [Test]
        public void read_indexer_docs() {
            var docs = Docs.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.IsNotNull(docs);
            Assert.AreEqual("Just your average indexer.", docs.SummaryElement.Node.InnerXml);
            Assert.AreEqual("an index", docs.GetParameterSummary("n").Node.InnerXml);
            Assert.AreEqual("a number", docs.ReturnsElement.Node.InnerXml);
            Assert.AreEqual("Some number.", docs.ValueElement.Node.InnerXml);
        }

        [Test]
        public void read_const_docs() {
            var docs = Docs.GetMember("F:TestLibrary1.Class1.MyConst");
            Assert.IsNotNull(docs);
            Assert.AreEqual("just a const", docs.SummaryElement.Node.InnerXml);
            Assert.AreEqual("1", docs.ValueElement.Node.InnerXml);
        }

        [Test]
        public void read_delegate_docs() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.MyFunc");
            Assert.IsNotNull(docs);
            Assert.AreEqual("My delegate.", docs.SummaryElement.Node.InnerXml);
            Assert.AreEqual("param a", docs.GetParameterSummary("a").Node.InnerXml);
            Assert.AreEqual("param b", docs.GetParameterSummary("b").Node.InnerXml);
            Assert.AreEqual("some int", docs.ReturnsElement.Node.InnerXml);
        }

        [Test]
        public void read_event_docs() {
            var docs = Docs.GetMember("E:TestLibrary1.Class1.DoStuff");
            Assert.IsNotNull(docs);
            Assert.AreEqual("My event!", docs.SummaryElement.Node.InnerXml);
        }

        [Test]
        public void read_exception_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
            Assert.IsNotNull(docs);
            Assert.AreEqual(2, docs.ExceptionElements.Count);
            Assert.AreEqual("This is not implemented.", docs.ExceptionElements[0].Node.InnerXml);
            Assert.That(docs.ExceptionElements.Select(x => x.CRef), Has.All.EqualTo("T:System.NotImplementedException"));
        }

        [Test]
        public void read_inline_code_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.IsNotNull(docs);
            var codeElement = docs.SummaryContents.OfType<XmlDocCodeElement>().Single();
            Assert.IsTrue(codeElement.IsInline);
            Assert.AreEqual("result = value + value", codeElement.Children[0].Node.OuterXml);
        }

        [Test]
        public void read_text_nodes_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.IsNotNull(docs);
            var codeElement = docs.SummaryContents.OfType<XmlDocTextNode>().ToList();
            Assert.AreEqual(2, codeElement.Count);
            Assert.AreEqual("Doubles the given value like so: ", codeElement[0].HtmlDecoded);
            Assert.AreEqual(".", codeElement[1].HtmlDecoded);
        }

        [Test]
        public void read_code_block_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.IsNotNull(docs);
            var codeBlock = docs.RemarksElements
                .First()
                .Children
                .OfType<XmlDocCodeElement>()
                .FirstOrDefault();
            Assert.IsNotNull(codeBlock);
            Assert.IsFalse(codeBlock.IsInline);
            Assert.AreEqual("This\n is\n  some\n   text.", codeBlock.Node.InnerXml);
        }

        [Test]
        public void read_examples_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.IsNotNull(docs);
            var examples = docs.ExampleElements;
            Assert.IsNotNull(examples);
            Assert.AreEqual(2, examples.Count);
            Assert.AreEqual("Example 1", examples[0].Node.InnerXml);
            Assert.AreEqual("Example 2", examples[1].Node.InnerXml);
        }

        [Test]
        public void read_list_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.IsNotNull(docs);
            Assert.IsNotNull(docs.SummaryElement);
            Assert.IsTrue(docs.HasSummaryContents);
            var list = docs.SummaryContents.OfType<XmlDocDefinitionList>().Single();
            Assert.IsNotNull(list);
            Assert.That("bullet".Equals(list.ListType, StringComparison.OrdinalIgnoreCase));
            var items = list.Items.ToList();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Col 1", items[0].TermContents.First().Node.OuterXml);
            Assert.AreEqual("Col 2", items[0].DescriptionContents.First().Node.OuterXml);
            Assert.IsTrue(items[0].IsHeader);
            Assert.AreEqual("A term.", items[1].TermContents.First().Node.OuterXml);
            Assert.AreEqual("A description.", items[1].DescriptionContents.First().Node.OuterXml);
            Assert.IsFalse(items[1].IsHeader);
        }

        [Test]
        public void read_para_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.Finalize");
            Assert.IsNotNull(docs);
            var paragraphs = docs.RemarksElements.First()
                .Children
                .OfType<XmlDocElement>()
                .Where(x => "PARA".Equals(x.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.AreEqual(3, paragraphs.Count);
            Assert.AreEqual("a paragraph", paragraphs[0].Node.InnerXml.Trim());
            Assert.AreEqual("and another", paragraphs[1].Node.InnerXml.Trim());
            Assert.AreEqual("a third", paragraphs[2].Node.InnerXml.Trim());
        }

        [Test]
        public void paramref_from_property() {
            var docs = Docs.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.IsNotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("index", paramref.Node.InnerXml);
            Assert.AreEqual("n", paramref.TargetName);
        }

        [Test]
        public void paramref_from_method() {
            var docs = Docs.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
            Assert.IsNotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("crap", paramref.TargetName);
        }

        [Test]
        public void paramref_from_delegate() {
            var docs = Docs.GetMember("T:TestLibrary1.Class1.MyFunc");
            Assert.IsNotNull(docs);
            var paramref = docs.RemarksElements.First().Children.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("a", paramref.TargetName);
        }

        [Test]
        public void permission_from_field() {
            var docs = Docs.GetMember("F:TestLibrary1.Class1.MyConst");
            Assert.AreEqual(1, docs.PermissionElements.Count);
            Assert.AreEqual("T:System.Security.PermissionSet", docs.PermissionElements[0].CRef);
            Assert.AreEqual("I have no idea what this is for.", docs.PermissionElements[0].Node.InnerXml);
        }

        [Test]
        public void see_and_seealso_from_event() {
            var docs = Docs.GetMember("E:TestLibrary1.Class1.DoStuff");
            var see = docs.RemarksElements.First().Children.OfType<XmlDocRefElement>().Single();
            Assert.AreEqual("T:TestLibrary1.Class1", see.CRef);
            Assert.AreEqual(2, docs.SeeAlsoElements.Count);
            Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", docs.SeeAlsoElements[0].CRef);
            Assert.AreEqual("The delegate.", docs.SeeAlsoElements[0].Node.InnerXml);
            Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Int32)", docs.SeeAlsoElements[1].CRef);
        }

        [Test]
        public void typeparamref_on_self_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("TA", typeparamref.TargetName);
        }

        [Test]
        public void typeparamref_to_parent_class() {
            var docs = Docs.GetMember(
                "M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("TA", typeparamref.TargetName);
        }

        [Test]
        public void typeparamref_to_delegate_generic() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2.MyFunc`1");
            var typeparamref = docs.SummaryContents.OfType<XmlDocNameElement>().Single();
            Assert.AreEqual("TX", typeparamref.TargetName);
        }

        [Test]
        public void typeparam_on_generic_class() {
            var docs = Docs.GetMember("T:TestLibrary1.Generic1`2");
            Assert.AreEqual(2, docs.TypeParameterSummaries.Count);
            Assert.AreEqual("TA", docs.TypeParameterSummaries[0].TargetName);
            Assert.AreEqual("TB", docs.TypeParameterSummaries[1].TargetName);
            Assert.AreEqual("B", docs.TypeParameterSummaries[1].Node.InnerXml);
            var tbParam = docs.GetTypeParameterSummary("TB");
            Assert.AreEqual("TB", tbParam.TargetName);
            Assert.AreEqual("B", tbParam.Node.InnerXml);
            Assert.IsNull(docs.GetTypeParameterSummary("XYZ"));
        }

    }
}
