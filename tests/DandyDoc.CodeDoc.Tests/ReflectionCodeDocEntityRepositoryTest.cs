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
            Assert.AreEqual("TestLibrary1", model.NamespaceName);

            Assert.IsTrue(model.HasSummary);
            Assert.AreEqual("This class is just for testing and has no real use outside of generating some documentation.", model.Summary.Node.InnerText);
            Assert.IsTrue(model.HasExamples);
            Assert.AreEqual(2, model.Examples.Count);
            Assert.AreEqual("Example 1", model.Examples[0].Node.InnerText);
            Assert.AreEqual("Example 2", model.Examples[1].Node.InnerText);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsTrue(model.HasRemarks);
            Assert.AreEqual(1, model.Remarks.Count);
            Assert.AreEqual("These are some remarks.", model.Remarks[0].Node.InnerText);
            Assert.IsFalse(model.HasSeeAlso);
        }

        [Test]
        public void type_test_for_FlagsEnum() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.FlagsEnum");
            Assert.AreEqual("FlagsEnum", model.ShortName);
            Assert.AreEqual("TestLibrary1.FlagsEnum", model.FullName);
            Assert.AreEqual("T:TestLibrary1.FlagsEnum", model.CRef.FullCRef);
            Assert.AreEqual("FlagsEnum", model.Title);
            Assert.AreEqual("Enumeration", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);

            Assert.IsTrue(model.HasSummary);
            Assert.AreEqual("An enumeration to check detection of the flags attribute.", model.Summary.Node.InnerText);
            Assert.IsTrue(model.HasExamples);
            Assert.AreEqual(1, model.Examples.Count);
            Assert.AreEqual("FlagsEnum.AB == FlagsEnum.A | FlagsEnum.B;", model.Examples[0].Node.InnerText);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasRemarks);
            Assert.IsFalse(model.HasSeeAlso);
        }

        [Test]
        public void type_test_for_Class1_Inner() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Class1.Inner");
            Assert.AreEqual("Inner", model.ShortName);
            Assert.AreEqual("TestLibrary1.Class1.Inner", model.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1.Inner", model.CRef.FullCRef);
            Assert.AreEqual("Inner", model.Title);
            Assert.AreEqual("Class", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);

            Assert.IsFalse(model.HasSummary);
            Assert.IsFalse(model.HasExamples);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasSeeAlso);
            Assert.IsTrue(model.HasRemarks);
            Assert.AreEqual(1, model.Remarks.Count);
            Assert.AreEqual("This is just some class.", model.Remarks[0].Node.InnerText);
        }

    }
}
