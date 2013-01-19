using System.Linq;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class XmlDocOverlayTest
	{

		public XmlDocOverlayTest(){
			AssemblyDefinitionCollection = new AssemblyDefinitionCollection("./TestLibrary1.dll");
			CRefOverlay = new CRefOverlay(AssemblyDefinitionCollection);
			XmlDocOverlay = new XmlDocOverlay(CRefOverlay);
		}

		private AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CRefOverlay CRefOverlay { get; private set; }

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
			var type = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("This class is just for testing and has no real use outside of generating some documentation.", docs.Summary.Node.InnerXml);
		}

		[Test]
		public void read_blank_summary_from_class(){
			var type = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNull(docs.Summary);
		}

		[Test]
		public void read_remarks_from_class() {
			var type = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNotNull(docs.Remarks);
			Assert.AreEqual("This is just some class.", docs.Remarks.First().Node.InnerXml);
		}

		[Test]
		public void read_blank_remarks_from_class() {
			var type = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.NoRemarks");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.Remarks);
			Assert.IsEmpty(docs.Remarks);
		}

		[Test]
		public void read_no_docs_from_class() {
			var type = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.NoDocs");
			var docs = XmlDocOverlay.GetDocumentation(type);
			Assert.IsNull(docs);
		}

		[Test]
		public void read_summary_and_blank_remarks_from_static_constructor() {
			var ctor = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#cctor");
			var docs = XmlDocOverlay.GetDocumentation(ctor);
			Assert.IsNotNull(docs.Remarks);
			Assert.IsEmpty(docs.Remarks);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("The static constructor.", docs.Summary.Node.InnerXml);
		}

		[Test]
		public void read_docs_from_parameterized_constructor() {
			var ctor = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String)");
			var docs = XmlDocOverlay.GetDocumentation(ctor) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.Summary);
			Assert.AreEqual("The instance constructor.", docs.Summary.Node.InnerXml);
			Assert.IsNotNull(docs.Remarks);
			Assert.AreEqual("A remark.", docs.Remarks.First().Node.InnerXml);
			Assert.IsNotNull(docs.DocsForParameter("crap"));
			Assert.AreEqual("Whatever.", docs.DocsForParameter("crap").Node.InnerXml);
		}

		[Test]
		public void read_two_param_constructor() {
			var ctor = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
			var docs = XmlDocOverlay.GetDocumentation(ctor) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.DocsForParameter("crap"));
			Assert.AreEqual("Crap param.", docs.DocsForParameter("crap").Node.InnerXml);
			Assert.IsNotNull(docs.DocsForParameter("dookie"));
			Assert.AreEqual("Dookie param.", docs.DocsForParameter("dookie").Node.InnerXml);
		}

		[Test]
		public void read_operator_docs() {
			var op = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
			var docs = XmlDocOverlay.GetDocumentation(op) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.AreEqual("The left hand parameter.", docs.DocsForParameter("a").Node.InnerXml);
			Assert.AreEqual("The right hand parameter.", docs.DocsForParameter("b").Node.InnerXml);
			Assert.IsNotNull(docs.Returns);
			Assert.AreEqual("Nope!", docs.Returns.Node.InnerXml);
		}

		[Test]
		public void read_indexer_docs() {
			var indexer = CRefOverlay.GetMemberDefinition("P:TestLibrary1.Class1.Item(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(indexer) as PropertyDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("Just your average indexer.", docs.Summary.Node.InnerXml);
			Assert.AreEqual("an index", docs.DocsForParameter("n").Node.InnerXml);
			Assert.AreEqual("a number", docs.Returns.Node.InnerXml);
			Assert.AreEqual("Some number.", docs.ValueDoc.Node.InnerXml);
		}

		[Test]
		public void read_const_docs() {
			var f = CRefOverlay.GetMemberDefinition("F:TestLibrary1.Class1.MyConst");
			var docs = XmlDocOverlay.GetDocumentation(f) as FieldDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("just a const", docs.Summary.Node.InnerXml);
			Assert.AreEqual("1", docs.ValueDoc.Node.InnerXml);
		}

		[Test]
		public void read_delegate_docs() {
			var d = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.MyFunc");
			var docs = XmlDocOverlay.GetDocumentation(d) as DelegateTypeDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("My delegate.", docs.Summary.Node.InnerXml);
			Assert.AreEqual("param a", docs.DocsForParameter("a").Node.InnerXml);
			Assert.AreEqual("param b", docs.DocsForParameter("b").Node.InnerXml);
			Assert.AreEqual("some int", docs.Returns.Node.InnerXml);
		}

		[Test]
		public void read_event_docs() {
			var e = CRefOverlay.GetMemberDefinition("E:TestLibrary1.Class1.DoStuff");
			var docs = XmlDocOverlay.GetDocumentation(e) as EventDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			Assert.AreEqual("My event!", docs.Summary.Node.InnerXml);
		}

		[Test]
		public void read_exception_from_method(){
			var op = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)");
			var docs = XmlDocOverlay.GetDocumentation(op) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			Assert.AreEqual(2, docs.Exceptions.Count);
			Assert.AreEqual("This is not implemented.", docs.Exceptions[0].Node.InnerXml);
			Assert.That(docs.Exceptions.Cast<ParsedXmlException>().Select(x => x.CRef), Has.All.EqualTo("T:System.NotImplementedException"));
		}

		[Test]
		public void read_inline_code_from_method(){
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var codeElement = docs.Summary.Children.OfType<ParsedXmlCode>().Single();
			Assert.AreEqual(true, codeElement.Inline);
			Assert.AreEqual("result = value + value", codeElement.Node.InnerXml);
		}

		[Test]
		public void read_text_nodes_from_method(){
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var codeElement = docs.Summary.Children.OfType<ParsedXmlTextNode>().ToList();
			Assert.AreEqual(2, codeElement.Count);
			Assert.AreEqual("Doubles the given value like so: ", codeElement[0].HtmlDecoded);
			Assert.AreEqual(".", codeElement[1].HtmlDecoded);
		}

		[Test]
		public void read_code_block_from_method() {
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var codeBlock = docs.Remarks
				.First()
				.Children
				.OfType<ParsedXmlCode>()
				.FirstOrDefault();
			Assert.IsNotNull(codeBlock);
			Assert.IsFalse(codeBlock.Inline);
			Assert.AreEqual("This\n is\n  some\n   text.", codeBlock.Node.InnerXml);
		}

		[Test]
		public void read_examples_from_method() {
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var examples = docs.Examples;
			Assert.IsNotNull(examples);
			Assert.AreEqual(2, examples.Count);
			Assert.AreEqual("Example 1", examples[0].Node.InnerXml);
			Assert.AreEqual("Example 2", examples[1].Node.InnerXml);
		}

		[Test]
		public void read_list_from_method() {
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var summary = docs.Summary;
			Assert.IsNotNull(summary);
			var list = summary.Children.OfType<ParsedXmlListElement>().Single();
			Assert.IsNotNull(list);
			Assert.AreEqual("bullet", list.ListType);
			var items = list.Items.ToList();
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("Col 1", items[0].Term.Node.InnerXml);
			Assert.AreEqual("Col 2", items[0].Description.Node.InnerXml);
			Assert.IsTrue(items[0].IsHeader);
			Assert.AreEqual("A term.", items[1].Term.Node.InnerXml);
			Assert.AreEqual("A description.", items[1].Description.Node.InnerXml);
			Assert.IsFalse(items[1].IsHeader);
		}

		[Test]
		public void read_para_from_methid(){
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.Finalize");
			var docs = XmlDocOverlay.GetDocumentation(method) as ParameterizedXmlDocBase;
			Assert.IsNotNull(docs);
			var paragraphs = docs.Remarks.First().Children.OfType<ParsedXmlParagraphElement>().ToList();
			Assert.AreEqual(3, paragraphs.Count);
			Assert.AreEqual("a paragraph", paragraphs[0].Node.InnerXml.Trim());
			Assert.AreEqual("and another", paragraphs[1].Node.InnerXml.Trim());
			Assert.AreEqual("a third", paragraphs[2].Node.InnerXml.Trim());
		}

		[Test]
		public void paramref_from_property(){
			var property = CRefOverlay.GetMemberDefinition("P:TestLibrary1.Class1.Item(System.Int32)");
			var docs = XmlDocOverlay.GetDocumentation(property) as PropertyDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			var paramref = docs.Remarks.First().Children.OfType<ParsedXmlParamrefElement>().Single();
			Assert.AreEqual("index", paramref.Node.InnerXml);
			Assert.AreEqual("n", paramref.ParameterName);
			Assert.IsNotNull(paramref.Target);
			Assert.AreEqual("n", paramref.Target.Name);
		}

		[Test]
		public void paramref_from_method(){
			var method = CRefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
			var docs = XmlDocOverlay.GetDocumentation(method) as MethodDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			var paramref = docs.Remarks.First().Children.OfType<ParsedXmlParamrefElement>().Single();
			Assert.AreEqual("crap", paramref.ParameterName);
			Assert.IsNotNull(paramref.Target);
			Assert.AreEqual("crap", paramref.Target.Name);
		}

		[Test]
		public void paramref_from_delegate(){
			var d = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.MyFunc");
			var docs = XmlDocOverlay.GetDocumentation(d) as DelegateTypeDefinitionXmlDoc;
			Assert.IsNotNull(docs);
			var paramref = docs.Remarks.First().Children.OfType<ParsedXmlParamrefElement>().Single();
			Assert.AreEqual("a", paramref.ParameterName);
			Assert.IsNotNull(paramref.Target);
			Assert.AreEqual("a", paramref.Target.Name);
		}

		[Test]
		public void permission_from_field(){
			var f = CRefOverlay.GetMemberDefinition("F:TestLibrary1.Class1.MyConst");
			var docs = XmlDocOverlay.GetDocumentation(f);
			Assert.AreEqual(1, docs.Permissions.Count);
			Assert.AreEqual("T:System.Security.PermissionSet",docs.Permissions[0].CRef);
			Assert.AreEqual("I have no idea what this is for.",docs.Permissions[0].Node.InnerXml);
		}

		[Test]
		public void see_and_seealso_from_event(){
			var e = CRefOverlay.GetMemberDefinition("E:TestLibrary1.Class1.DoStuff");
			var docs = XmlDocOverlay.GetDocumentation(e);
			var see = docs.Remarks.First().Children.OfType<ParsedXmlSeeElement>().Single();
			Assert.AreEqual("T:TestLibrary1.Class1", see.CRef);
			Assert.IsNotNull(see.CrefTarget);
			Assert.AreEqual("Class1", see.CrefTarget.Name);
			Assert.AreEqual(2, docs.SeeAlso.Count);
			Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", docs.SeeAlso[0].CRef);
			Assert.IsNotNull(docs.SeeAlso[0].CrefTarget);
			Assert.AreEqual("The delegate.", docs.SeeAlso[0].Node.InnerXml);
			Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Int32)", docs.SeeAlso[1].CRef);
			Assert.IsNotNull(docs.SeeAlso[1].CrefTarget);
		}

		[Test]
		public void typeparamref_on_self_class(){
			var c = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Generic1`2");
			var docs = XmlDocOverlay.GetDocumentation(c);
			var typeparamref = docs.Summary.Children.OfType<ParsedXmlTypeparamrefElement>().Single();
			Assert.AreEqual("TA", typeparamref.TypeparamName);
			Assert.IsNotNull(typeparamref.Target);
		}

		[Test]
		public void typeparamref_to_parent_class(){
			var m = CRefOverlay.GetMemberDefinition(
				"M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})");
			var docs = XmlDocOverlay.GetDocumentation(m);
			var typeparamref = docs.Summary.Children.OfType<ParsedXmlTypeparamrefElement>().Single();
			Assert.AreEqual("TA", typeparamref.TypeparamName);
			Assert.IsNotNull(typeparamref.Target);
		}

		[Test]
		public void typeparamref_to_delegate_generic(){
			var d = CRefOverlay.GetTypeDefinition("T:TestLibrary1.Generic1`2.MyFunc`1");
			var docs = XmlDocOverlay.GetDocumentation(d);
			var typeparamref = docs.Summary.Children.OfType<ParsedXmlTypeparamrefElement>().Single();
			Assert.AreEqual("TX",typeparamref.TypeparamName);
			Assert.IsNotNull(typeparamref.Target);
		}

	}
}
