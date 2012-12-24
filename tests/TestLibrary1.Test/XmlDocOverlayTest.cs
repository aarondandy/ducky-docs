using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Core;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class XmlDocOverlayTest
	{

		public XmlDocOverlayTest(){
			AssemblyDefinitionCollection = new AssemblyDefinitionCollection("./TestLibrary1.dll");
			CrefOverlay = new CrefOverlay(AssemblyDefinitionCollection);
			XmlDocOverlay = new XmlDocOverlay(CrefOverlay);
		}

		private AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		[Test]
		public void can_load_xml_from_assembly_definition(){
			var assemblyDefinition = GetAssembly();
			var xmlDoc = XmlDocOverlay.Load(assemblyDefinition);
			Assert.IsNotNull(xmlDoc);
			Assert.IsNotNull(xmlDoc.FirstChild);
		}

		[Test]
		public void read_summary_from_class(){
			var type = CrefOverlay.GetTypeDefinition("T:TestLibrary1.Class1");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("This class is just for testing and has no real use outside of generating some documentation.", docs.Summary.NormalizedInnerXml);
		}

		[Test]
		public void read_blank_summary_from_class(){
			var type = CrefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNull(docs.Summary);
		}

		[Test]
		public void read_remarks_from_class() {
			var type = CrefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNotNull(docs.Remarks);
			Assert.AreEqual("This is just some class.", docs.Remarks.NormalizedInnerXml);
		}

		[Test]
		public void read_blank_remarks_from_class() {
			var type = CrefOverlay.GetTypeDefinition("T:TestLibrary1.Class1");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNull(docs.Remarks);
		}

		[Test]
		public void read_summary_and_blank_remarks_from_static_constructor() {
			var ctor = CrefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#cctor");
			var docs = XmlDocOverlay.GetDocumentation(ctor);
			Assert.IsNull(docs.Remarks);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("The static constructor.", docs.Summary.NormalizedInnerXml);
		}

		[Test]
		public void read_docs_from_parameterized_constructor() {
			var ctor = CrefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String)");
			var docs = XmlDocOverlay.GetDocumentation(ctor) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("The instance constructor.", docs.Summary.NormalizedInnerXml);
			Assert.IsNotNull(docs.Remarks);
			Assert.AreEqual("A remark.", docs.Remarks.NormalizedInnerXml);
			Assert.IsNotNull(docs.DocsForParameter("crap"));
			Assert.AreEqual("Whatever.", docs.DocsForParameter("crap").NormalizedInnerXml);
		}

		[Test]
		public void read_two_param_constructor() {
			var ctor = CrefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
			var docs = XmlDocOverlay.GetDocumentation(ctor) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.DocsForParameter("crap"));
			Assert.AreEqual("Crap param.", docs.DocsForParameter("crap").NormalizedInnerXml);
			Assert.IsNotNull(docs.DocsForParameter("dookie"));
			Assert.AreEqual("Dookie param.", docs.DocsForParameter("dookie").NormalizedInnerXml);
		}

		[Test]
		public void read_operator_docs() {
			var op = CrefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
			var docs = XmlDocOverlay.GetDocumentation(op) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.AreEqual("The left hand parameter.", docs.DocsForParameter("a").NormalizedInnerXml);
			Assert.AreEqual("The right hand parameter.", docs.DocsForParameter("b").NormalizedInnerXml);
			Assert.IsNotNull(docs.Returns);
			Assert.AreEqual("Nope!", docs.Returns.NormalizedInnerXml);
		}

		[Test]
		public void read_indexer_docs() {
			var indexer = CrefOverlay.GetMemberDefinition("P:TestLibrary1.Class1.Item(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(indexer) as PropertyDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("Just your average indexer.", docs.Summary.NormalizedInnerXml);
			Assert.AreEqual("an index", docs.DocsForParameter("n").NormalizedInnerXml);
			Assert.AreEqual("a number", docs.Returns.NormalizedInnerXml);
			Assert.AreEqual("Some number.", docs.ValueDoc.NormalizedInnerXml);
		}

		[Test]
		public void read_const_docs() {
			var f = CrefOverlay.GetMemberDefinition("F:TestLibrary1.Class1.MyConst");
			var docs = XmlDocOverlay.GetDocumentation(f) as FieldDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("just a const", docs.Summary.NormalizedInnerXml);
			Assert.AreEqual("1", docs.ValueDoc.NormalizedInnerXml);
		}

		[Test]
		public void read_delegate_docs() {
			var d = CrefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.MyFunc");
			var docs = XmlDocOverlay.GetDocumentation(d) as DelegateTypeDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("My delegate.", docs.Summary.NormalizedInnerXml);
			Assert.AreEqual("param a", docs.DocsForParameter("a").NormalizedInnerXml);
			Assert.AreEqual("param b", docs.DocsForParameter("b").NormalizedInnerXml);
			Assert.AreEqual("some int", docs.Returns.NormalizedInnerXml);
		}

		[Test]
		public void read_event_docs() {
			var e = CrefOverlay.GetMemberDefinition("E:TestLibrary1.Class1.DoStuff");
			var docs = XmlDocOverlay.GetDocumentation(e) as EventDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("My event!", docs.Summary.NormalizedInnerXml);
		}

	}
}
