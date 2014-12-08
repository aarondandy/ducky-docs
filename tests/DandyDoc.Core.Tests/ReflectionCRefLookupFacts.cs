using System;
using System.Linq;
using System.Reflection;
using DuckyDocs.CRef;
using TestLibrary1;
using Xunit;

namespace DuckyDocs.Core.Tests
{
    public class ReflectionCRefLookupFacts
    {
        public ReflectionCRefLookupFacts() {
            Assembly = typeof(Class1).Assembly;
        }

        private Assembly Assembly { get; set; }

        public ReflectionCRefLookup Lookup {
            get {
                return new ReflectionCRefLookup(new[] { Assembly });
            }
        }

        [Fact]
        public void normal_class() {
            var type = Lookup.GetMember("T:TestLibrary1.Class1");
            Assert.NotNull(type);
            Assert.Equal("Class1", type.Name);
        }

        [Fact]
        public void normal_class_guess_cref_type() {
            var type = Lookup.GetMember("TestLibrary1.Class1");
            Assert.NotNull(type);
            Assert.Equal("Class1", type.Name);
        }

        [Fact]
        public void normal_method_no_params() {
            var member = Lookup.GetMember("M:TestLibrary1.Class1.BlankStatic");
            Assert.NotNull(member);
            Assert.Equal("BlankStatic", member.Name);
        }

        [Fact]
        public void normal_method_no_params_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.BlankStatic");
            Assert.NotNull(member);
            Assert.Equal("BlankStatic", member.Name);
        }

        [Fact]
        public void normal_method_one_param() {
            var member = Lookup.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.NotNull(member);
            var method = member as MethodInfo;
            Assert.NotNull(method);
            Assert.Equal("DoubleStatic", method.Name);
            Assert.Equal(1, method.GetParameters().Length);
            Assert.Equal("System.Double", method.GetParameters()[0].ParameterType.FullName);
        }

        [Fact]
        public void normal_method_one_param_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.DoubleStatic(System.Double)");
            Assert.NotNull(member);
            var method = member as MethodInfo;
            Assert.NotNull(method);
            Assert.Equal("DoubleStatic", method.Name);
            Assert.Equal(1, method.GetParameters().Length);
            Assert.Equal("System.Double", method.GetParameters()[0].ParameterType.FullName);
        }

        [Fact]
        public void normal_property() {
            var member = Lookup.GetMember("P:TestLibrary1.Class1.SomeProperty");
            Assert.NotNull(member);
            Assert.Equal("SomeProperty", member.Name);
        }

        [Fact]
        public void normal_property_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.SomeProperty");
            Assert.NotNull(member);
            Assert.Equal("SomeProperty", member.Name);
        }

        [Fact]
        public void normal_field() {
            var member = Lookup.GetMember("F:TestLibrary1.Class1.SomeField");
            Assert.NotNull(member);
            Assert.Equal("SomeField", member.Name);
        }

        [Fact]
        public void normal_field_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.SomeField");
            Assert.NotNull(member);
            Assert.Equal("SomeField", member.Name);
        }

        [Fact]
        public void normal_const() {
            var member = Lookup.GetMember("F:TestLibrary1.Class1.MyConst");
            Assert.NotNull(member);
            var field = member as FieldInfo;
            Assert.NotNull(field);
            Assert.Equal("MyConst", field.Name);
            Assert.NotNull(field.GetRawConstantValue());
        }

        [Fact]
        public void normal_const_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.MyConst");
            Assert.NotNull(member);
            var field = member as FieldInfo;
            Assert.NotNull(field);
            Assert.Equal("MyConst", field.Name);
            Assert.NotNull(field.GetRawConstantValue());
        }

        [Fact]
        public void normal_event() {
            var member = Lookup.GetMember("E:TestLibrary1.Class1.DoStuff");
            Assert.NotNull(member);
            var e = member as EventInfo;
            Assert.NotNull(e);
            Assert.Equal("DoStuff", e.Name);
            Assert.Equal("MyFunc", e.EventHandlerType.Name);
        }

        [Fact]
        public void normal_event_guess_type() {
            var member = Lookup.GetMember("TestLibrary1.Class1.DoStuff");
            Assert.NotNull(member);
            var e = member as EventInfo;
            Assert.NotNull(e);
            Assert.Equal("DoStuff", e.Name);
            Assert.Equal("MyFunc", e.EventHandlerType.Name);
        }

        [Fact]
        public void normal_operator() {
            var op = Lookup.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)")
                as MethodInfo;
            Assert.NotNull(op);
            Assert.Equal("op_Addition", op.Name);
            Assert.True(op.IsStatic);
            Assert.Equal(2, op.GetParameters().Length);
            Assert.True(op.GetParameters().Select(x => x.ParameterType.FullName).All(x => x == "TestLibrary1.Class1"));
        }

        [Fact]
        public void normal_indexer() {
            var member = Lookup.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
            Assert.NotNull(member);
            var indexer = member as PropertyInfo;
            Assert.NotNull(indexer);
            Assert.Equal("Item", indexer.Name);
            Assert.Equal(1, indexer.GetIndexParameters().Length);
        }

        [Fact]
        public void normal_constructor_one_param() {
            var member = Lookup.GetMember("M:TestLibrary1.Class1.#ctor(System.String)");
            Assert.NotNull(member);
            var ctor = member as ConstructorInfo;
            Assert.NotNull(ctor);
            Assert.False(ctor.IsStatic);
            Assert.True(ctor.GetParameters().Length > 0);
            Assert.Equal(1, ctor.GetParameters().Length);
            Assert.Equal("System.String", ctor.GetParameters()[0].ParameterType.FullName);
        }

        [Fact]
        public void normal_constructor_two_param() {
            var member = Lookup.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
            Assert.NotNull(member);
            var ctor = member as ConstructorInfo;
            Assert.NotNull(ctor);
            Assert.False(ctor.IsStatic);
            Assert.Equal(2, ctor.GetParameters().Length);
            Assert.True(ctor.GetParameters().Select(x => x.ParameterType.FullName).All(x => x == "System.String"));
        }

        [Fact]
        public void normal_finalizer() {
            var member = Lookup.GetMember("M:TestLibrary1.Class1.Finalize");
            Assert.NotNull(member);
            var finalizer = member as MethodInfo;
            Assert.NotNull(finalizer);
            Assert.Equal("Finalize", finalizer.Name);
            Assert.False(finalizer.IsStatic);
            Assert.False(finalizer.GetParameters().Length > 0);
        }

        [Fact]
        public void normal_inner() {
            var inner = Lookup.GetMember("T:TestLibrary1.Class1.Inner")
                as Type;
            Assert.NotNull(inner);
            Assert.Equal("Inner", inner.Name);
            Assert.True(inner.IsNested);
        }

        [Fact]
        public void normal_inner_property() {
            var member = Lookup.GetMember("P:TestLibrary1.Class1.Inner.Name");
            Assert.NotNull(member);
            Assert.Equal("Name", member.Name);
        }

        [Fact]
        public void global_namespace_type() {
            var type = Lookup.GetMember("T:InGlobal");
            Assert.NotNull(type);
            Assert.Equal("InGlobal", type.Name);
        }

        [Fact]
        public void global_namespace_guess_type() {
            var type = Lookup.GetMember("InGlobal");
            Assert.NotNull(type);
            Assert.Equal("InGlobal", type.Name);
        }

        [Fact]
        public void invalid_double_dot_cref_to_type() {
            var type = Lookup.GetMember("TestLibrary1..Class1");
            Assert.Null(type);
        }

        [Fact]
        public void invalid_trailing_dot_cref_to_type() {
            var type = Lookup.GetMember("TestLibrary1.Class1.");
            Assert.Null(type);
        }

        [Fact]
        public void invalid_double_dot_namespace_cref_to_type() {
            var type = Lookup.GetMember("TestLibrary1..Seal.NotSealed");
            Assert.Null(type);
        }

        [Fact]
        public void invalid_empty_cref_to_type() {
            var type = Lookup.GetMember("T:");
            Assert.Null(type);
        }

        [Fact]
        public void invalid_cref_type_to_member() {
            var type = Lookup.GetMember("Z:TestLibrary1.Class1.DoubleStatic(System.Int32)");
            Assert.Null(type);
        }

        [Fact]
        public void generic_class() {
            var type = Lookup.GetMember("T:TestLibrary1.Generic1`2");
            Assert.NotNull(type);
            Assert.Equal("Generic1`2", type.Name);
        }

        [Fact]
        public void generic_method_one_param() {
            var member = Lookup.GetMember("M:TestLibrary1.Generic1`2.Junk1``1(``0)");
            Assert.NotNull(member);
            Assert.Equal("Junk1", member.Name);
            var method = member as MethodInfo;
            Assert.NotNull(method);
            Assert.Equal(1, method.GetParameters().Length);
            Assert.True(method.GetParameters()[0].ParameterType.IsGenericParameter);
        }

        [Fact]
        public void generic_property() {
            var member = Lookup.GetMember("P:TestLibrary1.Generic1`2.A")
                as PropertyInfo;
            Assert.NotNull(member);
            Assert.Equal("A", member.Name);
        }

        [Fact]
        public void generic_field() {
            var member = Lookup.GetMember("F:TestLibrary1.Generic1`2.B")
                as FieldInfo;
            Assert.NotNull(member);
            Assert.Equal("B", member.Name);
        }

        [Fact]
        public void delegate_with_generics_from_parent() {
            var member = Lookup.GetMember("T:TestLibrary1.Generic1`2.MyFunc");
            Assert.NotNull(member);
            Assert.Equal("MyFunc", member.Name);
        }

        [Fact]
        public void delegate_with_own_generic() {
            var member = Lookup.GetMember("T:TestLibrary1.Generic1`2.MyFunc`1");
            Assert.NotNull(member);
            var method = member as Type;
            Assert.NotNull(method);
            Assert.Equal("MyFunc`1", method.Name);
            Assert.Equal(3, method.GetGenericArguments().Length);
        }

        [Fact]
        public void generic_event() {
            var e = Lookup.GetMember("E:TestLibrary1.Generic1`2.E")
                as EventInfo;
            Assert.NotNull(e);
            Assert.Equal("E", e.Name);
        }

        [Fact]
        public void generic_operator() {
            var op = Lookup.GetMember("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})")
                as MethodInfo;
            Assert.NotNull(op);
            Assert.Equal("op_Addition", op.Name);
            Assert.True(op.IsStatic);
            Assert.Equal(2, op.GetParameters().Length);
            Assert.True(op.GetParameters().Select(x => x.ParameterType.FullName).All(x => x.StartsWith("TestLibrary1.Generic1")));
        }

        [Fact]
        public void generic_inner_mixed_params() {
            var m = Lookup.GetMember("M:TestLibrary1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)")
                as MethodInfo;
            Assert.NotNull(m);
            Assert.True(m.Name.StartsWith("Junk3"));
            Assert.Equal(4, m.GetParameters().Length);
        }

        [Fact]
        public void generic_inner_method_with_only_class_generics() {
            var m = Lookup.GetMember("M:TestLibrary1.Generic1`2.Inner`1.Junk4(`2,`1,`0)")
                as MethodInfo;
            Assert.NotNull(m);
            Assert.True(m.Name.StartsWith("Junk4"));
            Assert.Equal(3, m.GetParameters().Length);
        }

        [Fact]
        public void method_ref_out_params() {
            var m = Lookup.GetMember("M:TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32@,System.Int32@)")
                as MethodInfo;
            Assert.NotNull(m);
            Assert.Equal("TrySomeOutRefStuff", m.Name);
            Assert.Equal(2, m.GetParameters().Length);
        }

        [Fact]
        public void can_lookup_conversion_operator_by_return_type(){
            var convertToString = Lookup.GetMember("M:TestLibrary1.Class1.op_Implicit(TestLibrary1.Class1)~System.String") as MethodInfo;
            Assert.NotNull(convertToString);
            Assert.Equal(typeof(string), convertToString.ReturnType);
            var convertToInt32 = Lookup.GetMember("M:TestLibrary1.Class1.op_Implicit(TestLibrary1.Class1)~System.Int32") as MethodInfo;
            Assert.NotNull(convertToInt32);
            Assert.Equal(typeof(int), convertToInt32.ReturnType);
        }

        [Fact]
        public void can_find_array_type() {
            var type = Lookup.GetMember("T:TestLibrary1.Class1[]") as Type;
            Assert.NotNull(type);
            Assert.Equal("TestLibrary1.Class1[]", type.FullName);
        }

        [Fact]
        public void can_find_ref_type() {
            var type = Lookup.GetMember("T:TestLibrary1.Class1@") as Type;
            Assert.NotNull(type);
            Assert.Equal("TestLibrary1.Class1&", type.FullName);
        }
    }
}
