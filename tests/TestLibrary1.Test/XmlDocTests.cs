using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Cecil;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class XmlDocTests
	{

		public XmlDocTests() {
			Docs = new XmlAssemblyDocumentation("./TestLibrary1.XML");
		}

		public XmlAssemblyDocumentation Docs { get; private set; }

		[Test]
		public void can_load_xml_from_assembly_definition() {
			var assembly = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			var xmlDoc = new XmlAssemblyDocumentation(Path.ChangeExtension(CecilUtilities.GetFilePath(assembly), "XML"));
			Assert.IsNotNull(xmlDoc);
		}

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

	}
}
