using System;
using System.Reflection;
using DuckyDocs.CRef;
using DuckyDocs.DisplayName;
using TestLibrary1;
using Xunit;

namespace DuckyDocs.Core.Tests
{
    public class ReflectionDisplayNameGeneratorFacts
    {
        private static Assembly GetAssembly() {
            return typeof(Class1).Assembly;
        }

        public ReflectionDisplayNameGeneratorFacts() {
            Default = new StandardReflectionDisplayNameGenerator();
            Full = new StandardReflectionDisplayNameGenerator {
                IncludeNamespaceForTypes = true,
                ShowGenericParametersOnDefinition = true,
                ShowTypeNameForMembers = true
            };
            Lookup = new ReflectionCRefLookup(new[] { GetAssembly() });
        }

        public StandardReflectionDisplayNameGenerator Default { get; private set; }

        public StandardReflectionDisplayNameGenerator Full { get; private set; }

        public ReflectionCRefLookup Lookup { get; private set; }

        public Type GetType(string cRef) {
            return Lookup.GetMember(cRef) as Type;
        }

        public MemberInfo GetMember(string cRef) {
            return Lookup.GetMember(cRef);
        }

        [Fact]
        public void normal_type() {
            Assert.Equal("Class1", Default.GetDisplayName(GetType("T:TestLibrary1.Class1")));
            Assert.Equal("TestLibrary1.Class1", Full.GetDisplayName(GetType("T:TestLibrary1.Class1")));
        }

        [Fact]
        public void normal_nested_type() {
            Assert.Equal("Inner", Default.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
            Assert.Equal("TestLibrary1.Class1.Inner", Full.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
        }

        [Fact]
        public void generic_type() {
            Assert.Equal("Generic1<TA, TB>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
            Assert.Equal("TestLibrary1.Generic1<TA, TB>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
        }

        [Fact]
        public void generic_nested_type() {
            Assert.Equal("Inner<TC>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
            Assert.Equal("TestLibrary1.Generic1<TA, TB>.Inner<TC>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
        }

        [Fact]
        public void normal_constructor() {
            Assert.Equal("Class1(String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
            Assert.Equal("TestLibrary1.Class1.Class1(System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
        }

        [Fact]
        public void generic_constructor() {
            Assert.Equal("Generic1(TA, TB, IEnumerable<TA>, String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
            Assert.Equal("TestLibrary1.Generic1<TA, TB>.Generic1(TA, TB, System.Collections.Generic.IEnumerable<TA>, System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
        }

        [Fact]
        public void normal_method() {
            Assert.Equal("DoubleStatic(Int32)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
            Assert.Equal("TestLibrary1.Class1.DoubleStatic(System.Int32)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
        }

        [Fact]
        public void generic_method() {
            Assert.Equal("AMix<TOther>(TA, TOther)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
            Assert.Equal("TestLibrary1.Generic1<TA, TB>.AMix<TOther>(TA, TOther)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
        }

        [Fact]
        public void generic_operator() {
            Assert.Equal("operator +(Generic1<Int32, Int32[]>, Generic1<TA, TB>)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})")));
        }

        [Fact]
        public void normal_property() {
            var member = GetMember("P:TestLibrary1.Class1.SomeProperty");
            Assert.Equal("SomeProperty", Default.GetDisplayName(member));
            Assert.Equal("TestLibrary1.Class1.SomeProperty", Full.GetDisplayName(member));
        }

        [Fact]
        public void indexer_property() {
            var member = GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.Equal("Item[Int32]", Default.GetDisplayName(member));
            Assert.Equal("TestLibrary1.Class1.Item[System.Int32]", Full.GetDisplayName(member));
        }

        [Fact]
        public void normal_field() {
            var member = GetMember("F:TestLibrary1.Class1.SomeField");
            Assert.Equal("SomeField", Default.GetDisplayName(member));
            Assert.Equal("TestLibrary1.Class1.SomeField", Full.GetDisplayName(member));
        }

        [Fact]
        public void null_type_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((Type)null));
        }

        [Fact]
        public void null_member_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MemberInfo)null));
        }

        [Fact]
        public void null_method_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MethodInfo)null));
        }

        [Fact]
        public void null_property_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((PropertyInfo)null));
        }

        [Fact]
        public void null_field_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((FieldInfo)null));
        }

        [Fact]
        public void null_event_test() {
            Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((EventInfo)null));
        }
    }
}
