using System;
using System.Reflection;
using DandyDoc.CRef;
using DandyDoc.DisplayName;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.Core.Tests
{

	[TestFixture]
	public class ReflectionDisplayNameGeneratorTests
	{

		private static Assembly GetAssembly() {
			return typeof(Class1).Assembly;
		}

		public ReflectionDisplayNameGeneratorTests() {
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

		[Test]
		public void normal_type() {
			Assert.AreEqual("Class1", Default.GetDisplayName(GetType("T:TestLibrary1.Class1")));
			Assert.AreEqual("TestLibrary1.Class1", Full.GetDisplayName(GetType("T:TestLibrary1.Class1")));
		}

		[Test]
		public void normal_nested_type() {
			Assert.AreEqual("Inner", Default.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
			Assert.AreEqual("TestLibrary1.Class1.Inner", Full.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
		}

		[Test]
		public void generic_type() {
			Assert.AreEqual("Generic1<TA, TB>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
		}

		[Test]
		public void generic_nested_type() {
			Assert.AreEqual("Inner<TC>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.Inner<TC>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
		}

		[Test]
		public void normal_constructor() {
			Assert.AreEqual("Class1(String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
			Assert.AreEqual("TestLibrary1.Class1.Class1(System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
		}

		[Test]
		public void generic_constructor() {
			Assert.AreEqual("Generic1(TA, TB, IEnumerable<TA>, String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.Generic1(TA, TB, System.Collections.Generic.IEnumerable<TA>, System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
		}

		[Test]
		public void normal_method() {
			Assert.AreEqual("DoubleStatic(Int32)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
			Assert.AreEqual("TestLibrary1.Class1.DoubleStatic(System.Int32)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
		}

		[Test]
		public void generic_method() {
			Assert.AreEqual("AMix<TOther>(TA, TOther)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.AMix<TOther>(TA, TOther)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
		}

		[Test]
		public void generic_operator() {
			Assert.AreEqual("operator +(Generic1<Int32, Int32[]>, Generic1<TA, TB>)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})")));
		}

		[Test]
		public void normal_property() {
			var member = GetMember("P:TestLibrary1.Class1.SomeProperty");
			Assert.AreEqual("SomeProperty", Default.GetDisplayName(member));
			Assert.AreEqual("TestLibrary1.Class1.SomeProperty", Full.GetDisplayName(member));
		}

		[Test]
		public void indexer_property() {
			var member = GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
			Assert.AreEqual("Item[Int32]", Default.GetDisplayName(member));
			Assert.AreEqual("TestLibrary1.Class1.Item[System.Int32]", Full.GetDisplayName(member));
		}

		[Test]
		public void normal_field() {
			var member = GetMember("F:TestLibrary1.Class1.SomeField");
			Assert.AreEqual("SomeField", Default.GetDisplayName(member));
			Assert.AreEqual("TestLibrary1.Class1.SomeField", Full.GetDisplayName(member));
		}

		[Test]
		public void null_type_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((Type)null));
		}

		[Test]
		public void null_member_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MemberInfo)null));
		}

		[Test]
		public void null_method_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MethodInfo)null));
		}

		[Test]
		public void null_property_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((PropertyInfo)null));
		}

		[Test]
		public void null_field_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((FieldInfo)null));
		}

		[Test]
		public void null_event_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((EventInfo)null));
		}

	}

}
