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

	}
}
