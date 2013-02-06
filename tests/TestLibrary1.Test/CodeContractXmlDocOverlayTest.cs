﻿using System;
using System.Linq;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[Obsolete("Need to replicate these tests for new XMl doc code.")]
	[TestFixture]
	public class CodeContractXmlDocOverlayTest
	{

		public CodeContractXmlDocOverlayTest() {
			AssemblyDefinitionCollection = new AssemblyDefinitionCollection("./TestLibrary1.dll");
			CRefOverlay = new CRefOverlay(AssemblyDefinitionCollection);
			XmlDocOverlay = new XmlDocOverlay(CRefOverlay);
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CRefOverlay CRefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		[Test]
		public void check_pure(){
			var m = CRefOverlay.GetMemberDefinition("M:TestLibrary1.ClassWithContracts.SomeStuff");
			var docs = XmlDocOverlay.GetDocumentation(m);
			Assert.IsNotNull(docs);
			Assert.IsTrue(docs.HasPureElement);
		}

		[Test]
		public void check_invariant(){
			var t = CRefOverlay.GetTypeDefinition("T:TestLibrary1.ClassWithContracts");
			var docs = XmlDocOverlay.GetDocumentation(t);
			Assert.IsNotNull(docs);
			var invariant = docs.Invariants.Single();
			Assert.IsTrue(invariant.IsInvariant);
			Assert.AreEqual("!String.IsNullOrEmpty(Text)", invariant.Node.InnerXml);
		}

		[Test]
		public void check_requires_method(){
			var m = CRefOverlay.GetMemberDefinition("M:TestLibrary1.ClassWithContracts.#ctor(System.String)") as MethodDefinition;
			var docs = XmlDocOverlay.GetDocumentation(m);
			Assert.IsNotNull(docs);
			var requires = docs.Requires.ToList();
			Assert.IsTrue(requires.All(r => r.IsRequires));
			var firstRequire = requires.First();
			Assert.AreEqual("!IsNullOrEmpty(text)", firstRequire.CSharp);
			Assert.AreEqual("Not IsNullOrEmpty(text)", firstRequire.VisualBasic);
			Assert.AreEqual("!string.IsNullOrEmpty(text)", firstRequire.Element.InnerXml);
			Assert.AreEqual("T:System.ArgumentException", firstRequire.ExceptionCref);
		}

		[Test]
		public void check_ensures_property(){
			var p = CRefOverlay.GetMemberDefinition("P:TestLibrary1.ClassWithContracts.Text") as PropertyDefinition;
			var docs = XmlDocOverlay.GetDocumentation(p);
			Assert.IsNotNull(docs);
			Assert.IsNotNull(docs.GetterDocs);
			var ensures = docs.GetterDocs.Ensures.Single();
			Assert.IsTrue(ensures.IsEnsures);
			Assert.AreEqual("!IsNullOrEmpty(result)", ensures.CSharp);
			Assert.AreEqual("Not IsNullOrEmpty(result)", ensures.VisualBasic);
			Assert.AreEqual("!string.IsNullOrEmpty(result)", ensures.Element.InnerXml);
		}

	}
}
