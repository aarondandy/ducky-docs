using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class XmlDocCodeContracts
	{

		public XmlDocCodeContracts() {
			Docs = new XmlAssemblyDocumentation("./TestLibrary1.XML");
		}

		public XmlAssemblyDocumentation Docs { get; private set; }

		[Test]
		public void check_purity() {
			var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.SomeStuff");
			Assert.IsNotNull(docs);
			Assert.IsTrue(docs.HasPureElement);
		}

		[Test]
		public void check_invariant() {
			var docs = Docs.GetMember("T:TestLibrary1.ClassWithContracts");
			Assert.IsNotNull(docs);
			var invariant = docs.InvariantElements.Single();
			Assert.IsTrue(invariant.IsInvariant);
			Assert.AreEqual("!String.IsNullOrEmpty(Text)", invariant.Node.InnerXml);
		}

		[Test]
		public void check_requires_method() {
			var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.#ctor(System.String)");
			Assert.IsNotNull(docs);
			var requires = docs.RequiresElements.ToList();
			Assert.IsTrue(requires.All(r => r.IsRequires));
			var firstRequire = requires.First();
			Assert.AreEqual("!IsNullOrEmpty(text)", firstRequire.CSharp);
			Assert.AreEqual("Not IsNullOrEmpty(text)", firstRequire.VisualBasic);
			Assert.AreEqual("!string.IsNullOrEmpty(text)", firstRequire.Element.InnerXml);
			Assert.AreEqual("T:System.ArgumentException", firstRequire.CRef);
		}

		[Test]
		public void check_ensures_property() {
			var docs = Docs.GetMember("P:TestLibrary1.ClassWithContracts.Text");
			Assert.IsNotNull(docs);
			Assert.IsTrue(docs.HasGetterElement);
			var ensures = docs.GetterElement.EnsuresElements.Single();
			Assert.IsTrue(ensures.IsNormalEnsures);
			Assert.AreEqual("!IsNullOrEmpty(result)", ensures.CSharp);
			Assert.AreEqual("Not IsNullOrEmpty(result)", ensures.VisualBasic);
			Assert.AreEqual("!string.IsNullOrEmpty(result)", ensures.Element.InnerXml);
		}

		[Test]
		public void check_requires_has_related_exception() {
			var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.#ctor(System.String)");
			Assert.IsNotNull(docs);
			var requires = docs.RequiresElements.Skip(1).First();
			var priorSibling = requires.PriorElement;
			Assert.IsNotNull(priorSibling);
			var exception = priorSibling as XmlDocRefElement;
			Assert.IsNotNull(exception);
			Assert.AreEqual("T:System.ArgumentException",exception.CRef);
		}

	}
}
