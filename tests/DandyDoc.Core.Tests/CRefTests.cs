﻿using DandyDoc.CRef;
using NUnit.Framework;

namespace DandyDoc.Core.Tests
{

    [TestFixture]
    public class CRefTests
    {
        [Test]
        public void generic_type_instance_to_definition_cref() {
            var genericInstanceCRef = new CRefIdentifier("T:Fake.Type{System.Int32}");
            var expectedCRef = new CRefIdentifier("T:Fake.Type`1");
            var actualCRef = CRefTransformer.FullSimplification.Transform(genericInstanceCRef);
            Assert.AreEqual(expectedCRef, actualCRef);
        }

        [Test]
        public void generic_method_instance_to_definition_cref() {
            var genericInstanceCRef = new CRefIdentifier("M:Fake.Type{System.Int32}.Method{System.String}( Fake.IEnumerable{`0}, ``0)");
            var expectedCRef = new CRefIdentifier("M:Fake.Type`1.Method``1( Fake.IEnumerable`1, ``0)");
            // NOTE: Whitespace is preserved in the parameters. Good or bad?
            var actualCRef = CRefTransformer.FullSimplification.Transform(genericInstanceCRef);
            Assert.AreEqual(expectedCRef, actualCRef);
        }

        [Test]
        public void check_generated_uri() {
            var cRef = new CRefIdentifier("!:Fake{Fake.Enumerable{System.Int32}}.Method(``0)");
            var uri = cRef.ToUri();
            Assert.IsNotNull(uri);
            Assert.AreEqual("cref",uri.Scheme);

            Assert.AreEqual(
                "!%3AFake%7BFake.Enumerable%7BSystem.Int32%7D%7D.Method(%60%600)",
                uri.PathAndQuery);
        }

        [Test]
        public void cref_to_uri_round_trip() {
            var expectedCRef = new CRefIdentifier("!:Fake{Fake.Enumerable{System.Int32}}.Method(``0)");

            var uri = expectedCRef.ToUri();
            CRefIdentifier actualCRef;
            Assert.That(CRefIdentifier.TryParse(uri, out actualCRef));

            Assert.IsNotNull(actualCRef);
            Assert.AreEqual(expectedCRef, actualCRef);
        }

        [Test]
        public void constructor_cref_to_uri_round_trip() {
            var expectedCRef = new CRefIdentifier("!:Fake.#ctor()");

            var uri = expectedCRef.ToUri();
            CRefIdentifier actualCRef;
            Assert.That(CRefIdentifier.TryParse(uri, out actualCRef));

            Assert.IsNotNull(actualCRef);
            Assert.AreEqual(expectedCRef, actualCRef);
        }

        [Test]
        public void extract_return_type(){
            var cRef = new CRefIdentifier("M:Some.Method(Some.ParamType)~System.String");
            Assert.AreEqual("System.String", cRef.ReturnTypePart);
        }

    }
}
