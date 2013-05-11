using System;
using System.Linq;
using System.Reflection;
using DandyDoc.CRef;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.Core.Tests
{
    [TestFixture]
    public class ReflectionCRefGeneratorTests
    {

        private class DoABunchOfHorribleNullStuff : ReflectionCRefGenerator
        {

            public void DoNullStuff() {
                Assert.IsNull(GetCRef((object)null));
                Assert.IsNull(GetCRef("hot dog!"));
                Assert.Throws<ArgumentNullException>(() => GetCRef((MemberInfo)null));
                Assert.Throws<ArgumentNullException>(() => GetCRef((Type)null));
                Assert.Throws<ArgumentNullException>(() => GetGenericParameterName(null));
                Assert.Throws<ArgumentNullException>(() => GetFullName(null));

            }

        }

        public ReflectionCRefGenerator Generator {
            get {
                return new ReflectionCRefGenerator();
            }
        }

        [Test]
        public void null_cref_object_generation() {
            var doNulls = new DoABunchOfHorribleNullStuff();
            doNulls.DoNullStuff();
        }

        [Test]
        public void normal_class() {
            var type = typeof(Class1);
            Assert.AreEqual("T:TestLibrary1.Class1", Generator.GetCRef(type));
        }

        [Test]
        public void normal_method_no_params() {
            var member = typeof(Class1).GetMethod("BlankStatic");
            Assert.AreEqual("M:TestLibrary1.Class1.BlankStatic", Generator.GetCRef(member));
        }

        [Test]
        public void normal_method_one_param() {
            var member = typeof(Class1).GetMethods().First(x => x.Name == "DoubleStatic" && x.ReturnType == typeof(double));
            Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Double)", Generator.GetCRef(member));
        }

        [Test]
        public void normal_property() {
            var member = typeof(Class1).GetProperty("SomeProperty");
            Assert.AreEqual("P:TestLibrary1.Class1.SomeProperty", Generator.GetCRef(member));
        }

        [Test]
        public void normal_field() {
            var member = typeof(Class1).GetField("SomeField");
            Assert.AreEqual("F:TestLibrary1.Class1.SomeField", Generator.GetCRef(member));
        }

        [Test]
        public void normal_const() {
            var member = typeof(Class1).GetField("MyConst");
            Assert.AreEqual("F:TestLibrary1.Class1.MyConst", Generator.GetCRef(member));
        }

        [Test]
        public void normal_delegate() {
            var member = typeof(Class1).GetNestedTypes().First(x => x.Name == "MyFunc");
            Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", Generator.GetCRef(member));
        }

        [Test]
        public void normal_event() {
            var member = typeof(Class1).GetEvent("DoStuff");
            Assert.AreEqual("E:TestLibrary1.Class1.DoStuff", Generator.GetCRef(member));
        }

        [Test]
        public void normal_operator() {
            var member = typeof(Class1).GetMethods().First(x => x.Name.Contains("Addition"));
            Assert.AreEqual("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)", Generator.GetCRef(member));
        }

        [Test]
        public void normal_indexer() {
            var member = typeof(Class1).GetProperties().First(x => x.Name == "Item");
            Assert.AreEqual("P:TestLibrary1.Class1.Item(System.Int32)", Generator.GetCRef(member));
        }

        [Test]
        public void normal_static_constructor_no_params() {
            var member = typeof(Class1).GetConstructors(BindingFlags.NonPublic | BindingFlags.Static).First();
            Assert.AreEqual("M:TestLibrary1.Class1.#cctor", Generator.GetCRef(member));
        }

        [Test]
        public void normal_constructor_one_param() {
            var member = typeof(Class1).GetConstructors().First(x => x.GetParameters().Length == 1);
            Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String)", Generator.GetCRef(member));
        }

        [Test]
        public void normal_constructor_two_param() {
            var member = typeof(Class1).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String,System.String)", Generator.GetCRef(member));
        }

        [Test]
        public void normal_finalizer() {
            var member = typeof(Class1).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "Finalize");
            Assert.AreEqual("M:TestLibrary1.Class1.Finalize", Generator.GetCRef(member));
        }

        [Test]
        public void normal_nested_type() {
            var member = typeof(Class1).GetNestedTypes().First(x => x.Name == "Inner");
            Assert.AreEqual("T:TestLibrary1.Class1.Inner", Generator.GetCRef(member));
        }

        [Test]
        public void normal_nested_property() {
            var member = typeof(Class1).GetNestedTypes().First(x => x.Name == "Inner").GetProperties().First(x => x.Name == "Name");
            Assert.AreEqual("P:TestLibrary1.Class1.Inner.Name", Generator.GetCRef(member));
        }

        [Test]
        public void normal_global_namespace_type() {
            var member = typeof(InGlobal);
            Assert.AreEqual("T:InGlobal", Generator.GetCRef(member));
        }

        [Test]
        public void generic_class() {
            var member = typeof(Generic1<,>);
            Assert.AreEqual("T:TestLibrary1.Generic1`2", Generator.GetCRef(member));
        }

        [Test]
        public void generic_method_one_param() {
            var member = typeof(Generic1<,>).GetMethods().First(x => x.Name == "Junk1");
            Assert.AreEqual("M:TestLibrary1.Generic1`2.Junk1``1(``0)", Generator.GetCRef(member));
        }

        [Test]
        public void generic_property() {
            var member = typeof(Generic1<,>).GetProperties().First(x => x.Name == "A");
            Assert.AreEqual("P:TestLibrary1.Generic1`2.A", Generator.GetCRef(member));
        }

        [Test]
        public void generic_field() {
            var member = typeof(Generic1<,>).GetFields().First(x => x.Name == "B");
            Assert.AreEqual("F:TestLibrary1.Generic1`2.B", Generator.GetCRef(member));
        }

        [Test]
        public void generic_with_nested_delegate() {
            var member = typeof(Generic1<,>).GetNestedTypes().First(x => x.Name == "MyFunc");
            Assert.AreEqual("T:TestLibrary1.Generic1`2.MyFunc", Generator.GetCRef(member));
        }

        [Test]
        public void generic_delegate_within_generic() {
            var member = typeof(Generic1<,>).GetNestedTypes().First(x => x.Name == "MyFunc`1");
            Assert.AreEqual("T:TestLibrary1.Generic1`2.MyFunc`1", Generator.GetCRef(member));
        }

        [Test]
        public void generic_event() {
            var member = typeof(Generic1<,>).GetEvents().First(x => x.Name == "E");
            Assert.AreEqual("E:TestLibrary1.Generic1`2.E", Generator.GetCRef(member));
        }

        [Test]
        public void crazy_generic_operator() {
            var member = typeof(Generic1<,>).GetMethods().First(x => x.Name.Contains("Addition"));
            Assert.AreEqual("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})", Generator.GetCRef(member));
        }

        [Test]
        public void generic_nested_mixed_params() {
            var member = typeof(Generic1<,>).GetNestedTypes().First(x => x.Name == "Inner`1").GetMethods().First(x => x.Name.StartsWith("Junk3"));
            Assert.AreEqual("M:TestLibrary1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)", Generator.GetCRef(member));
        }

        [Test]
        public void generic_nested_class_generic_params() {
            var member = typeof(Generic1<,>).GetNestedTypes().First(x => x.Name == "Inner`1").GetMethods().First(x => x.Name.StartsWith("Junk4"));
            Assert.AreEqual("M:TestLibrary1.Generic1`2.Inner`1.Junk4(`2,`1,`0)", Generator.GetCRef(member));
        }

        [Test]
        public void generic_crazy_constructor() {
            var member = typeof(Generic1<,>).GetConstructors().First(x => x.GetParameters().Length == 4);
            Assert.AreEqual("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)", Generator.GetCRef(member));
        }

        [Test]
        public void ref_out_param_method() {
            var member = typeof(Class1).GetMethods().Single(x => x.Name == "TrySomeOutRefStuff");
            Assert.AreEqual("M:TestLibrary1.Class1.TrySomeOutRefStuff(System.Int32@,System.Int32@)", Generator.GetCRef(member));
        }

    }
}
