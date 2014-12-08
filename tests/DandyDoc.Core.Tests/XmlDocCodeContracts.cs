using System.Linq;
using DuckyDocs.XmlDoc;
using System.IO;
using Xunit;

namespace DuckyDocs.Core.Tests
{
    public class XmlDocCodeContracts
    {

        public XmlDocCodeContracts() {
            var testDllLocation = typeof(TestLibrary1.FlagsEnum).Assembly.Location;
            var textXmlLocation = Path.ChangeExtension(testDllLocation, "XML");
            Docs = new XmlAssemblyDocument(textXmlLocation);
        }

        public XmlAssemblyDocument Docs { get; private set; }

        [Fact]
        public void check_purity() {
            var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.SomeStuff");
            Assert.NotNull(docs);
            Assert.True(docs.HasPureElement);
        }

        [Fact]
        public void check_invariant() {
            var docs = Docs.GetMember("T:TestLibrary1.ClassWithContracts");
            Assert.NotNull(docs);
            var invariant = docs.InvariantElements.Single();
            Assert.True(invariant.IsInvariant);
            Assert.Equal("!String.IsNullOrEmpty(Text)", invariant.Node.InnerXml);
        }

        [Fact]
        public void check_requires_method() {
            var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.#ctor(System.String)");
            Assert.NotNull(docs);
            var requires = docs.RequiresElements.ToList();
            Assert.True(requires.All(r => r.IsRequires));
            var firstRequire = requires.First();
            Assert.Equal("!IsNullOrEmpty(text)", firstRequire.CSharp);
            Assert.Equal("Not IsNullOrEmpty(text)", firstRequire.VisualBasic);
            Assert.Equal("!string.IsNullOrEmpty(text)", firstRequire.Element.InnerXml);
            Assert.Equal("T:System.ArgumentException", firstRequire.CRef);
            Assert.False(firstRequire.RequiresParameterNotEqualNull("text"));
            Assert.True(firstRequire.RequiresParameterNotNullOrEmpty("text"));
            Assert.False(firstRequire.RequiresParameterNotNullOrWhiteSpace("text"));
            Assert.True(firstRequire.RequiresParameterNotEverNull("text"));
        }

        [Fact]
        public void check_ensures_property() {
            var docs = Docs.GetMember("P:TestLibrary1.ClassWithContracts.Text");
            Assert.NotNull(docs);
            Assert.True(docs.HasGetterElement);
            var ensures = docs.GetterElement.EnsuresElements.Single();
            Assert.True(ensures.IsNormalEnsures);
            Assert.Equal("!IsNullOrEmpty(result)", ensures.CSharp);
            Assert.Equal("Not IsNullOrEmpty(result)", ensures.VisualBasic);
            Assert.Equal("!string.IsNullOrEmpty(result)", ensures.Element.InnerXml);
            Assert.False(ensures.EnsuresResultNotEqualNull);
            Assert.True(ensures.EnsuresResultNotNullOrEmpty);
            Assert.False(ensures.EnsuresResultNotNullOrWhiteSpace);
            Assert.True(ensures.EnsuresResultNotEverNull);
        }

        [Fact]
        public void check_property_not_nulls() {
            var docs = Docs.GetMember("P:TestLibrary1.ClassWithContracts.Stuff");
            Assert.NotNull(docs);
            Assert.True(docs.HasGetterElement);
            var ensures = docs.GetterElement.EnsuresElements.Single();
            Assert.True(ensures.EnsuresResultNotEqualNull);
            Assert.True(docs.HasSetterElement);
            var requires = docs.SetterElement.RequiresElements.Single();
            Assert.True(requires.RequiresParameterNotEqualNull("value"));

        }

        [Fact]
        public void check_requires_has_related_exception() {
            var docs = Docs.GetMember("M:TestLibrary1.ClassWithContracts.#ctor(System.String)");
            Assert.NotNull(docs);
            Assert.True(docs.RequiresElements.First().RequiresParameterNotNullOrEmpty("text"));
            var secondRequires = docs.RequiresElements.Skip(1).Single();
            Assert.Equal("!text.Equals(\"nope\")", secondRequires.Node.InnerText);
            var priorSibling = secondRequires.PriorElement;
            Assert.NotNull(priorSibling);
            var exception = priorSibling as XmlDocRefElement;
            Assert.NotNull(exception);
            Assert.Equal("T:System.ArgumentException", exception.CRef);
        }
    }
}
