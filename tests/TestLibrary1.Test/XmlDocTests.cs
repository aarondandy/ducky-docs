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
		public void read_summary_from_class() {
			var docs = Docs.GetMember("T:TestLibrary1.Class1");
			Assert.IsNotNull(docs.SummaryElement);
			Assert.IsTrue(docs.HasSummaryContents);
			Assert.AreEqual(1, docs.SummaryContents.Count);
			Assert.AreEqual(
				"This class is just for testing and has no real use outside of generating some documentation.",
				((XmlDocTextNode)(docs.SummaryContents[0])).HtmlDecoded
			);
		}

	}
}
