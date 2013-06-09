using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using NUnit.Framework;
using TestLibrary1;

#pragma warning disable 1591

namespace DandyDoc.CodeDoc.Tests
{

    [TestFixture]
    public class CodeDocEntityRepositoryTest
    {

        public virtual ICodeDocMemberRepository TestLibrary1Repository {
            get {
                var testLib1Asm = typeof(Class1).Assembly;
                var testLib1AsmPath = testLib1Asm.GetFilePath();
                var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
                return new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(testLib1Asm),
                    new XmlAssemblyDocument(testLib1XmlPath)
                );
            }
        }

        public virtual CodeDocType GetCodeDocType(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.IsNotNull(model);
            Assert.That(model is CodeDocType);
            return (CodeDocType)model;
        }

        public virtual CodeDocMethod GetCodeDocMethod(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.IsNotNull(model);
            Assert.That(model is CodeDocMethod);
            return (CodeDocMethod)model;
        }

        public virtual CodeDocField GetCodeDocField(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.IsNotNull(model);
            Assert.That(model is CodeDocField);
            return (CodeDocField)model;
        }

        public virtual CodeDocEvent GetCodeDocEvent(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.IsNotNull(model);
            Assert.That(model is CodeDocEvent);
            return (CodeDocEvent)model;
        }

        public virtual CodeDocProperty GetCodeDocProperty(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.IsNotNull(model);
            Assert.That(model is CodeDocProperty);
            return (CodeDocProperty)model;
        }

        [Test]
        public void constructor_throws_on_invalid_requests() {
            Assert.Throws<ArgumentNullException>(() => TestLibrary1Repository.GetMemberModel((CRefIdentifier)null));
        }

        [Test]
        public void namespaces_and_assembly_counts_match_test_library() {
            Assert.AreEqual(1, TestLibrary1Repository.Assemblies.Count);
            Assert.AreEqual(5, TestLibrary1Repository.Namespaces.Count);
        }

        [Test]
        public void namespaces_and_assemblies_have_same_number_of_types() {
            Assert.AreEqual(
                TestLibrary1Repository.Assemblies.Sum(x => x.TypeCRefs.Count),
                TestLibrary1Repository.Namespaces.Sum(x => x.TypeCRefs.Count)
            );
        }

        [Test]
        public void no_empty_namespaces() {
            Assert.That(TestLibrary1Repository.Namespaces.All(x => x.TypeCRefs.Any()));
        }

        [Test]
        public void no_empty_assemblies() {
            Assert.That(TestLibrary1Repository.Assemblies.All(x => x.TypeCRefs.Any()));
        }

        [Test]
        public void verify_basic_attributes_for_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
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
        public void verify_basic_xml_doc_for_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            Assert.IsTrue(model.HasSummaryContents);
            Assert.AreEqual("This class is just for testing and has no real use outside of generating some documentation.", model.SummaryContents.First().Node.OuterXml);
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
        public void get_nested_types_from_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var expectedNestedTypeShortNames =
                new[] {"Inherits", "Inner", "NoRemarks", "NoDocs", "ProtectedStruct", "IThing"}
                .OrderBy(x => x)
                .ToArray();
            Array.Sort(expectedNestedTypeShortNames);

            Assert.IsTrue(model.HasNestedTypes);
            var nestedTypeNames =
                model.NestedTypes
                .Select(x => x.ShortName)
                .OrderBy(x => x)
                .ToArray();
            Assert.AreEqual(expectedNestedTypeShortNames, nestedTypeNames);
        }

        [Test]
        public void nested_type_member_of_type_has_summary() {
            var model = GetCodeDocType("TestLibrary1.Class1")
                .NestedTypes
                .Single(x => x.ShortName == "NoRemarks");

            Assert.IsTrue(model.HasSummaryContents);
            Assert.AreEqual("no remarks here", model.SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void get_nested_delegate_from_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var expectedNestedDelegateCRefNames =
                new[] { "T:TestLibrary1.Class1.MyFunc" }
                .OrderBy(x => x)
                .ToArray();
            Array.Sort(expectedNestedDelegateCRefNames);

            Assert.IsTrue(model.HasNestedDelegates);
            var nestedDelegateCRefs =
                model.NestedDelegates
                .Select(x => x.CRef.FullCRef)
                .OrderBy(x => x)
                .ToArray();
            Assert.AreEqual(expectedNestedDelegateCRefNames, nestedDelegateCRefs);
            Assert.That(model.NestedDelegates.All(x => x.SubTitle == "Delegate"));
        }

        [Test]
        public void nested_delegate_member_of_type_has_summary() {
            var model = GetCodeDocType("TestLibrary1.Class1")
                .NestedDelegates
                .Single(x => x.CRef.FullCRef == "T:TestLibrary1.Class1.MyFunc");

            Assert.IsTrue(model.HasSummaryContents);
            Assert.AreEqual("My delegate.", model.SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void simple_type_has_constructors() {
            var model = GetCodeDocType("TestLibrary1.Class1");

            Assert.IsTrue(model.HasConstructors);
            Assert.AreEqual(2, model.Constructors.Count);
            Assert.That(model.Constructors.All(x => x.SubTitle == "Constructor"));
        }

        [Test]
        public void simple_type_has_methods() {
            var model = GetCodeDocType("TestLibrary1.Class1");

            Assert.IsTrue(model.HasMethods);
            Assert.AreEqual(11, model.Methods.Count);
            Assert.That(model.Methods.OfType<CodeDocMethod>().Any(x => x.IsStatic.GetValueOrDefault()));
            Assert.That(model.Methods.OfType<CodeDocMethod>().Any(x => !x.IsStatic.GetValueOrDefault()));
            Assert.That(model.Methods.All(x => x.SubTitle == "Method"));
        }

        [Test]
        public void simple_type_has_operators() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            Assert.IsTrue(model.HasOperators);
            Assert.AreEqual(4, model.Operators.Count);
            Assert.That(model.Operators.OfType<CodeDocMethod>().All(x => x.IsStatic.GetValueOrDefault()));
            Assert.That(model.Operators.Any(x => x.SubTitle == "Operator"));
            Assert.That(model.Operators.Any(x => x.SubTitle == "Conversion"));
            Assert.That(model.Operators.Any(x => x.Title.Contains('+')));
        }

        [Test]
        public void simple_conversion_attribute_check(){
            var model = GetCodeDocMethod("M:TestLibrary1.Class1.op_Implicit(TestLibrary1.Class1)~System.String");
            Assert.IsNotNull(model);
            Assert.AreEqual("Implicit Class1 to String", model.Title);
            Assert.AreEqual("Conversion", model.SubTitle);
        }

        [Test]
        public void simple_type_has_events() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] {"DoStuff", "DoStuffInstance"};
            Array.Sort(names);

            Assert.IsTrue(model.HasEvents);
            Assert.AreEqual(names, model.Events.Select(x => x.ShortName).OrderBy(x => x).ToArray());
        }

        [Test]
        public void simple_type_has_fields() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] { "SomeClasses", "SomeNullableInt", "MyConst", "SomeField", "ReadonlyField" };
            Array.Sort(names);

            Assert.IsTrue(model.HasFields);
            Assert.AreEqual(names, model.Fields.Select(x => x.ShortName).OrderBy(x => x).ToArray());
            Assert.That(model.Fields.OfType<CodeDocField>().Any(x => x.IsInitOnly.GetValueOrDefault()));
            Assert.That(model.Fields.OfType<CodeDocField>().Any(x => x.IsLiteral.GetValueOrDefault()));
            Assert.That(model.Fields.OfType<CodeDocField>().All(
                x => x.SubTitle == (x.IsLiteral.GetValueOrDefault() ? "Constant" : "Field")));
        }

        [Test]
        public void simple_type_has_properties() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] {"HasTableInRemarks", "Item[Int32]", "SomeProperty"};
            Array.Sort(names);

            Assert.IsTrue(model.HasProperties);
            Assert.AreEqual(names, model.Properties.Select(x => x.ShortName).OrderBy(x => x).ToArray());
            Assert.That(model.Properties.Any(x => x.SubTitle == "Property"));
            Assert.That(model.Properties.Any(x => x.SubTitle == "Indexer"));
        }

        [Test]
        public void check_basic_enum_values() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.AreEqual("FlagsEnum", model.ShortName);
            Assert.AreEqual("TestLibrary1.FlagsEnum", model.FullName);
            Assert.AreEqual("T:TestLibrary1.FlagsEnum", model.CRef.FullCRef);
            Assert.AreEqual("FlagsEnum", model.Title);
            Assert.AreEqual("Enumeration", model.SubTitle);
            Assert.AreEqual("TestLibrary1", model.NamespaceName);
            Assert.IsTrue(model.IsEnum.GetValueOrDefault());
            Assert.IsTrue(model.IsFlagsEnum.GetValueOrDefault());
        }

        [Test]
        public void check_enum_xml_doc_details() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.IsTrue(model.HasSummaryContents);
            Assert.AreEqual("An enumeration to check detection of the flags attribute.", model.SummaryContents.First().Node.OuterXml);
            Assert.IsTrue(model.HasExamples);
            Assert.AreEqual(1, model.Examples.Count);
            Assert.AreEqual("FlagsEnum.AB == FlagsEnum.A | FlagsEnum.B;", model.Examples[0].Node.InnerText);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasRemarks);
            Assert.IsFalse(model.HasSeeAlso);
        }

        [Test]
        public void get_base_chain_for_type() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            var expected = new[] {
                new CRefIdentifier("T:System.Enum"),
                new CRefIdentifier("T:System.ValueType"),
                new CRefIdentifier("T:System.Object")
            };

            var actual = model.BaseChain.Select(x => x.CRef).ToArray();

            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(expected,actual);
        }

        [Test]
        public void get_interfaces_for_type() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.IsTrue(model.HasInterfaces);
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IComparable")));
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IConvertible")));
            Assert.That(model.Interfaces.Select(x => x.CRef), Contains.Item(new CRefIdentifier("T:System.IFormattable")));
        }

        [Test]
        public void basic_values_for_nested_type() {
            var model = GetCodeDocType("TestLibrary1.Class1.Inner");
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
            Assert.IsTrue(model.HasBaseChain);
            Assert.AreEqual(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.IsFalse(model.HasInterfaces);
        }

        [Test]
        public void check_xml_docs_for_nested_type() {
            var model = GetCodeDocType("TestLibrary1.Class1.Inner");
            Assert.IsFalse(model.HasSummaryContents);
            Assert.IsFalse(model.HasExamples);
            Assert.IsFalse(model.HasPermissions);
            Assert.IsFalse(model.HasSeeAlso);
            Assert.IsTrue(model.HasRemarks);
            Assert.AreEqual(1, model.Remarks.Count);
            Assert.AreEqual("This is just some class.", model.Remarks[0].Node.InnerText);
        }

        [Test]
        public void basic_generic_type_values() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2");
            Assert.IsNotNull(model);
            Assert.AreEqual("Generic1<TA, TB>", model.ShortName);
            Assert.IsNull(model.DeclaringType);
        }

        [Test]
        public void generic_type_type_parameters() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2");

            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(2, model.GenericParameters.Count);

            Assert.AreEqual("TA", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].HasSummaryContents);
            Assert.AreEqual(
                "<typeparam name=\"TA\">A</typeparam>",
                model.GenericParameters[0].SummaryContents.First().Node.ParentNode.OuterXml);
            Assert.IsFalse(model.GenericParameters[0].IsContravariant.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[0].IsCovariant.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[0].HasDefaultConstructorConstraint.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[0].HasReferenceTypeConstraint.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[0].HasNotNullableValueTypeConstraint.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[0].HasTypeConstraints);
            Assert.AreEqual(1, model.GenericParameters[0].TypeConstraints.Count);
            Assert.AreEqual("T:System.ValueType", model.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);

            Assert.AreEqual("TB", model.GenericParameters[1].Name);
            Assert.IsTrue(model.GenericParameters[1].HasSummaryContents);
            Assert.AreEqual(
                "<typeparam name=\"TB\">B</typeparam>",
                model.GenericParameters[1].SummaryContents.First().Node.ParentNode.OuterXml);
            Assert.IsFalse(model.GenericParameters[1].IsContravariant.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[1].IsCovariant.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[1].HasDefaultConstructorConstraint.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[1].HasReferenceTypeConstraint.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[1].HasNotNullableValueTypeConstraint.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[1].HasTypeConstraints);
            Assert.AreEqual(1, model.GenericParameters[1].TypeConstraints.Count);

            // the short name should contain the specific generic type
            Assert.That(model.GenericParameters[1].TypeConstraints[0].ShortName.Contains("TA"));
            // the generic type reference was converted to the definition
            Assert.AreEqual("T:System.Collections.Generic.IEnumerable{`0}", model.GenericParameters[1].TypeConstraints[0].CRef.FullCRef);
        }

        [Test]
        public void type_test_for_generic_variance() {
            var model = GetCodeDocType("T:TestLibrary1.Generic1`2.IVariance`2");
            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(2, model.GenericParameters.Count);
            Assert.AreEqual("TIn", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].IsContravariant.GetValueOrDefault());
            Assert.IsFalse(model.GenericParameters[0].IsCovariant.GetValueOrDefault());
            Assert.AreEqual("TOut", model.GenericParameters[1].Name);
            Assert.IsFalse(model.GenericParameters[1].IsContravariant.GetValueOrDefault());
            Assert.IsTrue(model.GenericParameters[1].IsCovariant.GetValueOrDefault());
        }

        [Test]
        public void type_generic_contraint_test() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2.Constraints`1");
            Assert.IsTrue(model.HasGenericParameters);
            Assert.AreEqual(1, model.GenericParameters.Count);
            Assert.AreEqual("TConstraints", model.GenericParameters[0].Name);
            Assert.IsTrue(model.GenericParameters[0].HasDefaultConstructorConstraint.GetValueOrDefault());
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
            var field = GetCodeDocField("TestLibrary1.Class1.SomeClasses");
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeClasses"), field.CRef);
            Assert.AreEqual("SomeClasses", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:TestLibrary1.Class1[]"), field.ValueType.CRef);
            Assert.IsFalse(field.HasValueDescriptionContents);
            Assert.IsFalse(field.IsLiteral.GetValueOrDefault());
            Assert.IsFalse(field.IsInitOnly.GetValueOrDefault());
            Assert.IsFalse(field.IsStatic.GetValueOrDefault());
            Assert.IsNotNull(field.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", field.DeclaringType.CRef.FullCRef);
        }

        [Test]
        public void field_nullable_int_test() {
            var field = GetCodeDocField("TestLibrary1.Class1.SomeNullableInt");
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeNullableInt"), field.CRef);
            Assert.AreEqual("SomeNullableInt", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Nullable{System.Int32}"), field.ValueType.CRef);
            Assert.IsNotNull(field.Namespace);
            Assert.AreEqual("N:TestLibrary1", field.Namespace.CRef.FullCRef);
            Assert.IsNotNull(field.Assembly);
            Assert.AreEqual("A:" + typeof(Class1).Assembly.FullName, field.Assembly.CRef.FullCRef);
            Assert.IsFalse(field.HasValueDescriptionContents);
            Assert.IsFalse(field.IsLiteral.GetValueOrDefault());
            Assert.IsFalse(field.IsInitOnly.GetValueOrDefault());
            Assert.IsFalse(field.IsStatic.GetValueOrDefault());
        }

        [Test]
        public void field_const_int_test() {
            var field = GetCodeDocField("TestLibrary1.Class1.MyConst");
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.MyConst"), field.CRef);
            Assert.AreEqual("MyConst", field.ShortName);
            Assert.AreEqual("Constant", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.IsTrue(field.HasValueDescriptionContents);
            Assert.AreEqual("1", field.ValueDescriptionContents.First().Node.OuterXml.Trim());
            Assert.IsTrue(field.IsLiteral.GetValueOrDefault());
            Assert.IsFalse(field.IsInitOnly.GetValueOrDefault());
            Assert.IsTrue(field.IsStatic.GetValueOrDefault());
        }

        [Test]
        public void field_static_double() {
            var field = GetCodeDocField("TestLibrary1.Class1.SomeField");
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.SomeField"), field.CRef);
            Assert.AreEqual("SomeField", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Double"), field.ValueType.CRef);
            Assert.IsTrue(field.HasValueDescriptionContents);
            Assert.AreEqual("A double value.", field.ValueDescriptionContents.First().Node.OuterXml);
            Assert.IsFalse(field.IsLiteral.GetValueOrDefault());
            Assert.IsFalse(field.IsInitOnly.GetValueOrDefault());
            Assert.IsTrue(field.IsStatic.GetValueOrDefault());
        }

        [Test]
        public void field_readonly_int() {
            var field = GetCodeDocField("TestLibrary1.Class1.ReadonlyField");
            Assert.IsNotNull(field);
            Assert.AreEqual(new CRefIdentifier("F:TestLibrary1.Class1.ReadonlyField"), field.CRef);
            Assert.AreEqual("ReadonlyField", field.ShortName);
            Assert.AreEqual("Field", field.SubTitle);
            Assert.AreEqual(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.IsFalse(field.HasValueDescriptionContents);
            Assert.IsFalse(field.IsLiteral.GetValueOrDefault());
            Assert.IsTrue(field.IsInitOnly.GetValueOrDefault());
            Assert.IsFalse(field.IsStatic.GetValueOrDefault());
        }

        [Test]
        public void method_strange_test() {
            var method = GetCodeDocMethod(
                "TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])");
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])", method.CRef.FullCRef);
            Assert.AreEqual("HasStrangeParams(Nullable<Int32>, Class1[])", method.ShortName);
            Assert.AreEqual("Method", method.SubTitle);
            Assert.IsFalse(method.IsStatic.GetValueOrDefault());
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
            var method = GetCodeDocMethod("TestLibrary1.Class1.#ctor(System.String)");
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String)", method.CRef.FullCRef);
            Assert.AreEqual("Class1(String)", method.ShortName);
            Assert.AreEqual("Constructor", method.SubTitle);
            Assert.IsFalse(method.IsStatic.GetValueOrDefault());
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
            var method = GetCodeDocMethod("TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.IsNotNull(method);
            Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Double)", method.CRef.FullCRef);
            Assert.AreEqual("DoubleStatic(Double)", method.ShortName);
            Assert.AreEqual("Method", method.SubTitle);
            Assert.IsTrue(method.IsStatic.GetValueOrDefault());
            Assert.IsTrue(method.HasReturn);
            Assert.AreEqual("T:System.Double", method.Return.ParameterType.CRef.FullCRef);
            Assert.IsTrue(method.Return.HasSummaryContents);
            Assert.AreEqual("The result of doubling the value.", method.Return.SummaryContents.First().Node.OuterXml);

            Assert.IsTrue(method.HasParameters);
            Assert.AreEqual(1, method.Parameters.Count);
            Assert.AreEqual("n", method.Parameters[0].Name);
            Assert.AreEqual("T:System.Double", method.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.IsTrue(method.Parameters[0].HasSummaryContents);
            Assert.AreEqual("The value to double.", method.Parameters[0].SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void code_contract_constructor() {
            var method = GetCodeDocMethod("TestLibrary1.ClassWithContracts.#ctor(System.String)");
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
            var method = GetCodeDocMethod("M:TestLibrary1.ClassWithContracts.SomeStuff");
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
            var method = GetCodeDocMethod("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)");
            Assert.IsNotNull(method);
            Assert.IsNotNull(method.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Generic1`2", method.DeclaringType.CRef.FullCRef);

            Assert.IsTrue(method.HasGenericParameters);
            Assert.AreEqual(1, method.GenericParameters.Count);
            Assert.AreEqual("TOther", method.GenericParameters[0].Name);
            Assert.IsFalse(method.GenericParameters[0].HasSummaryContents);
            Assert.IsFalse(method.GenericParameters[0].HasTypeConstraints);
        }

        [Test]
        public void method_generic_constraints() {
            var method = GetCodeDocMethod("M:TestLibrary1.Generic1`2.Constraints`1.GetStuff``1(`2,``0)");
            Assert.IsNotNull(method);

            Assert.IsTrue(method.HasGenericParameters);
            Assert.AreEqual(1, method.GenericParameters.Count);
            Assert.AreEqual("TStuff", method.GenericParameters[0].Name);
            Assert.AreEqual("some stuff", method.GenericParameters[0].SummaryContents.First().Node.OuterXml);
            Assert.IsTrue(method.GenericParameters[0].HasTypeConstraints);
            Assert.AreEqual(1, method.GenericParameters[0].TypeConstraints.Count);
            Assert.AreEqual("T:System.IConvertible", method.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);
        }

        [Test]
        public void delegate_with_comments() {
            var type = GetCodeDocType("T:TestLibrary1.Class1.MyFunc") as CodeDocDelegate;
            Assert.IsNotNull(type);
            Assert.IsNotNull(type.DeclaringType);
            Assert.AreEqual("T:TestLibrary1.Class1", type.DeclaringType.CRef.FullCRef);
            Assert.IsTrue(type.HasSummaryContents);
            Assert.That(type.SummaryContents.First().Node.OuterXml.Contains("My delegate."));
            Assert.IsTrue(type.HasRemarks);

            Assert.IsFalse(type.HasGenericParameters);
            Assert.IsTrue(type.HasParameters);
            Assert.AreEqual(2, type.Parameters.Count);
            Assert.AreEqual("a", type.Parameters[0].Name);
            Assert.AreEqual("param a", type.Parameters[0].SummaryContents.First().Node.OuterXml);
            Assert.AreEqual("T:System.Int32", type.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.AreEqual("b", type.Parameters[1].Name);
            Assert.AreEqual("param b", type.Parameters[1].SummaryContents.First().Node.OuterXml);
            Assert.AreEqual("T:System.Int32", type.Parameters[1].ParameterType.CRef.FullCRef);
            Assert.IsTrue(type.HasReturn);
            Assert.AreEqual("T:System.Int32", type.Return.ParameterType.CRef.FullCRef);
            Assert.AreEqual("some int", type.Return.SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void delegate_with_generic_arg() {
            var type = GetCodeDocType("TestLibrary1.Generic1`2.MyFunc`1") as CodeDocDelegate;
            Assert.IsNotNull(type);

            Assert.IsTrue(type.HasGenericParameters);
            Assert.AreEqual(1, type.GenericParameters.Count);
            Assert.AreEqual("TX", type.GenericParameters[0].Name);
        }

        [Test]
        public void event_test() {
            var evt = GetCodeDocEvent("TestLibrary1.Class1.DoStuff");
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
            Assert.IsTrue(evt.HasSummaryContents);
            Assert.That(evt.SummaryContents.First().Node.OuterXml.Contains("My event!"));
            Assert.IsTrue(evt.HasRemarks);
            Assert.AreEqual(1, evt.Remarks.Count);
            Assert.That(evt.Remarks[0].Node.InnerText.Contains("stuff"));
        }

        [Test]
        public void property_test() {
            var prop = GetCodeDocProperty("TestLibrary1.Class1.HasTableInRemarks");
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
            Assert.IsTrue(prop.HasSummaryContents);
            Assert.That(prop.SummaryContents.First().Node.OuterXml.Contains("This has a table in the remarks section."));
            Assert.IsTrue(prop.HasRemarks);
            Assert.IsFalse(prop.HasGetter);
            Assert.IsTrue(prop.HasSetter);
            Assert.IsNotNull(prop.Setter);
        }

        [Test]
        public void property_indexer_test() {
            var prop = GetCodeDocProperty("TestLibrary1.Class1.Item(System.Int32)");
            Assert.IsNotNull(prop);

            Assert.AreEqual("T:System.Int32", prop.ValueType.CRef.FullCRef);
            Assert.IsTrue(prop.HasSummaryContents);
            Assert.IsTrue(prop.HasValueDescriptionContents);
            Assert.AreEqual("Some number.", prop.ValueDescriptionContents.First().Node.OuterXml);

            Assert.IsTrue(prop.HasParameters);
            Assert.AreEqual(1, prop.Parameters.Count);
            Assert.AreEqual("n", prop.Parameters[0].Name);
            Assert.AreEqual("T:System.Int32", prop.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.IsTrue(prop.Parameters[0].HasSummaryContents);
            Assert.AreEqual("an index", prop.Parameters[0].SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void out_ref_method_test() {
            var method = GetCodeDocMethod("TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32@,System.Int32@)");
            Assert.IsNotNull(method);
            Assert.IsTrue(method.Parameters[0].IsOut.GetValueOrDefault());
            Assert.IsTrue(method.Parameters[0].IsByRef.GetValueOrDefault());
            Assert.IsFalse(method.Parameters[1].IsOut.GetValueOrDefault());
            Assert.IsTrue(method.Parameters[1].IsByRef.GetValueOrDefault());
        }

        [Test]
        public void simple_inheritance_check() {
            var a = GetCodeDocMethod("TestLibrary1.Seal.BaseClassToSeal.SealMe(System.Int32)");
            var b = GetCodeDocMethod("TestLibrary1.Seal.KickSealingCan.SealMe(System.Int32)");
            var c = GetCodeDocMethod("TestLibrary1.Seal.SealIt.SealMe(System.Int32)");
            var d = GetCodeDocMethod("TestLibrary1.Seal.SealSomeOfIt.SealMe(System.Int32)");
            Assert.AreEqual("From BaseClassToSeal.", a.SummaryContents.First().Node.OuterXml);
            Assert.AreEqual("From BaseClassToSeal.", b.SummaryContents.First().Node.OuterXml);
            Assert.AreEqual("From SealIt.", c.SummaryContents.First().Node.OuterXml);
            Assert.AreEqual("From BaseClassToSeal.", d.SummaryContents.First().Node.OuterXml);
        }

        [Test]
        public void explicit_interface_implementation_is_hidden(){
            var type = GetCodeDocType("T:TestLibrary1.Class1.ProtectedStruct");
            Assert.That(type.Methods.All(m => m.NamespaceName.StartsWith("System")));
        }

    }
}
