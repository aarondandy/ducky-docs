using System.Linq;
using DandyDoc.XmlDoc;
using NUnit.Framework;

#pragma warning disable 1591

namespace DandyDoc.Core.Tests
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
            Assert.IsFalse(firstRequire.RequiresParameterNotEqualNull("text"));
            Assert.IsTrue(firstRequire.RequiresParameterNotNullOrEmpty("text"));
            Assert.IsFalse(firstRequire.RequiresParameterNotNullOrWhiteSpace("text"));
            Assert.IsTrue(firstRequire.RequiresParameterNotEverNull("text"));
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
            Assert.IsFalse(ensures.EnsuresResultNotEqualNull);
            Assert.IsTrue(ensures.EnsuresResultNotNullOrEmpty);
            Assert.IsFalse(ensures.EnsuresResultNotNullOrWhiteSpace);
            Assert.IsTrue(ensures.EnsuresResultNotEverNull);
        }

        [Test]
        public void check_property_not_nulls() {
            var docs = Docs.GetMember("P:TestLibrary1.ClassWithContracts.Stuff");
            Assert.IsNotNull(docs);
            Assert.IsTrue(docs.HasGetterElement);
            var ensures = docs.GetterElement.EnsuresElements.Single();
            Assert.IsTrue(ensures.EnsuresResultNotEqualNull);
            Assert.IsTrue(docs.HasSetterElement);
            var requires = docs.SetterElement.RequiresElements.Single();
            Assert.IsTrue(requires.RequiresParameterNotEqualNull("value"));

        }

        [Test]
        public void check_requires_has_related_exception() {
            var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.#ctor(System.String)");
            Assert.IsNotNull(docs);
            Assert.IsTrue(docs.RequiresElements.First().RequiresParameterNotNullOrEmpty("text"));
            var secondRequires = docs.RequiresElements.Skip(1).Single();
            Assert.AreEqual("!text.Equals(\"nope\")", secondRequires.Node.InnerText);
            var priorSibling = secondRequires.PriorElement;
            Assert.IsNotNull(priorSibling);
            var exception = priorSibling as XmlDocRefElement;
            Assert.IsNotNull(exception);
            Assert.AreEqual("T:System.ArgumentException", exception.CRef);
        }

    }
}
