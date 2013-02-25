using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DandyDoc.CRef;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.CodeDoc.Tests
{

    [TestFixture]
    public class ReflectionCodeDocEntityRepositoryTest
    {

        public ReflectionCodeDocEntityRepositoryTest() {
            var testLib1Asm = typeof(Class1).Assembly;
            var testLib1AsmPath = ReflectionUtilities.GetFilePath(testLib1Asm);
            var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
            TestLibrary1Repository = new ReflectionCodeDocEntityRepository(
                new ReflectionCRefLookup(new[] { testLib1Asm }),
                new[]{new XmlAssemblyDocumentation(testLib1XmlPath)}
            );
        }

        public ReflectionCodeDocEntityRepository TestLibrary1Repository { get; set; }

        [Test]
        public void invalid_requests() {
            Assert.Throws<ArgumentException>(
                () => TestLibrary1Repository.GetEntity(String.Empty));
            Assert.Throws<ArgumentNullException>(
                () => TestLibrary1Repository.GetEntity((CRefIdentifier)null));
        }

        [Test]
        public void type_test_for_Class1(){
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Class1");
            Assert.AreEqual("Class1", model.ShortName);
            Assert.AreEqual("TestLibrary1.Class1", model.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1", model.CRef.FullCRef);
            Assert.AreEqual("Class1", model.Title);
            Assert.AreEqual("Class", model.SubTitle);
        }

        [Test]
        public void type_test_for_FlagsEnum() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.FlagsEnum");
            Assert.AreEqual("FlagsEnum", model.ShortName);
            Assert.AreEqual("TestLibrary1.FlagsEnum", model.FullName);
            Assert.AreEqual("T:TestLibrary1.FlagsEnum", model.CRef.FullCRef);
            Assert.AreEqual("FlagsEnum", model.Title);
            Assert.AreEqual("Enumeration", model.SubTitle);
        }

        [Test]
        public void type_test_for_Class1_Inner() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Class1.Inner");
            Assert.AreEqual("Inner", model.ShortName);
            Assert.AreEqual("TestLibrary1.Class1.Inner", model.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1.Inner", model.CRef.FullCRef);
            Assert.AreEqual("Inner", model.Title);
            Assert.AreEqual("Class", model.SubTitle);
        }

    }
}
