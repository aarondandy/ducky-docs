using System;
using System.IO;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.CodeDoc.Tests
{

    [TestFixture]
    public class CodeDocTypeReflectionEntityRepositoryTest
    {

        public CodeDocTypeReflectionEntityRepositoryTest() {
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
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Class1") as CodeDocType;
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

            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChainCRefs);
            Assert.IsFalse(model.HasDirectInterfaces);
        }

        [Test]
        public void type_test_for_FlagsEnum() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.FlagsEnum") as CodeDocType;
            Assert.AreEqual("FlagsEnum", model.ShortName);
            Assert.AreEqual("TestLibrary1.FlagsEnum", model.FullName);
            Assert.AreEqual("T:TestLibrary1.FlagsEnum", model.CRef.FullCRef);
            Assert.AreEqual("FlagsEnum", model.Title);
            Assert.AreEqual("Enumeration", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);
            Assert.IsTrue(model.IsEnum);

            Assert.IsTrue(model.HasSummary);
            Assert.AreEqual("An enumeration to check detection of the flags attribute.", model.Summary.Node.InnerText);
            Assert.IsTrue(model.HasExamples);
            Assert.AreEqual(1, model.Examples.Count);
            Assert.AreEqual("FlagsEnum.AB == FlagsEnum.A | FlagsEnum.B;", model.Examples[0].Node.InnerText);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasRemarks);
            Assert.IsFalse(model.HasSeeAlso);

            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new [] {
                    new CRefIdentifier("T:System.Enum"),
                    new CRefIdentifier("T:System.ValueType"),
                    new CRefIdentifier("T:System.Object")
                },
                model.BaseChainCRefs);
            Assert.IsTrue(model.HasDirectInterfaces);
            Assert.That(model.DirectInterfaceCRefs, Contains.Item(new CRefIdentifier("T:System.IComparable")));
            Assert.That(model.DirectInterfaceCRefs, Contains.Item(new CRefIdentifier("T:System.IConvertible")));
            Assert.That(model.DirectInterfaceCRefs, Contains.Item(new CRefIdentifier("T:System.IFormattable")));
        }

        [Test]
        public void type_test_for_Class1_Inner() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Class1.Inner") as CodeDocType;
            Assert.IsNotNull(model);
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

            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new[] {new CRefIdentifier("T:System.Object")},
                model.BaseChainCRefs);
            Assert.IsFalse(model.HasDirectInterfaces);
        }

        [Test]
        public void type_test_for_Generic1() {
            var model = TestLibrary1Repository.GetEntity("TestLibrary1.Generic1`2") as CodeDocType;
            Assert.IsNotNull(model);
            Assert.AreEqual("Generic1<TA, TB>", model.ShortName);
            
            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(2, model.GenericParameters.Count);

            Assert.AreEqual("TA", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].HasSummary);
            Assert.AreEqual(
                "<typeparam name=\"TA\">A</typeparam>",
                model.GenericParameters[0].Summary.Node.OuterXml);
            Assert.IsFalse(model.GenericParameters[0].IsContravariant);
            Assert.IsFalse(model.GenericParameters[0].IsCovariant);
            Assert.IsTrue(model.GenericParameters[0].HasDefaultConstructorConstraint);
            Assert.IsFalse(model.GenericParameters[0].HasReferenceTypeConstraint);
            Assert.IsTrue(model.GenericParameters[0].HasNotNullableValueTypeConstraint);
            Assert.IsTrue(model.GenericParameters[0].HasTypeConstraints);
            Assert.AreEqual(1, model.GenericParameters[0].TypeConstraints.Count);
            Assert.AreEqual("T:System.ValueType", model.GenericParameters[0].TypeConstraints[0].FullCRef);

            Assert.AreEqual("TB", model.GenericParameters[1].Name);
            Assert.IsTrue(model.GenericParameters[1].HasSummary);
            Assert.AreEqual(
                "<typeparam name=\"TB\">B</typeparam>",
                model.GenericParameters[1].Summary.Node.OuterXml);
            Assert.IsFalse(model.GenericParameters[1].IsContravariant);
            Assert.IsFalse(model.GenericParameters[1].IsCovariant);
            Assert.IsFalse(model.GenericParameters[1].HasDefaultConstructorConstraint);
            Assert.IsTrue(model.GenericParameters[1].HasReferenceTypeConstraint);
            Assert.IsFalse(model.GenericParameters[1].HasNotNullableValueTypeConstraint);
            Assert.IsTrue(model.GenericParameters[1].HasTypeConstraints);
            Assert.AreEqual(1, model.GenericParameters[1].TypeConstraints.Count);
            // TODO: this CRef could be a problem, need a solution... without context what is `0?
            Assert.AreEqual("T:System.Collections.Generic.IEnumerable{`0}", model.GenericParameters[1].TypeConstraints[0].FullCRef);
        }

        [Test]
        public void type_test_for_generic_variance() {
            var model = TestLibrary1Repository
                .GetEntity("T:TestLibrary1.Generic1`2.IVariance`2") as CodeDocType;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(2, model.GenericParameters.Count);
            Assert.AreEqual("TIn", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].IsContravariant);
            Assert.IsFalse(model.GenericParameters[0].IsCovariant);
            Assert.AreEqual("TOut", model.GenericParameters[1].Name);
            Assert.IsFalse(model.GenericParameters[1].IsContravariant);
            Assert.IsTrue(model.GenericParameters[1].IsCovariant);
        }

        [Test]
        public void type_generic_contraint_test() {
            var model = TestLibrary1Repository
                .GetEntity("TestLibrary1.Generic1`2.Constraints`1") as CodeDocType;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(1, model.GenericParameters.Count);
            Assert.AreEqual("TConstraints", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].HasDefaultConstructorConstraint);
            Assert.IsTrue(model.GenericParameters[0].HasTypeConstraints);
            Assert.AreEqual(2, model.GenericParameters[0].TypeConstraints.Count);
            Assert.AreEqual(
                new[] {
                    new CRefIdentifier("T:System.Collections.Generic.IEnumerable{System.Int32}"),
                    new CRefIdentifier("T:System.IDisposable")
                },
                model.GenericParameters[0].TypeConstraints);
        }

    }
}
