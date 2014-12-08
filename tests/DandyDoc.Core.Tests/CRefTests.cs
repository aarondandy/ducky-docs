using DuckyDocs.CRef;
using Xunit;

namespace DuckyDocs.Core.Tests
{

    public class CRefTests
    {
        [Fact]
        public void generic_type_instance_to_definition_cref() {
            var genericInstanceCRef = new CRefIdentifier("T:Fake.Type{System.Int32}");
            var expectedCRef = new CRefIdentifier("T:Fake.Type`1");
            var actualCRef = CRefTransformer.FullSimplification.Transform(genericInstanceCRef);
            Assert.Equal(expectedCRef, actualCRef);
        }

        [Fact]
        public void generic_method_instance_to_definition_cref() {
            var genericInstanceCRef = new CRefIdentifier("M:Fake.Type{System.Int32}.Method{System.String}( Fake.IEnumerable{`0}, ``0)");
            var expectedCRef = new CRefIdentifier("M:Fake.Type`1.Method``1( Fake.IEnumerable`1, ``0)");
            // NOTE: Whitespace is preserved in the parameters. Good or bad?
            var actualCRef = CRefTransformer.FullSimplification.Transform(genericInstanceCRef);
            Assert.Equal(expectedCRef, actualCRef);
        }

        [Fact]
        public void check_generated_uri() {
            var cRef = new CRefIdentifier("!:Fake{Fake.Enumerable{System.Int32}}.Method(``0)");
            var uri = cRef.ToUri();
            Assert.NotNull(uri);
            Assert.Equal("cref",uri.Scheme);

            Assert.Equal(
                "%21%3AFake%7BFake.Enumerable%7BSystem.Int32%7D%7D.Method%28%60%600%29",
                uri.PathAndQuery);
        }

        [Fact]
        public void cref_to_uri_round_trip() {
            var expectedCRef = new CRefIdentifier("!:Fake{Fake.Enumerable{System.Int32}}.Method(``0)");

            var uri = expectedCRef.ToUri();
            CRefIdentifier actualCRef;
            Assert.True(CRefIdentifier.TryParse(uri, out actualCRef));

            Assert.NotNull(actualCRef);
            Assert.Equal(expectedCRef, actualCRef);
        }

        [Fact]
        public void constructor_cref_to_uri_round_trip() {
            var expectedCRef = new CRefIdentifier("!:Fake.#ctor()");

            var uri = expectedCRef.ToUri();
            CRefIdentifier actualCRef;
            Assert.True(CRefIdentifier.TryParse(uri, out actualCRef));

            Assert.NotNull(actualCRef);
            Assert.Equal(expectedCRef, actualCRef);
        }

        [Fact]
        public void extract_return_type(){
            var cRef = new CRefIdentifier("M:Some.Method(Some.ParamType)~System.String");
            Assert.Equal("System.String", cRef.ReturnTypePart);
        }

    }
}
