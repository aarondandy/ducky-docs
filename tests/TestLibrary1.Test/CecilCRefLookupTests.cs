using System.Linq;
using DandyDoc.CRef;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class CecilCRefLookupTests
	{

		public CecilCRefLookupTests() {
			Assembly = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
		}

		private AssemblyDefinition Assembly { get; set; }

		public CecilCRefLookup Lookup {
			get {
				return new CecilCRefLookup(new[]{Assembly});
			}
		}

		[Test]
		public void normal_class() {
			var type = Lookup.GetMember("T:TestLibrary1.Class1");
			Assert.IsNotNull(type);
			Assert.AreEqual("Class1", type.Name);
		}

		[Test]
		public void normal_class_guess_cref_type() {
			var type = Lookup.GetMember("TestLibrary1.Class1");
			Assert.IsNotNull(type);
			Assert.AreEqual("Class1", type.Name);
		}

		[Test]
		public void normal_method_no_params() {
			var member = Lookup.GetMember("M:TestLibrary1.Class1.BlankStatic");
			Assert.IsNotNull(member);
			Assert.AreEqual("BlankStatic", member.Name);
		}

		[Test]
		public void normal_method_no_params_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.BlankStatic");
			Assert.IsNotNull(member);
			Assert.AreEqual("BlankStatic", member.Name);
		}

		[Test]
		public void normal_method_one_param() {
			var member = Lookup.GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Double)");
			Assert.IsNotNull(member);
			var method = member as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("DoubleStatic", method.Name);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.AreEqual("System.Double", method.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void normal_method_one_param_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.DoubleStatic(System.Double)");
			Assert.IsNotNull(member);
			var method = member as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("DoubleStatic", method.Name);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.AreEqual("System.Double", method.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void normal_property() {
			var member = Lookup.GetMember("P:TestLibrary1.Class1.SomeProperty");
			Assert.IsNotNull(member);
			Assert.AreEqual("SomeProperty", member.Name);
		}

		[Test]
		public void normal_property_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.SomeProperty");
			Assert.IsNotNull(member);
			Assert.AreEqual("SomeProperty", member.Name);
		}

		[Test]
		public void normal_field() {
			var member = Lookup.GetMember("F:TestLibrary1.Class1.SomeField");
			Assert.IsNotNull(member);
			Assert.AreEqual("SomeField", member.Name);
		}

		[Test]
		public void normal_field_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.SomeField");
			Assert.IsNotNull(member);
			Assert.AreEqual("SomeField", member.Name);
		}

		[Test]
		public void normal_const() {
			var member = Lookup.GetMember("F:TestLibrary1.Class1.MyConst");
			Assert.IsNotNull(member);
			var field = member as FieldDefinition;
			Assert.IsNotNull(field);
			Assert.AreEqual("MyConst", field.Name);
			Assert.IsNotNull(field.Constant);
		}

		[Test]
		public void normal_const_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.MyConst");
			Assert.IsNotNull(member);
			var field = member as FieldDefinition;
			Assert.IsNotNull(field);
			Assert.AreEqual("MyConst", field.Name);
			Assert.IsNotNull(field.Constant);
		}

		[Test]
		public void normal_event() {
			var member = Lookup.GetMember("E:TestLibrary1.Class1.DoStuff");
			Assert.IsNotNull(member);
			var e = member as EventDefinition;
			Assert.IsNotNull(e);
			Assert.AreEqual("DoStuff", e.Name);
			Assert.AreEqual("MyFunc", e.EventType.Name);
		}

		[Test]
		public void normal_event_guess_type() {
			var member = Lookup.GetMember("TestLibrary1.Class1.DoStuff");
			Assert.IsNotNull(member);
			var e = member as EventDefinition;
			Assert.IsNotNull(e);
			Assert.AreEqual("DoStuff", e.Name);
			Assert.AreEqual("MyFunc", e.EventType.Name);
		}

		[Test]
		public void normal_operator() {
			var op = Lookup.GetMember("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)") as MethodDefinition;
			Assert.IsNotNull(op);
			Assert.AreEqual("op_Addition", op.Name);
			Assert.That(op.IsStatic);
			Assert.AreEqual(2, op.Parameters.Count);
			Assert.That(
				op.Parameters.Select(x => x.ParameterType.FullName),
				Has.All.EqualTo("TestLibrary1.Class1"));
		}

		[Test]
		public void normal_indexer() {
			var member = Lookup.GetMember("P:TestLibrary1.Class1.Item(System.Int32)");
			Assert.IsNotNull(member);
			var indexer = member as PropertyDefinition;
			Assert.IsNotNull(indexer);
			Assert.AreEqual("Item", indexer.Name);
			Assert.AreEqual(1, indexer.Parameters.Count);
		}

		[Test]
		public void normal_constructor_one_param() {
			var member = Lookup.GetMember("M:TestLibrary1.Class1.#ctor(System.String)");
			Assert.IsNotNull(member);
			var ctor = member as MethodDefinition;
			Assert.IsNotNull(ctor);
			Assert.IsFalse(ctor.IsStatic);
			Assert.IsTrue(ctor.HasParameters);
			Assert.AreEqual(1, ctor.Parameters.Count);
			Assert.AreEqual("System.String", ctor.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void normal_constructor_two_param() {
			var member = Lookup.GetMember("M:TestLibrary1.Class1.#ctor(System.String,System.String)");
			Assert.IsNotNull(member);
			var ctor = member as MethodDefinition;
			Assert.IsNotNull(ctor);
			Assert.IsFalse(ctor.IsStatic);
			Assert.IsTrue(ctor.HasParameters);
			Assert.AreEqual(2, ctor.Parameters.Count);
			Assert.That(ctor.Parameters.Select(x => x.ParameterType.FullName), Has.All.EqualTo("System.String"));
		}

		[Test]
		public void normal_finalizer() {
			var member = Lookup.GetMember("M:TestLibrary1.Class1.Finalize");
			Assert.IsNotNull(member);
			var finalizer = member as MethodDefinition;
			Assert.IsNotNull(finalizer);
			Assert.AreEqual("Finalize", finalizer.Name);
			Assert.IsFalse(finalizer.IsStatic);
			Assert.IsFalse(finalizer.HasParameters);
		}

		[Test]
		public void normal_inner() {
			var inner = Lookup.GetMember("T:TestLibrary1.Class1.Inner") as TypeDefinition;
			Assert.IsNotNull(inner);
			Assert.AreEqual("Inner", inner.Name);
			Assert.IsTrue(inner.IsNested);
		}

		[Test]
		public void normal_inner_property() {
			var member = Lookup.GetMember("P:TestLibrary1.Class1.Inner.Name");
			Assert.IsNotNull(member);
			Assert.AreEqual("Name", member.Name);
		}

		[Test]
		public void global_namespace_type() {
			var type = Lookup.GetMember("T:InGlobal");
			Assert.IsNotNull(type);
			Assert.AreEqual("InGlobal", type.Name);
		}

		[Test]
		public void global_namespace_guess_type() {
			var type = Lookup.GetMember("InGlobal");
			Assert.IsNotNull(type);
			Assert.AreEqual("InGlobal", type.Name);
		}

		[Test]
		public void invalid_double_dot_cref_to_type() {
			var type = Lookup.GetMember("TestLibrary1..Class1");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_trailing_dot_cref_to_type() {
			var type = Lookup.GetMember("TestLibrary1.Class1.");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_double_dot_namespace_cref_to_type() {
			var type = Lookup.GetMember("TestLibrary1..Seal.NotSealed");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_empty_cref_to_type() {
			var type = Lookup.GetMember("T:");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_cref_type_to_member() {
			var type = Lookup.GetMember("Z:TestLibrary1.Class1.DoubleStatic(System.Int32)");
			Assert.IsNull(type);
		}

		[Test]
		public void generic_class() {
			var type = Lookup.GetMember("T:TestLibrary1.Generic1`2");
			Assert.IsNotNull(type);
			Assert.AreEqual("Generic1`2", type.Name);
		}

		[Test]
		public void generic_method_one_param() {
			var member = Lookup.GetMember("M:TestLibrary1.Generic1`2.Junk1``1(``0)");
			Assert.IsNotNull(member);
			Assert.AreEqual("Junk1", member.Name);
			var method = member as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.IsTrue(method.Parameters[0].ParameterType.IsGenericParameter);
		}

		[Test]
		public void generic_property() {
			var member = Lookup.GetMember("P:TestLibrary1.Generic1`2.A") as PropertyDefinition;
			Assert.IsNotNull(member);
			Assert.AreEqual("A", member.Name);
		}

		[Test]
		public void generic_field() {
			var member = Lookup.GetMember("F:TestLibrary1.Generic1`2.B") as FieldDefinition;
			Assert.IsNotNull(member);
			Assert.AreEqual("B", member.Name);
		}

		[Test]
		public void delegate_with_generics_from_parent() {
			var member = Lookup.GetMember("T:TestLibrary1.Generic1`2.MyFunc");
			Assert.IsNotNull(member);
			Assert.AreEqual("MyFunc", member.Name);
		}

		[Test]
		public void delegate_with_own_generic() {
			var member = Lookup.GetMember("T:TestLibrary1.Generic1`2.MyFunc`1");
			Assert.IsNotNull(member);
			var method = member as TypeDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("MyFunc`1", method.Name);
			Assert.IsTrue(method.HasGenericParameters);
			Assert.AreEqual(3, method.GenericParameters.Count);
		}

		[Test]
		public void generic_event() {
			var e = Lookup.GetMember("E:TestLibrary1.Generic1`2.E") as EventDefinition;
			Assert.IsNotNull(e);
			Assert.AreEqual("E", e.Name);
		}

		[Test]
		public void generic_operator() {
			var op = Lookup.GetMember("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})") as MethodDefinition;
			Assert.IsNotNull(op);
			Assert.AreEqual("op_Addition", op.Name);
			Assert.That(op.IsStatic);
			Assert.AreEqual(2, op.Parameters.Count);
			Assert.That(
				op.Parameters.Select(x => x.ParameterType.FullName),
				Has.All.StartsWith("TestLibrary1.Generic1"));
		}

		[Test]
		public void generic_inner_mixed_params() {
			var m = Lookup.GetMember("M:TestLibrary1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)") as MethodDefinition;
			Assert.IsNotNull(m);
			Assert.That(m.Name.StartsWith("Junk3"));
			Assert.AreEqual(4, m.Parameters.Count);
		}

		[Test]
		public void generic_inner_method_with_only_class_generics() {
			var m = Lookup.GetMember("M:TestLibrary1.Generic1`2.Inner`1.Junk4(`2,`1,`0)") as MethodDefinition;
			Assert.IsNotNull(m);
			Assert.That(m.Name.StartsWith("Junk4"));
			Assert.AreEqual(3, m.Parameters.Count);
		}

	}
}
