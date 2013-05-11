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

        public virtual ICodeDocEntityRepository TestLibrary1Repository {
            get {
                var testLib1Asm = typeof(Class1).Assembly;
                var testLib1AsmPath = ReflectionUtilities.GetFilePath(testLib1Asm);
                var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
                return new ReflectionCodeDocEntityRepository(
                    new ReflectionCRefLookup(testLib1Asm),
                    new XmlAssemblyDocumentation(testLib1XmlPath)
                );
            }
        }

        [Test]
        public void invalid_requests() {
            Assert.Throws<ArgumentException>(
                () => TestLibrary1Repository.GetContentEntity(String.Empty));
            Assert.Throws<ArgumentNullException>(
                () => TestLibrary1Repository.GetContentEntity((CRefIdentifier)null));
        }

        [Test]
        public void namespace_assembly_test() {
            Assert.AreEqual(1, TestLibrary1Repository.Assemblies.Count);
            Assert.AreEqual(5, TestLibrary1Repository.Namespaces.Count);
            Assert.Less(0, TestLibrary1Repository.Assemblies.Single().TypeCRefs.Count);
            Assert.AreEqual(
                TestLibrary1Repository.Assemblies.Sum(x => x.TypeCRefs.Count),
                TestLibrary1Repository.Namespaces.Sum(x => x.Types.Count)
            );
        }

        [Test]
        public void type_test_for_Class1() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1") as CodeDocType;
            Assert.AreEqual("Class1", model.ShortName);
            Assert.AreEqual("TestLibrary1.Class1", model.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1", model.CRef.FullCRef);
            Assert.AreEqual("Class1", model.Title);
            Assert.AreEqual("Class", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);
            Assert.IsNotNull(model.Namespace);
            Assert.AreEqual("N:TestLibrary1", model.Namespace.CRef.FullCRef);
            Assert.IsNotNull(model.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, model.Assembly.CRef.FullCRef);
            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.IsFalse(model.HasInterfaces);
            Assert.IsNull(model.DeclaringType);
        }

        [Test]
        public void type_xmldoc_test_for_Class1() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1") as CodeDocType;

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
        public void type_members_test_for_Class1() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1") as CodeDocType;

            Assert.IsTrue(model.HasNestedTypes);
            Assert.AreEqual(6, model.NestedTypes.Count);
            Assert.Contains("Inherits", model.NestedTypes.Select(x => x.ShortName).ToList());
            Assert.Contains("Inner", model.NestedTypes.Select(x => x.ShortName).ToList());
            Assert.Contains("NoRemarks", model.NestedTypes.Select(x => x.ShortName).ToList());
            Assert.Contains("NoDocs", model.NestedTypes.Select(x => x.ShortName).ToList());
            Assert.Contains("ProtectedStruct", model.NestedTypes.Select(x => x.ShortName).ToList());
            Assert.Contains("IThing", model.NestedTypes.Select(x => x.ShortName).ToList());

            Assert.AreEqual(
                "no remarks here",
                model.NestedTypes
                    .First(x => x.ShortName == "NoRemarks")
                    .Summary.Node.InnerText);

            Assert.IsTrue(model.HasNestedDelegates);
            Assert.AreEqual(1, model.NestedDelegates.Count);
            Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", model.NestedDelegates.Single().CRef.FullCRef);

            Assert.IsTrue(model.HasConstructors);
            Assert.AreEqual(2, model.Constructors.Count);
            Assert.That(model.Constructors, Has.All.Property("SubTitle").EqualTo("Constructor"));

            Assert.IsTrue(model.HasMethods);
            Assert.Less(5, model.Methods.Count);
            Assert.That(model.Methods, Has.All.Property("SubTitle").EqualTo("Method"));

            Assert.IsTrue(model.HasOperators);
            Assert.AreEqual(1, model.Operators.Count);
            Assert.That(model.Operators.Single().Title.Contains('+'));
            Assert.That(model.Operators, Has.All.Property("SubTitle").EqualTo("Operator"));

            Assert.IsTrue(model.HasEvents);
            Assert.AreEqual(2, model.Events.Count);

            Assert.IsTrue(model.HasFields);
            Assert.AreEqual(5, model.Fields.Count);
            Assert.That(model.Fields, Has.Some.Property("SubTitle").EqualTo("Constant"));
            Assert.That(model.Fields, Has.Some.Property("SubTitle").EqualTo("Field"));

            Assert.IsTrue(model.HasProperties);
            Assert.AreEqual(3, model.Properties.Count);
            Assert.That(model.Properties, Has.Some.Property("SubTitle").EqualTo("Indexer"));
            Assert.That(model.Properties, Has.Some.Property("SubTitle").EqualTo("Property"));
        }

        [Test]
        public void type_test_for_FlagsEnum() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.FlagsEnum") as CodeDocType;
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
                new[] {
                    new CRefIdentifier("T:System.Enum"),
                    new CRefIdentifier("T:System.ValueType"),
                    new CRefIdentifier("T:System.Object")
                },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.IsTrue(model.HasInterfaces);
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IComparable")));
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IConvertible")));
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IFormattable")));
        }

        [Test]
        public void type_test_for_Class1_Inner() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.Inner") as CodeDocType;
            Assert.IsNotNull(model);
            Assert.AreEqual("Inner", model.ShortName);
            Assert.AreEqual("TestLibrary1.Class1.Inner", model.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1.Inner", model.CRef.FullCRef);
            Assert.AreEqual("Inner", model.Title);
            Assert.AreEqual("Class", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);
            Assert.IsNotNull(model.Namespace);
            Assert.AreEqual("N:TestLibrary1", model.Namespace.CRef.FullCRef);
            Assert.IsNotNull(model.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", model.DeclaringType.CRef.FullCRef);

            Assert.IsFalse(model.HasSummary);
            Assert.IsFalse(model.HasExamples);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasSeeAlso);
            Assert.IsTrue(model.HasRemarks);
            Assert.AreEqual(1, model.Remarks.Count);
            Assert.AreEqual("This is just some class.", model.Remarks[0].Node.InnerText);

            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.IsFalse(model.HasInterfaces);
        }

        [Test]
        public void type_test_for_Generic1() {
            var model = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Generic1`2") as CodeDocType;
            Assert.IsNotNull(model);
            Assert.AreEqual("Generic1<TA, TB>", model.ShortName);
            Assert.IsNull(model.DeclaringType);

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
            Assert.AreEqual("T:System.ValueType", model.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);

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
            // TODO: perhaps this would be better as an ICodeDocEntity
            Assert.AreEqual("T:System.Collections.Generic.IEnumerable{`0}", model.GenericParameters[1].TypeConstraints[0].CRef.FullCRef);
        }

        [Test]
        public void type_test_for_generic_variance() {
            var model = TestLibrary1Repository
                .GetContentEntity("T:TestLibrary1.Generic1`2.IVariance`2") as CodeDocType;
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
                .GetContentEntity("TestLibrary1.Generic1`2.Constraints`1") as CodeDocType;
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
                model.GenericParameters[0].TypeConstraints.Select(x => x.CRef).ToArray());
        }

        [Test]
        public void field_array_of_ref_type_tests() {
            var field = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.SomeClasses") as CodeDocField;
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeClasses"), field.CRef);
            Assert.AreEqual("SomeClasses", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:TestLibrary1.Class1[]"), field.ValueType.CRef);
            Assert.IsFalse(field.HasValueDescription);
            Assert.IsFalse(field.IsLiteral);
            Assert.IsFalse(field.IsInitOnly);
            Assert.IsFalse(field.IsStatic);
            Assert.IsNotNull(field.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", field.DeclaringType.CRef.FullCRef);
        }

        [Test]
        public void field_nullable_int_test() {
            var field = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.SomeNullableInt") as CodeDocField;
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeNullableInt"), field.CRef);
            Assert.AreEqual("SomeNullableInt", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Nullable{System.Int32}"), field.ValueType.CRef);
            Assert.IsNotNull(field.Namespace);
            Assert.AreEqual("N:TestLibrary1", field.Namespace.CRef.FullCRef);
            Assert.IsNotNull(field.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, field.Assembly.CRef.FullCRef);
            Assert.IsFalse(field.HasValueDescription);
            Assert.IsFalse(field.IsLiteral);
            Assert.IsFalse(field.IsInitOnly);
            Assert.IsFalse(field.IsStatic);
        }

        [Test]
        public void field_const_int_test() {
            var field = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.MyConst") as CodeDocField;
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.MyConst"), field.CRef);
            Assert.AreEqual("MyConst", field.ShortName);
            Assert.AreEqual("Constant", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.IsTrue(field.HasValueDescription);
            Assert.AreEqual("1", field.ValueDescription.Node.InnerText.Trim());
            Assert.IsTrue(field.IsLiteral);
            Assert.IsFalse(field.IsInitOnly);
            Assert.IsTrue(field.IsStatic);
        }

        [Test]
        public void field_static_double() {
            var field = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.SomeField") as CodeDocField;
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeField"), field.CRef);
            Assert.AreEqual("SomeField", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Double"), field.ValueType.CRef);
            Assert.IsTrue(field.HasValueDescription);
            Assert.AreEqual("A double value.", field.ValueDescription.Node.InnerText);
            Assert.IsFalse(field.IsLiteral);
            Assert.IsFalse(field.IsInitOnly);
            Assert.IsTrue(field.IsStatic);
        }

        [Test]
        public void field_readonly_int() {
            var field = TestLibrary1Repository
                .GetContentEntity("TestLibrary1.Class1.ReadonlyField") as CodeDocField;
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.ReadonlyField"), field.CRef);
            Assert.AreEqual("ReadonlyField", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.IsFalse(field.HasValueDescription);
            Assert.IsFalse(field.IsLiteral);
            Assert.IsTrue(field.IsInitOnly);
            Assert.IsFalse(field.IsStatic);
        }

        [Test]
        public void method_strange_test() {
            var method = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])", method.CRef.FullCRef);
            Assert.AreEqual("HasStrangeParams(Nullable<Int32>, Class1[])", method.ShortName);
            Assert.AreEqual("Method", method.SubTitle);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.HasReturn);
            Assert.IsNotNull(method.Namespace);
            Assert.AreEqual("N:TestLibrary1", method.Namespace.CRef.FullCRef);
            Assert.IsNotNull(method.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, method.Assembly.CRef.FullCRef);

            Assert.IsTrue(method.HasParameters);
            Assert.AreEqual(2, method.Parameters.Count);
            Assert.AreEqual("a", method.Parameters[0].Name);
            Assert.AreEqual("T:System.Nullable{System.Int32}", method.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.AreEqual("someClass", method.Parameters[1].Name);
            Assert.AreEqual("T:TestLibrary1.Class1[]", method.Parameters[1].ParameterType.CRef.FullCRef);

            Assert.IsTrue(method.HasExceptions);
            Assert.AreEqual(1, method.Exceptions.Count);
            Assert.AreEqual("T:System.NotImplementedException", method.Exceptions[0].ExceptionType.CRef.FullCRef);
            Assert.IsTrue(method.Exceptions[0].HasConditions);
            Assert.AreEqual(1, method.Exceptions[0].Conditions.Count);
            Assert.AreEqual("Too lazy to implement.", method.Exceptions[0].Conditions[0].Node.InnerText);
        }

        [Test]
        public void method_one_param_ctor() {
            var method = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.#ctor(System.String)") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String)", method.CRef.FullCRef);
            Assert.AreEqual("Class1(String)", method.ShortName);
            Assert.AreEqual("Constructor", method.SubTitle);
            Assert.IsFalse(method.IsStatic);
            Assert.IsFalse(method.HasReturn);

            Assert.IsNotNull(method.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", method.DeclaringType.CRef.FullCRef);

            Assert.IsTrue(method.HasParameters);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual("crap", method.Parameters[0].Name);
            Assert.AreEqual("T:System.String", method.Parameters[0].ParameterType.CRef.FullCRef);
        }

        [Test]
        public void method_DoubleStatic() {
            var method = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.DoubleStatic(System.Double)") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Double)", method.CRef.FullCRef);
            Assert.AreEqual("DoubleStatic(Double)", method.ShortName);
            Assert.AreEqual("Method", method.SubTitle);
            Assert.IsTrue(method.IsStatic);
            Assert.IsTrue(method.HasReturn);
            Assert.AreEqual("T:System.Double", method.Return.ParameterType.CRef.FullCRef);
            Assert.IsTrue(method.Return.HasSummary);
            Assert.AreEqual("The result of doubling the value.", method.Return.Summary.Node.InnerText);

            Assert.IsTrue(method.HasParameters);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual("n", method.Parameters[0].Name);
            Assert.AreEqual("T:System.Double", method.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.IsTrue(method.Parameters[0].HasSummary);
            Assert.AreEqual("The value to double.", method.Parameters[0].Summary.Node.InnerText);
        }

        [Test]
        public void code_contract_constructor() {
            var method = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.ClassWithContracts.#ctor(System.String)") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.IsTrue(method.HasExceptions);
            Assert.AreEqual(1, method.Exceptions.Count);
            Assert.AreEqual("T:System.ArgumentException", method.Exceptions[0].ExceptionType.CRef.FullCRef);
            Assert.AreEqual(2, method.Exceptions[0].Conditions.Count);
            Assert.That(method.Exceptions[0].Conditions[0].Node.InnerText.Contains("IsNullOrEmpty(text)"));
            Assert.That(method.Exceptions[0].Conditions[1].Node.InnerText.Contains("text.Equals(\"nope\")"));
            Assert.AreEqual(2, method.Exceptions[0].Ensures.Count);
            Assert.That(method.Exceptions[0].Ensures[0].Node.InnerText.Contains("Text == null"));
            Assert.That(method.Exceptions[0].Ensures[1].Node.InnerText.Contains("Text != \"nope!\""));

            Assert.IsTrue(method.HasEnsures);
            Assert.AreEqual(2, method.Ensures.Count);
            Assert.IsFalse(method.HasNormalTerminationEnsures);
            Assert.IsTrue(method.HasRequires);
            Assert.AreEqual(2, method.Requires.Count);
        }

        [Test]
        public void code_contract_simple_ensures_method() {
            var method = TestLibrary1Repository.GetContentEntity(
                "M:TestLibrary1.ClassWithContracts.SomeStuff") as CodeDocMethod;
            Assert.IsNotNull(method);

            Assert.IsFalse(method.HasRequires);
            Assert.IsTrue(method.HasEnsures);
            Assert.AreEqual(1, method.Ensures.Count);
            Assert.IsTrue(method.HasNormalTerminationEnsures);
            Assert.AreEqual(1, method.NormalTerminationEnsures.Count());
            Assert.That(method.NormalTerminationEnsures.First().Node.InnerText.Contains("IsNullOrEmpty"));
        }

        [Test]
        public void method_generic() {
            var method = TestLibrary1Repository.GetContentEntity(
                "M:TestLibrary1.Generic1`2.AMix``1(`0,``0)") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Generic1`2", method.DeclaringType.CRef.FullCRef);

            Assert.IsTrue(method.HasGenericParameters);
            Assert.AreEqual(1, method.GenericParameters.Count);
            Assert.AreEqual("TOther", method.GenericParameters[0].Name);
            Assert.IsFalse(method.GenericParameters[0].HasSummary);
            Assert.IsFalse(method.GenericParameters[0].HasTypeConstraints);
        }

        [Test]
        public void method_generic_constraints() {
            var method = TestLibrary1Repository.GetContentEntity(
                "M:TestLibrary1.Generic1`2.Constraints`1.GetStuff``1(`2,``0)") as CodeDocMethod;
            Assert.IsNotNull(method);

            Assert.IsTrue(method.HasGenericParameters);
            Assert.AreEqual(1, method.GenericParameters.Count);
            Assert.AreEqual("TStuff", method.GenericParameters[0].Name);
            Assert.AreEqual("some stuff", method.GenericParameters[0].Summary.Node.InnerText);
            Assert.IsTrue(method.GenericParameters[0].HasTypeConstraints);
            Assert.AreEqual(1, method.GenericParameters[0].TypeConstraints.Count);
            Assert.AreEqual("T:System.IConvertible", method.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);
        }

        [Test]
        public void delegate_with_comments() {
            var type = TestLibrary1Repository.GetContentEntity(
                "T:TestLibrary1.Class1.MyFunc") as CodeDocDelegate;
            Assert.IsNotNull(type);
            Assert.IsNotNull(type.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", type.DeclaringType.CRef.FullCRef);
            Assert.IsTrue(type.HasSummary);
            Assert.That(type.Summary.Node.InnerText.Contains("My delegate."));
            Assert.IsTrue(type.HasRemarks);

            Assert.IsFalse(type.HasGenericParameters);
            Assert.IsTrue(type.HasParameters);
            Assert.AreEqual(2, type.Parameters.Count);
            Assert.AreEqual("a", type.Parameters[0].Name);
            Assert.AreEqual("param a", type.Parameters[0].Summary.Node.InnerText);
            Assert.AreEqual("T:System.Int32", type.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.AreEqual("b", type.Parameters[1].Name);
            Assert.AreEqual("param b", type.Parameters[1].Summary.Node.InnerText);
            Assert.AreEqual("T:System.Int32", type.Parameters[1].ParameterType.CRef.FullCRef);
            Assert.IsTrue(type.HasReturn);
            Assert.AreEqual("T:System.Int32", type.Return.ParameterType.CRef.FullCRef);
            Assert.AreEqual("some int", type.Return.Summary.Node.InnerText);
        }

        [Test]
        public void delegate_with_generic_arg() {
            var type = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Generic1`2.MyFunc`1") as CodeDocDelegate;
            Assert.IsNotNull(type);

            Assert.IsTrue(type.HasGenericParameters);
            Assert.AreEqual(1, type.GenericParameters.Count);
            Assert.AreEqual("TX", type.GenericParameters[0].Name);
        }

        [Test]
        public void event_test() {
            var evt = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.DoStuff") as CodeDocEvent;
            Assert.IsNotNull(evt);
            Assert.IsNotNull(evt.DeclaringType);
            Assert.IsNotNull(evt.Namespace);
            Assert.AreEqual("N:TestLibrary1", evt.Namespace.CRef.FullCRef);
            Assert.IsNotNull(evt.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, evt.Assembly.CRef.FullCRef);
            Assert.AreEqual("T:TestLibrary1.Class1", evt.DeclaringType.CRef.FullCRef);
            Assert.AreEqual("DoStuff", evt.ShortName);
            Assert.AreEqual("DoStuff", evt.Title);
            Assert.AreEqual("Event", evt.SubTitle);
            Assert.AreEqual("E:TestLibrary1.Class1.DoStuff", evt.CRef.FullCRef);
            Assert.AreEqual("TestLibrary1.Class1.DoStuff", evt.FullName);
            Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", evt.DelegateType.CRef.FullCRef);
            Assert.IsTrue(evt.HasSummary);
            Assert.That(evt.Summary.Node.InnerText.Contains("My event!"));
            Assert.IsTrue(evt.HasRemarks);
            Assert.AreEqual(1, evt.Remarks.Count);
            Assert.That(evt.Remarks[0].Node.InnerText.Contains("stuff"));
        }

        [Test]
        public void property_test() {
            var prop = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.HasTableInRemarks") as CodeDocProperty;
            Assert.IsNotNull(prop);
            Assert.IsNotNull(prop.DeclaringType);
            Assert.IsNotNull(prop.Namespace);
            Assert.AreEqual("N:TestLibrary1", prop.Namespace.CRef.FullCRef);
            Assert.IsNotNull(prop.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, prop.Assembly.CRef.FullCRef);
            Assert.AreEqual("T:TestLibrary1.Class1", prop.DeclaringType.CRef.FullCRef);
            Assert.AreEqual("HasTableInRemarks", prop.Title);
            Assert.AreEqual("HasTableInRemarks", prop.ShortName);
            Assert.AreEqual("Property", prop.SubTitle);
            Assert.AreEqual("TestLibrary1.Class1.HasTableInRemarks", prop.FullName);
            Assert.AreEqual("P:TestLibrary1.Class1.HasTableInRemarks", prop.CRef.FullCRef);
            Assert.IsTrue(prop.HasSummary);
            Assert.That(prop.Summary.Node.InnerText.Contains("This has a table in the remarks section."));
            Assert.IsTrue(prop.HasRemarks);
            Assert.IsFalse(prop.HasGetter);
            Assert.IsTrue(prop.HasSetter);
            Assert.IsNotNull(prop.Setter);
        }

        [Test]
        public void property_indexer_test() {
            var prop = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.Item(System.Int32)") as CodeDocProperty;
            Assert.IsNotNull(prop);

            Assert.AreEqual("T:System.Int32", prop.ValueType.CRef.FullCRef);
            Assert.IsTrue(prop.HasSummary);
            Assert.IsTrue(prop.HasValueDescription);
            Assert.AreEqual("Some number.", prop.ValueDescription.Node.InnerText);

            Assert.IsTrue(prop.HasParameters);
            Assert.AreEqual(1, prop.Parameters.Count);
            Assert.AreEqual("n", prop.Parameters[0].Name);
            Assert.AreEqual("T:System.Int32", prop.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.IsTrue(prop.Parameters[0].HasSummary);
            Assert.AreEqual("an index", prop.Parameters[0].Summary.Node.InnerText);
        }

        [Test]
        public void out_ref_method_test() {
            var method = TestLibrary1Repository.GetContentEntity(
                "TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32@,System.Int32@)") as CodeDocMethod;
            Assert.IsNotNull(method);
            Assert.IsTrue(method.Parameters[0].IsOut);
            Assert.IsTrue(method.Parameters[0].IsByRef);
            Assert.IsFalse(method.Parameters[1].IsOut);
            Assert.IsTrue(method.Parameters[1].IsByRef);
        }

    }
}
