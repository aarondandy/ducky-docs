using System;
using DandyDoc.CRef;
using DandyDoc.DisplayName;
using Mono.Cecil;
using NUnit.Framework;

namespace DandyDoc.Core.Cecil.Tests
{
	[TestFixture]
	public class CecilDisplayNameGeneratorTests
	{

		private static AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		public CecilDisplayNameGeneratorTests() {
			Default = new StandardCecilDisplayNameGenerator();
			Full = new StandardCecilDisplayNameGenerator {
				IncludeNamespaceForTypes = true,
				ShowGenericParametersOnDefinition = true,
				ShowTypeNameForMembers = true
			};
			Lookup = new CecilCRefLookup(new[] { GetAssembly() });
		}

		public StandardCecilDisplayNameGenerator Default { get; private set; }

		public StandardCecilDisplayNameGenerator Full { get; private set; }

		public CecilCRefLookup Lookup { get; private set; }

		public TypeDefinition GetType(string cRef) {
			return Lookup.GetMember(cRef) as TypeDefinition;
		}

		public IMemberDefinition GetMember(string cRef) {
			return Lookup.GetMember(cRef) as IMemberDefinition;
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
		public void null_type_ref_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((TypeReference)null));
		}

		[Test]
		public void null_member_ref_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MemberReference)null));
		}

		[Test]
		public void null_member_def_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((IMemberDefinition)null));
		}

		[Test]
		public void null_method_def_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((MethodDefinition)null));
		}

		[Test]
		public void null_property_def_test() {
			Assert.Throws<ArgumentNullException>(() => Default.GetDisplayName((PropertyDefinition)null));
		}

	}
}
