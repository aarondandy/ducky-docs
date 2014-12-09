using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DuckyDocs.CRef;
using DuckyDocs.Reflection;
using DuckyDocs.XmlDoc;
using TestLibrary1;
using Xunit;

namespace DuckyDocs.CodeDoc.Tests
{
    public class CodeDocEntityRepositoryFacts
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
            Assert.NotNull(model);
            Assert.True(model is CodeDocType);
            return (CodeDocType)model;
        }

        public virtual CodeDocMethod GetCodeDocMethod(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.NotNull(model);
            Assert.True(model is CodeDocMethod);
            return (CodeDocMethod)model;
        }

        public virtual CodeDocField GetCodeDocField(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.NotNull(model);
            Assert.True(model is CodeDocField);
            return (CodeDocField)model;
        }

        public virtual CodeDocEvent GetCodeDocEvent(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.NotNull(model);
            Assert.True(model is CodeDocEvent);
            return (CodeDocEvent)model;
        }

        public virtual CodeDocProperty GetCodeDocProperty(string cRef) {
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var model = TestLibrary1Repository.GetMemberModel(new CRefIdentifier(cRef));
            Assert.NotNull(model);
            Assert.True(model is CodeDocProperty);
            return (CodeDocProperty)model;
        }

        [Fact]
        public void constructor_throws_on_invalid_requests() {
            Assert.Throws<ArgumentNullException>(() => TestLibrary1Repository.GetMemberModel((CRefIdentifier)null));
        }

        [Fact]
        public void namespaces_and_assembly_counts_match_test_library() {
            Assert.Equal(1, TestLibrary1Repository.Assemblies.Count);
            Assert.Equal(5, TestLibrary1Repository.Namespaces.Count);
        }

        [Fact]
        public void namespaces_and_assemblies_have_same_number_of_types() {
            Assert.Equal(
                TestLibrary1Repository.Assemblies.Sum(x => x.TypeCRefs.Count),
                TestLibrary1Repository.Namespaces.Sum(x => x.TypeCRefs.Count)
            );
        }

        [Fact]
        public void no_empty_namespaces() {
            Assert.True(TestLibrary1Repository.Namespaces.All(x => x.TypeCRefs.Any()));
        }

        [Fact]
        public void no_empty_assemblies() {
            Assert.True(TestLibrary1Repository.Assemblies.All(x => x.TypeCRefs.Any()));
        }

        [Fact]
        public void verify_basic_attributes_for_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            Assert.Equal("Class1", model.ShortName);
            Assert.Equal("TestLibrary1.Class1", model.FullName);
            Assert.Equal("T:TestLibrary1.Class1", model.CRef.FullCRef);
            Assert.Equal("Class1", model.Title);
            Assert.Equal("Class", model.SubTitle);
            Assert.Equal("TestLibrary1", model.NamespaceName);
            Assert.NotNull(model.Namespace);
            Assert.Equal("N:TestLibrary1", model.Namespace.CRef.FullCRef);
            Assert.NotNull(model.Assembly);
            Assert.Equal("A:" + typeof(Class1).Assembly.FullName, model.Assembly.CRef.FullCRef);
            Assert.True(model.HasBaseChain);
            Assert.Equal(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.False(model.HasInterfaces);
            Assert.Null(model.DeclaringType);
        }

        [Fact]
        public void verify_basic_xml_doc_for_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            Assert.True(model.HasSummaryContents);
            Assert.Equal("This class is just for testing and has no real use outside of generating some documentation.", model.SummaryContents.First().Node.OuterXml);
            Assert.True(model.HasExamples);
            Assert.Equal(2, model.Examples.Count);
            Assert.Equal("Example 1", model.Examples[0].Node.InnerText);
            Assert.Equal("Example 2", model.Examples[1].Node.InnerText);
            Assert.False(model.HasPermissions);
            Assert.True(model.HasRemarks);
            Assert.Equal(1, model.Remarks.Count);
            Assert.Equal("These are some remarks.", model.Remarks[0].Node.InnerText);
            Assert.False(model.HasSeeAlso);
        }

        [Fact]
        public void get_nested_types_from_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var expectedNestedTypeShortNames =
                new[] {"Inherits", "Inner", "NoRemarks", "NoDocs", "ProtectedStruct", "IThing"}
                .OrderBy(x => x)
                .ToArray();
            Array.Sort(expectedNestedTypeShortNames);

            Assert.True(model.HasNestedTypes);
            var nestedTypeNames =
                model.NestedTypes
                .Select(x => x.ShortName)
                .OrderBy(x => x)
                .ToArray();
            Assert.Equal(expectedNestedTypeShortNames, nestedTypeNames);
        }

        [Fact]
        public void nested_type_member_of_type_has_summary() {
            var model = GetCodeDocType("TestLibrary1.Class1")
                .NestedTypes
                .Single(x => x.ShortName == "NoRemarks");

            Assert.True(model.HasSummaryContents);
            Assert.Equal("no remarks here", model.SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void get_nested_delegate_from_simple_type() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var expectedNestedDelegateCRefNames =
                new[] { "T:TestLibrary1.Class1.MyFunc" }
                .OrderBy(x => x)
                .ToArray();
            Array.Sort(expectedNestedDelegateCRefNames);

            Assert.True(model.HasNestedDelegates);
            var nestedDelegateCRefs =
                model.NestedDelegates
                .Select(x => x.CRef.FullCRef)
                .OrderBy(x => x)
                .ToArray();
            Assert.Equal(expectedNestedDelegateCRefNames, nestedDelegateCRefs);
            Assert.True(model.NestedDelegates.All(x => x.SubTitle == "Delegate"));
        }

        [Fact]
        public void nested_delegate_member_of_type_has_summary() {
            var model = GetCodeDocType("TestLibrary1.Class1")
                .NestedDelegates
                .Single(x => x.CRef.FullCRef == "T:TestLibrary1.Class1.MyFunc");

            Assert.True(model.HasSummaryContents);
            Assert.Equal("My delegate.", model.SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void simple_type_has_constructors() {
            var model = GetCodeDocType("TestLibrary1.Class1");

            Assert.True(model.HasConstructors);
            Assert.Equal(2, model.Constructors.Count);
            Assert.True(model.Constructors.All(x => x.SubTitle == "Constructor"));
        }

        [Fact]
        public void simple_type_has_methods() {
            var model = GetCodeDocType("TestLibrary1.Class1");

            Assert.True(model.HasMethods);
            Assert.Equal(11, model.Methods.Count);
            Assert.True(model.Methods.OfType<CodeDocMethod>().Any(x => x.IsStatic.GetValueOrDefault()));
            Assert.True(model.Methods.OfType<CodeDocMethod>().Any(x => !x.IsStatic.GetValueOrDefault()));
            Assert.True(model.Methods.All(x => x.SubTitle == "Method"));
        }

        [Fact]
        public void simple_type_has_operators() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            Assert.True(model.HasOperators);
            Assert.Equal(4, model.Operators.Count);
            Assert.True(model.Operators.OfType<CodeDocMethod>().All(x => x.IsStatic.GetValueOrDefault()));
            Assert.True(model.Operators.Any(x => x.SubTitle == "Operator"));
            Assert.True(model.Operators.Any(x => x.SubTitle == "Conversion"));
            Assert.True(model.Operators.Any(x => x.Title.Contains('+')));
        }

        [Fact]
        public void simple_conversion_attribute_check(){
            var model = GetCodeDocMethod("M:TestLibrary1.Class1.op_Implicit(TestLibrary1.Class1)~System.String");
            Assert.NotNull(model);
            Assert.Equal("Implicit Class1 to String", model.Title);
            Assert.Equal("Conversion", model.SubTitle);
        }

        [Fact]
        public void simple_type_has_events() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] {"DoStuff", "DoStuffInstance"};
            Array.Sort(names);

            Assert.True(model.HasEvents);
            Assert.Equal(names, model.Events.Select(x => x.ShortName).OrderBy(x => x).ToArray());
        }

        [Fact]
        public void simple_type_has_fields() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] { "SomeClasses", "SomeNullableInt", "MyConst", "SomeField", "ReadonlyField" };
            Array.Sort(names);

            Assert.True(model.HasFields);
            Assert.Equal(names, model.Fields.Select(x => x.ShortName).OrderBy(x => x).ToArray());
            Assert.True(model.Fields.OfType<CodeDocField>().Any(x => x.IsInitOnly.GetValueOrDefault()));
            Assert.True(model.Fields.OfType<CodeDocField>().Any(x => x.IsLiteral.GetValueOrDefault()));
            Assert.True(model.Fields.OfType<CodeDocField>().All(
                x => x.SubTitle == (x.IsLiteral.GetValueOrDefault() ? "Constant" : "Field")));
        }

        [Fact]
        public void simple_type_has_properties() {
            var model = GetCodeDocType("TestLibrary1.Class1");
            var names = new[] {"HasTableInRemarks", "Item[Int32]", "SomeProperty"};
            Array.Sort(names);

            Assert.True(model.HasProperties);
            Assert.Equal(names, model.Properties.Select(x => x.ShortName).OrderBy(x => x).ToArray());
            Assert.True(model.Properties.Any(x => x.SubTitle == "Property"));
            Assert.True(model.Properties.Any(x => x.SubTitle == "Indexer"));
        }

        [Fact]
        public void check_basic_enum_values() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.Equal("FlagsEnum", model.ShortName);
            Assert.Equal("TestLibrary1.FlagsEnum", model.FullName);
            Assert.Equal("T:TestLibrary1.FlagsEnum", model.CRef.FullCRef);
            Assert.Equal("FlagsEnum", model.Title);
            Assert.Equal("Enumeration", model.SubTitle);
            Assert.Equal("TestLibrary1", model.NamespaceName);
            Assert.True(model.IsEnum.GetValueOrDefault());
            Assert.True(model.IsFlagsEnum.GetValueOrDefault());
        }

        [Fact]
        public void check_enum_xml_doc_details() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.True(model.HasSummaryContents);
            Assert.Equal("An enumeration to check detection of the flags attribute.", model.SummaryContents.First().Node.OuterXml);
            Assert.True(model.HasExamples);
            Assert.Equal(1, model.Examples.Count);
            Assert.Equal("FlagsEnum.AB == FlagsEnum.A | FlagsEnum.B;", model.Examples[0].Node.InnerText);
            Assert.False(model.HasPermissions);
            Assert.False(model.HasRemarks);
            Assert.False(model.HasSeeAlso);
        }

        [Fact]
        public void get_base_chain_for_type() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            var expected = new[] {
                new CRefIdentifier("T:System.Enum"),
                new CRefIdentifier("T:System.ValueType"),
                new CRefIdentifier("T:System.Object")
            };

            var actual = model.BaseChain.Select(x => x.CRef).ToArray();

            Assert.True(model.HasBaseChain);
            Assert.Equal(expected,actual);
        }

        [Fact]
        public void get_interfaces_for_type() {
            var model = GetCodeDocType("TestLibrary1.FlagsEnum");
            Assert.True(model.HasInterfaces);
            Assert.True(model.Interfaces.Select(x => x.CRef.FullCRef).Contains("T:System.IComparable"));
            Assert.True(model.Interfaces.Select(x => x.CRef.FullCRef).Contains("T:System.IConvertible"));
            Assert.True(model.Interfaces.Select(x => x.CRef.FullCRef).Contains("T:System.IFormattable"));
        }

        [Fact]
        public void basic_values_for_nested_type() {
            var model = GetCodeDocType("TestLibrary1.Class1.Inner");
            Assert.Equal("Inner", model.ShortName);
            Assert.Equal("TestLibrary1.Class1.Inner", model.FullName);
            Assert.Equal("T:TestLibrary1.Class1.Inner", model.CRef.FullCRef);
            Assert.Equal("Inner", model.Title);
            Assert.Equal("Class", model.SubTitle);
            Assert.Equal("TestLibrary1", model.NamespaceName);
            Assert.NotNull(model.Namespace);
            Assert.Equal("N:TestLibrary1", model.Namespace.CRef.FullCRef);
            Assert.NotNull(model.DeclaringType);
            Assert.Equal("T:TestLibrary1.Class1", model.DeclaringType.CRef.FullCRef);
            Assert.True(model.HasBaseChain);
            Assert.Equal(
                new[] { new CRefIdentifier("T:System.Object") },
                model.BaseChain.Select(x => x.CRef).ToArray());
            Assert.False(model.HasInterfaces);
        }

        [Fact]
        public void check_xml_docs_for_nested_type() {
            var model = GetCodeDocType("TestLibrary1.Class1.Inner");
            Assert.False(model.HasSummaryContents);
            Assert.False(model.HasExamples);
            Assert.False(model.HasPermissions);
            Assert.False(model.HasSeeAlso);
            Assert.True(model.HasRemarks);
            Assert.Equal(1, model.Remarks.Count);
            Assert.Equal("This is just some class.", model.Remarks[0].Node.InnerText);
        }

        [Fact]
        public void basic_generic_type_values() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2");
            Assert.NotNull(model);
            Assert.Equal("Generic1<TA, TB>", model.ShortName);
            Assert.Null(model.DeclaringType);
        }

        [Fact]
        public void generic_type_type_parameters() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2");

            Assert.True(model.HasGenericParameters);
            Assert.Equal(2, model.GenericParameters.Count);

            Assert.Equal("TA", model.GenericParameters[0].Name);
            Assert.True(model.GenericParameters[0].HasSummaryContents);
            Assert.Equal(
                "<typeparam name=\"TA\">A</typeparam>",
                model.GenericParameters[0].SummaryContents.First().Node.ParentNode.OuterXml);
            Assert.False(model.GenericParameters[0].IsContravariant.GetValueOrDefault());
            Assert.False(model.GenericParameters[0].IsCovariant.GetValueOrDefault());
            Assert.True(model.GenericParameters[0].HasDefaultConstructorConstraint.GetValueOrDefault());
            Assert.False(model.GenericParameters[0].HasReferenceTypeConstraint.GetValueOrDefault());
            Assert.True(model.GenericParameters[0].HasNotNullableValueTypeConstraint.GetValueOrDefault());
            Assert.True(model.GenericParameters[0].HasTypeConstraints);
            Assert.Equal(1, model.GenericParameters[0].TypeConstraints.Count);
            Assert.Equal("T:System.ValueType", model.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);

            Assert.Equal("TB", model.GenericParameters[1].Name);
            Assert.True(model.GenericParameters[1].HasSummaryContents);
            Assert.Equal(
                "<typeparam name=\"TB\">B</typeparam>",
                model.GenericParameters[1].SummaryContents.First().Node.ParentNode.OuterXml);
            Assert.False(model.GenericParameters[1].IsContravariant.GetValueOrDefault());
            Assert.False(model.GenericParameters[1].IsCovariant.GetValueOrDefault());
            Assert.False(model.GenericParameters[1].HasDefaultConstructorConstraint.GetValueOrDefault());
            Assert.True(model.GenericParameters[1].HasReferenceTypeConstraint.GetValueOrDefault());
            Assert.False(model.GenericParameters[1].HasNotNullableValueTypeConstraint.GetValueOrDefault());
            Assert.True(model.GenericParameters[1].HasTypeConstraints);
            Assert.Equal(1, model.GenericParameters[1].TypeConstraints.Count);

            // the short name should contain the specific generic type
            Assert.True(model.GenericParameters[1].TypeConstraints[0].ShortName.Contains("TA"));
            // the generic type reference was converted to the definition
            Assert.Equal("T:System.Collections.Generic.IEnumerable{`0}", model.GenericParameters[1].TypeConstraints[0].CRef.FullCRef);
        }

        [Fact]
        public void type_test_for_generic_variance() {
            var model = GetCodeDocType("T:TestLibrary1.Generic1`2.IVariance`2");
            Assert.True(model.HasGenericParameters);
            Assert.Equal(2, model.GenericParameters.Count);
            Assert.Equal("TIn", model.GenericParameters[0].Name);
            Assert.True(model.GenericParameters[0].IsContravariant.GetValueOrDefault());
            Assert.False(model.GenericParameters[0].IsCovariant.GetValueOrDefault());
            Assert.Equal("TOut", model.GenericParameters[1].Name);
            Assert.False(model.GenericParameters[1].IsContravariant.GetValueOrDefault());
            Assert.True(model.GenericParameters[1].IsCovariant.GetValueOrDefault());
        }

        [Fact]
        public void type_generic_contraint_test() {
            var model = GetCodeDocType("TestLibrary1.Generic1`2.Constraints`1");
            Assert.True(model.HasGenericParameters);
            Assert.Equal(1, model.GenericParameters.Count);
            Assert.Equal("TConstraints", model.GenericParameters[0].Name);
            Assert.True(model.GenericParameters[0].HasDefaultConstructorConstraint.GetValueOrDefault());
            Assert.True(model.GenericParameters[0].HasTypeConstraints);
            Assert.Equal(2, model.GenericParameters[0].TypeConstraints.Count);
            Assert.Equal(
                new[] {
                    new CRefIdentifier("T:System.Collections.Generic.IEnumerable{System.Int32}"),
                    new CRefIdentifier("T:System.IDisposable")
                },
                model.GenericParameters[0].TypeConstraints.Select(x => x.CRef).ToArray());
        }

        [Fact]
        public void field_array_of_ref_type_tests() {
            var field = GetCodeDocField("TestLibrary1.Class1.SomeClasses");
            Assert.NotNull(field);
            Assert.Equal(new CRefIdentifier("F:TestLibrary1.Class1.SomeClasses"), field.CRef);
            Assert.Equal("SomeClasses", field.ShortName);
            Assert.Equal("Field", field.SubTitle);
            Assert.Equal(new CRefIdentifier("T:TestLibrary1.Class1[]"), field.ValueType.CRef);
            Assert.False(field.HasValueDescriptionContents);
            Assert.False(field.IsLiteral.GetValueOrDefault());
            Assert.False(field.IsInitOnly.GetValueOrDefault());
            Assert.False(field.IsStatic.GetValueOrDefault());
            Assert.NotNull(field.DeclaringType);
            Assert.Equal("T:TestLibrary1.Class1", field.DeclaringType.CRef.FullCRef);
        }

        [Fact]
        public void field_nullable_int_test() {
            var field = GetCodeDocField("TestLibrary1.Class1.SomeNullableInt");
            Assert.NotNull(field);
            Assert.Equal(new CRefIdentifier("F:TestLibrary1.Class1.SomeNullableInt"), field.CRef);
            Assert.Equal("SomeNullableInt", field.ShortName);
            Assert.Equal("Field", field.SubTitle);
            Assert.Equal(new CRefIdentifier("T:System.Nullable{System.Int32}"), field.ValueType.CRef);
            Assert.NotNull(field.Namespace);
            Assert.Equal("N:TestLibrary1", field.Namespace.CRef.FullCRef);
            Assert.NotNull(field.Assembly);
            Assert.Equal("A:" + typeof(Class1).Assembly.FullName, field.Assembly.CRef.FullCRef);
            Assert.False(field.HasValueDescriptionContents);
            Assert.False(field.IsLiteral.GetValueOrDefault());
            Assert.False(field.IsInitOnly.GetValueOrDefault());
            Assert.False(field.IsStatic.GetValueOrDefault());
        }

        [Fact]
        public void field_const_int_test() {
            var field = GetCodeDocField("TestLibrary1.Class1.MyConst");
            Assert.NotNull(field);
            Assert.Equal(new CRefIdentifier("F:TestLibrary1.Class1.MyConst"), field.CRef);
            Assert.Equal("MyConst", field.ShortName);
            Assert.Equal("Constant", field.SubTitle);
            Assert.Equal(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.True(field.HasValueDescriptionContents);
            Assert.Equal("1", field.ValueDescriptionContents.First().Node.OuterXml.Trim());
            Assert.True(field.IsLiteral.GetValueOrDefault());
            Assert.False(field.IsInitOnly.GetValueOrDefault());
            Assert.True(field.IsStatic.GetValueOrDefault());
        }

        [Fact]
        public void field_static_double() {
            var field = GetCodeDocField("TestLibrary1.Class1.SomeField");
            Assert.NotNull(field);
            Assert.Equal(new CRefIdentifier("F:TestLibrary1.Class1.SomeField"), field.CRef);
            Assert.Equal("SomeField", field.ShortName);
            Assert.Equal("Field", field.SubTitle);
            Assert.Equal(new CRefIdentifier("T:System.Double"), field.ValueType.CRef);
            Assert.True(field.HasValueDescriptionContents);
            Assert.Equal("A double value.", field.ValueDescriptionContents.First().Node.OuterXml);
            Assert.False(field.IsLiteral.GetValueOrDefault());
            Assert.False(field.IsInitOnly.GetValueOrDefault());
            Assert.True(field.IsStatic.GetValueOrDefault());
        }

        [Fact]
        public void field_readonly_int() {
            var field = GetCodeDocField("TestLibrary1.Class1.ReadonlyField");
            Assert.NotNull(field);
            Assert.Equal(new CRefIdentifier("F:TestLibrary1.Class1.ReadonlyField"), field.CRef);
            Assert.Equal("ReadonlyField", field.ShortName);
            Assert.Equal("Field", field.SubTitle);
            Assert.Equal(new CRefIdentifier("T:System.Int32"), field.ValueType.CRef);
            Assert.False(field.HasValueDescriptionContents);
            Assert.False(field.IsLiteral.GetValueOrDefault());
            Assert.True(field.IsInitOnly.GetValueOrDefault());
            Assert.False(field.IsStatic.GetValueOrDefault());
        }

        [Fact]
        public void method_strange_test() {
            var method = GetCodeDocMethod(
                "TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])");
            Assert.NotNull(method);
            Assert.Equal("M:TestLibrary1.Class1.HasStrangeParams(System.Nullable{System.Int32},TestLibrary1.Class1[])", method.CRef.FullCRef);
            Assert.Equal("HasStrangeParams(Nullable<Int32>, Class1[])", method.ShortName);
            Assert.Equal("Method", method.SubTitle);
            Assert.False(method.IsStatic.GetValueOrDefault());
            Assert.False(method.HasReturn);
            Assert.NotNull(method.Namespace);
            Assert.Equal("N:TestLibrary1", method.Namespace.CRef.FullCRef);
            Assert.NotNull(method.Assembly);
            Assert.Equal("A:" + typeof(Class1).Assembly.FullName, method.Assembly.CRef.FullCRef);

            Assert.True(method.HasParameters);
            Assert.Equal(2, method.Parameters.Count);
            Assert.Equal("a", method.Parameters[0].Name);
            Assert.Equal("T:System.Nullable{System.Int32}", method.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.Equal("someClass", method.Parameters[1].Name);
            Assert.Equal("T:TestLibrary1.Class1[]", method.Parameters[1].ParameterType.CRef.FullCRef);

            Assert.True(method.HasExceptions);
            Assert.Equal(1, method.Exceptions.Count);
            Assert.Equal("T:System.NotImplementedException", method.Exceptions[0].ExceptionType.CRef.FullCRef);
            Assert.True(method.Exceptions[0].HasConditions);
            Assert.Equal(1, method.Exceptions[0].Conditions.Count);
            Assert.Equal("Too lazy to implement.", method.Exceptions[0].Conditions[0].Node.InnerText);
        }

        [Fact]
        public void method_one_param_ctor() {
            var method = GetCodeDocMethod("TestLibrary1.Class1.#ctor(System.String)");
            Assert.NotNull(method);
            Assert.Equal("M:TestLibrary1.Class1.#ctor(System.String)", method.CRef.FullCRef);
            Assert.Equal("Class1(String)", method.ShortName);
            Assert.Equal("Constructor", method.SubTitle);
            Assert.False(method.IsStatic.GetValueOrDefault());
            Assert.False(method.HasReturn);

            Assert.NotNull(method.DeclaringType);
            Assert.Equal("T:TestLibrary1.Class1", method.DeclaringType.CRef.FullCRef);

            Assert.True(method.HasParameters);
            Assert.Equal(1, method.Parameters.Count);
            Assert.Equal("crap", method.Parameters[0].Name);
            Assert.Equal("T:System.String", method.Parameters[0].ParameterType.CRef.FullCRef);
        }

        [Fact]
        public void method_DoubleStatic() {
            var method = GetCodeDocMethod("TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.NotNull(method);
            Assert.Equal("M:TestLibrary1.Class1.DoubleStatic(System.Double)", method.CRef.FullCRef);
            Assert.Equal("DoubleStatic(Double)", method.ShortName);
            Assert.Equal("Method", method.SubTitle);
            Assert.True(method.IsStatic.GetValueOrDefault());
            Assert.True(method.HasReturn);
            Assert.Equal("T:System.Double", method.Return.ParameterType.CRef.FullCRef);
            Assert.True(method.Return.HasSummaryContents);
            Assert.Equal("The result of doubling the value.", method.Return.SummaryContents.First().Node.OuterXml);

            Assert.True(method.HasParameters);
            Assert.Equal(1, method.Parameters.Count);
            Assert.Equal("n", method.Parameters[0].Name);
            Assert.Equal("T:System.Double", method.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.True(method.Parameters[0].HasSummaryContents);
            Assert.Equal("The value to double.", method.Parameters[0].SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void code_contract_constructor() {
            var method = GetCodeDocMethod("TestLibrary1.ClassWithContracts.#ctor(System.String)");
            Assert.NotNull(method);
            Assert.True(method.HasExceptions);
            Assert.Equal(1, method.Exceptions.Count);
            Assert.Equal("T:System.ArgumentException", method.Exceptions[0].ExceptionType.CRef.FullCRef);
            Assert.Equal(2, method.Exceptions[0].Conditions.Count);
            Assert.True(method.Exceptions[0].Conditions[0].Node.InnerText.Contains("IsNullOrEmpty(text)"));
            Assert.True(method.Exceptions[0].Conditions[1].Node.InnerText.Contains("text.Equals(\"nope\")"));
            Assert.Equal(2, method.Exceptions[0].Ensures.Count);
            Assert.True(method.Exceptions[0].Ensures[0].Node.InnerText.Contains("Text == null"));
            Assert.True(method.Exceptions[0].Ensures[1].Node.InnerText.Contains("Text != \"nope!\""));

            Assert.True(method.HasEnsures);
            Assert.Equal(2, method.Ensures.Count);
            Assert.False(method.HasNormalTerminationEnsures);
            Assert.True(method.HasRequires);
            Assert.Equal(2, method.Requires.Count);
        }

        [Fact]
        public void code_contract_simple_ensures_method() {
            var method = GetCodeDocMethod("M:TestLibrary1.ClassWithContracts.SomeStuff");
            Assert.NotNull(method);

            Assert.False(method.HasRequires);
            Assert.True(method.HasEnsures);
            Assert.Equal(1, method.Ensures.Count);
            Assert.True(method.HasNormalTerminationEnsures);
            Assert.Equal(1, method.NormalTerminationEnsures.Count());
            Assert.True(method.NormalTerminationEnsures.First().Node.InnerText.Contains("IsNullOrEmpty"));
        }

        [Fact]
        public void method_generic() {
            var method = GetCodeDocMethod("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)");
            Assert.NotNull(method);
            Assert.NotNull(method.DeclaringType);
            Assert.Equal("T:TestLibrary1.Generic1`2", method.DeclaringType.CRef.FullCRef);

            Assert.True(method.HasGenericParameters);
            Assert.Equal(1, method.GenericParameters.Count);
            Assert.Equal("TOther", method.GenericParameters[0].Name);
            Assert.False(method.GenericParameters[0].HasSummaryContents);
            Assert.False(method.GenericParameters[0].HasTypeConstraints);
        }

        [Fact]
        public void method_generic_constraints() {
            var method = GetCodeDocMethod("M:TestLibrary1.Generic1`2.Constraints`1.GetStuff``1(`2,``0)");
            Assert.NotNull(method);

            Assert.True(method.HasGenericParameters);
            Assert.Equal(1, method.GenericParameters.Count);
            Assert.Equal("TStuff", method.GenericParameters[0].Name);
            Assert.Equal("some stuff", method.GenericParameters[0].SummaryContents.First().Node.OuterXml);
            Assert.True(method.GenericParameters[0].HasTypeConstraints);
            Assert.Equal(1, method.GenericParameters[0].TypeConstraints.Count);
            Assert.Equal("T:System.IConvertible", method.GenericParameters[0].TypeConstraints[0].CRef.FullCRef);
        }

        [Fact]
        public void delegate_with_comments() {
            var type = GetCodeDocType("T:TestLibrary1.Class1.MyFunc") as CodeDocDelegate;
            Assert.NotNull(type);
            Assert.NotNull(type.DeclaringType);
            Assert.Equal("T:TestLibrary1.Class1", type.DeclaringType.CRef.FullCRef);
            Assert.True(type.HasSummaryContents);
            Assert.True(type.SummaryContents.First().Node.OuterXml.Contains("My delegate."));
            Assert.True(type.HasRemarks);

            Assert.False(type.HasGenericParameters);
            Assert.True(type.HasParameters);
            Assert.Equal(2, type.Parameters.Count);
            Assert.Equal("a", type.Parameters[0].Name);
            Assert.Equal("param a", type.Parameters[0].SummaryContents.First().Node.OuterXml);
            Assert.Equal("T:System.Int32", type.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.Equal("b", type.Parameters[1].Name);
            Assert.Equal("param b", type.Parameters[1].SummaryContents.First().Node.OuterXml);
            Assert.Equal("T:System.Int32", type.Parameters[1].ParameterType.CRef.FullCRef);
            Assert.True(type.HasReturn);
            Assert.Equal("T:System.Int32", type.Return.ParameterType.CRef.FullCRef);
            Assert.Equal("some int", type.Return.SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void delegate_with_generic_arg() {
            var type = GetCodeDocType("TestLibrary1.Generic1`2.MyFunc`1") as CodeDocDelegate;
            Assert.NotNull(type);

            Assert.True(type.HasGenericParameters);
            Assert.Equal(1, type.GenericParameters.Count);
            Assert.Equal("TX", type.GenericParameters[0].Name);
        }

        [Fact]
        public void event_test() {
            var evt = GetCodeDocEvent("TestLibrary1.Class1.DoStuff");
            Assert.NotNull(evt);
            Assert.NotNull(evt.DeclaringType);
            Assert.NotNull(evt.Namespace);
            Assert.Equal("N:TestLibrary1", evt.Namespace.CRef.FullCRef);
            Assert.NotNull(evt.Assembly);
            Assert.Equal("A:" + typeof(Class1).Assembly.FullName, evt.Assembly.CRef.FullCRef);
            Assert.Equal("T:TestLibrary1.Class1", evt.DeclaringType.CRef.FullCRef);
            Assert.Equal("DoStuff", evt.ShortName);
            Assert.Equal("DoStuff", evt.Title);
            Assert.Equal("Event", evt.SubTitle);
            Assert.Equal("E:TestLibrary1.Class1.DoStuff", evt.CRef.FullCRef);
            Assert.Equal("TestLibrary1.Class1.DoStuff", evt.FullName);
            Assert.Equal("T:TestLibrary1.Class1.MyFunc", evt.DelegateType.CRef.FullCRef);
            Assert.True(evt.HasSummaryContents);
            Assert.True(evt.SummaryContents.First().Node.OuterXml.Contains("My event!"));
            Assert.True(evt.HasRemarks);
            Assert.Equal(1, evt.Remarks.Count);
            Assert.True(evt.Remarks[0].Node.InnerText.Contains("stuff"));
        }

        [Fact]
        public void property_test() {
            var prop = GetCodeDocProperty("TestLibrary1.Class1.HasTableInRemarks");
            Assert.NotNull(prop);
            Assert.NotNull(prop.DeclaringType);
            Assert.NotNull(prop.Namespace);
            Assert.Equal("N:TestLibrary1", prop.Namespace.CRef.FullCRef);
            Assert.NotNull(prop.Assembly);
            Assert.Equal("A:" + typeof(Class1).Assembly.FullName, prop.Assembly.CRef.FullCRef);
            Assert.Equal("T:TestLibrary1.Class1", prop.DeclaringType.CRef.FullCRef);
            Assert.Equal("HasTableInRemarks", prop.Title);
            Assert.Equal("HasTableInRemarks", prop.ShortName);
            Assert.Equal("Property", prop.SubTitle);
            Assert.Equal("TestLibrary1.Class1.HasTableInRemarks", prop.FullName);
            Assert.Equal("P:TestLibrary1.Class1.HasTableInRemarks", prop.CRef.FullCRef);
            Assert.True(prop.HasSummaryContents);
            Assert.True(prop.SummaryContents.First().Node.OuterXml.Contains("This has a table in the remarks section."));
            Assert.True(prop.HasRemarks);
            Assert.False(prop.HasGetter);
            Assert.True(prop.HasSetter);
            Assert.NotNull(prop.Setter);
        }

        [Fact]
        public void property_indexer_test() {
            var prop = GetCodeDocProperty("TestLibrary1.Class1.Item(System.Int32)");
            Assert.NotNull(prop);

            Assert.Equal("T:System.Int32", prop.ValueType.CRef.FullCRef);
            Assert.True(prop.HasSummaryContents);
            Assert.True(prop.HasValueDescriptionContents);
            Assert.Equal("Some number.", prop.ValueDescriptionContents.First().Node.OuterXml);

            Assert.True(prop.HasParameters);
            Assert.Equal(1, prop.Parameters.Count);
            Assert.Equal("n", prop.Parameters[0].Name);
            Assert.Equal("T:System.Int32", prop.Parameters[0].ParameterType.CRef.FullCRef);
            Assert.True(prop.Parameters[0].HasSummaryContents);
            Assert.Equal("an index", prop.Parameters[0].SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void out_ref_method_test() {
            var method = GetCodeDocMethod("TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32@,System.Int32@)");
            Assert.NotNull(method);
            Assert.True(method.Parameters[0].IsOut.GetValueOrDefault());
            Assert.True(method.Parameters[0].IsByRef.GetValueOrDefault());
            Assert.False(method.Parameters[1].IsOut.GetValueOrDefault());
            Assert.True(method.Parameters[1].IsByRef.GetValueOrDefault());
        }

        [Fact]
        public void simple_inheritance_check() {
            var a = GetCodeDocMethod("TestLibrary1.Seal.BaseClassToSeal.SealMe(System.Int32)");
            var b = GetCodeDocMethod("TestLibrary1.Seal.KickSealingCan.SealMe(System.Int32)");
            var c = GetCodeDocMethod("TestLibrary1.Seal.SealIt.SealMe(System.Int32)");
            var d = GetCodeDocMethod("TestLibrary1.Seal.SealSomeOfIt.SealMe(System.Int32)");
            Assert.Equal("From BaseClassToSeal.", a.SummaryContents.First().Node.OuterXml);
            Assert.Equal("From BaseClassToSeal.", b.SummaryContents.First().Node.OuterXml);
            Assert.Equal("From SealIt.", c.SummaryContents.First().Node.OuterXml);
            Assert.Equal("From BaseClassToSeal.", d.SummaryContents.First().Node.OuterXml);
        }

        [Fact]
        public void explicit_interface_implementation_is_hidden(){
            var type = GetCodeDocType("T:TestLibrary1.Class1.ProtectedStruct");
            Assert.True(type.Methods.All(m => m.NamespaceName.StartsWith("System")));
        }
    }
}
