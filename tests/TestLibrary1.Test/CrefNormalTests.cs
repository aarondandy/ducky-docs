using System;
using System.Linq;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{

	[TestFixture]
	public class CrefNormalTests
	{

		private AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		private TypeDefinition GetType(AssemblyDefinition assemblyDefinition, string typeName) {
			var type = assemblyDefinition.Modules.SelectMany(x => x.Types).FirstOrDefault(t => t.Name == typeName);
			Assert.IsNotNull(type);
			return type;
		}

		[Test]
		public void cref_to_normal_class() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetTypeDefinition("T:TestLibrary1.Class1");
			Assert.IsNotNull(type);
			Assert.AreEqual("Class1", type.Name);
		}

		[Test]
		public void cref_to_normal_class_guess_cref_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetReference("TestLibrary1.Class1");
			Assert.IsNotNull(type);
			Assert.IsInstanceOf(typeof(TypeDefinition), type);
			Assert.AreEqual("Class1", type.Name);
		}

		[Test]
		public void cref_from_normal_class() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection{assembly});
			Assert.AreEqual("T:TestLibrary1.Class1", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_normal_method_no_params() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var member = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.BlankStatic");
			Assert.IsNotNull(member);
			Assert.AreEqual("BlankStatic", member.Name);
		}

		[Test]
		public void cref_to_normal_method_no_params_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var member = crefOverlay.GetReference("TestLibrary1.Class1.BlankStatic");
			Assert.IsNotNull(member);
			Assert.IsInstanceOf(typeof(MethodDefinition), member);
			Assert.AreEqual("BlankStatic", member.Name);
		}

		[Test]
		public void cref_from_normal_method_no_params() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var method = type.Methods.First(x => x.Name == "BlankStatic");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.BlankStatic", crefOverlay.GetCref(method));
		}

		[Test]
		public void cref_to_normal_method_one_param() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var method = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.DoubleStatic(System.Double)") as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("DoubleStatic", method.Name);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.AreEqual("System.Double", method.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void cref_to_normal_method_one_param_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var method = crefOverlay.GetReference("TestLibrary1.Class1.DoubleStatic(System.Double)") as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("DoubleStatic", method.Name);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.AreEqual("System.Double", method.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void cref_from_normal_method_one_param() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var method = type.Methods.First(x => x.Name == "DoubleStatic" && x.ReturnType.Name.EndsWith("Double"));
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.DoubleStatic(System.Double)", crefOverlay.GetCref(method));
		}

		[Test]
		public void cref_to_normal_property() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var property = crefOverlay.GetMemberDefinition("P:TestLibrary1.Class1.SomeProperty") as PropertyDefinition;
			Assert.IsNotNull(property);
			Assert.AreEqual("SomeProperty", property.Name);
		}

		[Test]
		public void cref_to_normal_property_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var property = crefOverlay.GetMemberDefinition("TestLibrary1.Class1.SomeProperty") as PropertyDefinition;
			Assert.IsNotNull(property);
			Assert.AreEqual("SomeProperty", property.Name);
		}

		[Test]
		public void cref_from_normal_property() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var property = type.Properties.First(x => x.Name == "SomeProperty");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("P:TestLibrary1.Class1.SomeProperty", crefOverlay.GetCref(property));
		}

		[Test]
		public void cref_to_normal_field() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var field = crefOverlay.GetMemberDefinition("F:TestLibrary1.Class1.SomeField") as FieldDefinition;
			Assert.IsNotNull(field);
			Assert.AreEqual("SomeField", field.Name);
		}

		[Test]
		public void cref_to_normal_field_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var field = crefOverlay.GetMemberDefinition("TestLibrary1.Class1.SomeField") as FieldDefinition;
			Assert.IsNotNull(field);
			Assert.AreEqual("SomeField", field.Name);
		}

		[Test]
		public void cref_from_normal_field() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var field = type.Fields.First(x => x.Name == "SomeField");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("F:TestLibrary1.Class1.SomeField", crefOverlay.GetCref(field));
		}

		[Test]
		public void cref_to_normal_const() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var field = crefOverlay.GetMemberDefinition("F:TestLibrary1.Class1.MyConst") as FieldDefinition;
			Assert.IsNotNull(field);
			Assert.AreEqual("MyConst", field.Name);
			Assert.IsNotNull(field.Constant);
		}

		[Test]
		public void cref_from_normal_const() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var field = type.Fields.First(x => x.Name == "MyConst");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("F:TestLibrary1.Class1.MyConst", crefOverlay.GetCref(field));
		}

		[Test]
		public void cref_to_normal_delegate() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var del = crefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.MyFunc") as TypeDefinition;
			Assert.IsNotNull(del);
			Assert.AreEqual("MyFunc", del.Name);
		}

		[Test]
		public void cref_from_normal_delegate() {
			var assembly = GetAssembly();
			var del = GetType(assembly, "Class1").NestedTypes.First(x => x.Name == "MyFunc");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:TestLibrary1.Class1.MyFunc", crefOverlay.GetCref(del));
		}

		[Test]
		public void cref_to_normal_event() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var evt = crefOverlay.GetMemberDefinition("E:TestLibrary1.Class1.DoStuff") as EventDefinition;
			Assert.IsNotNull(evt);
			Assert.AreEqual("DoStuff", evt.Name);
			Assert.AreEqual("MyFunc", evt.EventType.Name);
		}

		[Test]
		public void cref_to_normal_event_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var evt = crefOverlay.GetMemberDefinition("TestLibrary1.Class1.DoStuff") as EventDefinition;
			Assert.IsNotNull(evt);
			Assert.AreEqual("DoStuff", evt.Name);
			Assert.AreEqual("MyFunc", evt.EventType.Name);
		}

		[Test]
		public void cref_from_normal_event() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Events.First(x => x.Name == "DoStuff");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("E:TestLibrary1.Class1.DoStuff", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_operator() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var op = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)") as MethodDefinition;
			Assert.IsNotNull(op);
			Assert.AreEqual("op_Addition", op.Name);
			Assert.That(op.IsStatic);
			Assert.AreEqual(2, op.Parameters.Count);
			Assert.That(
				op.Parameters.Select(x => x.ParameterType.FullName),
				Has.All.EqualTo("TestLibrary1.Class1"));
		}

		[Test]
		public void cref_from_normal_operator() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Methods.First(x => x.Name.Contains("Addition"));
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_indexer() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var indexer = crefOverlay.GetMemberDefinition("P:TestLibrary1.Class1.Item(System.Int32)") as PropertyDefinition;
			Assert.IsNotNull(indexer);
			Assert.AreEqual("Item", indexer.Name);
			Assert.AreEqual(1, indexer.Parameters.Count);
		}

		[Test]
		public void cref_from_normal_indexer() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			var property = type.Properties.First(x => x.Name == "Item");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("P:TestLibrary1.Class1.Item(System.Int32)", crefOverlay.GetCref(property));
		}

		[Test]
		public void cref_to_normal_static_constructor_no_params() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var ctor = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#cctor") as MethodDefinition;
			Assert.IsNotNull(ctor);
			Assert.IsTrue(ctor.IsStatic);
			Assert.IsFalse(ctor.HasParameters);

		}

		[Test]
		public void cref_from_normal_static_constructor_no_params() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Methods.First(x => x.IsConstructor && x.Parameters.Count == 0);
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.#cctor", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_constructor_one_param() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var ctor = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String)") as MethodDefinition;
			Assert.IsNotNull(ctor);
			Assert.IsFalse(ctor.IsStatic);
			Assert.IsTrue(ctor.HasParameters);
			Assert.AreEqual(1, ctor.Parameters.Count);
			Assert.AreEqual("System.String", ctor.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void cref_from_normal_constructor_one_param() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Methods.First(x => x.IsConstructor && x.Parameters.Count == 1);
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String)", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_constructor_two_param() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var ctor = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.#ctor(System.String,System.String)") as MethodDefinition;
			Assert.IsNotNull(ctor);
			Assert.IsFalse(ctor.IsStatic);
			Assert.IsTrue(ctor.HasParameters);
			Assert.AreEqual(2, ctor.Parameters.Count);
			Assert.That(ctor.Parameters.Select(x => x.ParameterType.FullName), Has.All.EqualTo("System.String"));
		}

		[Test]
		public void cref_from_normal_constructor_two_param() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Methods.First(x => x.IsConstructor && x.Parameters.Count == 2);
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.#ctor(System.String,System.String)", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_finalizer() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var finalizer = crefOverlay.GetMemberDefinition("M:TestLibrary1.Class1.Finalize") as MethodDefinition;
			Assert.IsNotNull(finalizer);
			Assert.AreEqual("Finalize", finalizer.Name);
			Assert.IsFalse(finalizer.IsStatic);
			Assert.IsFalse(finalizer.HasParameters);
		}

		[Test]
		public void cref_from_normal_finalizer() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Class1").Methods.First(x => x.Name == "Finalize");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Class1.Finalize", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_normal_inner() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var inner = crefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			Assert.IsNotNull(inner);
			Assert.AreEqual("Inner", inner.Name);
			Assert.IsTrue(inner.IsNested);
		}

		[Test]
		public void cref_from_normal_inner() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			type = type.NestedTypes.First(x => x.Name == "Inner");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:TestLibrary1.Class1.Inner", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_normal_inner_property() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetTypeDefinition("T:TestLibrary1.Class1.Inner");
			Assert.IsNotNull(type);
			Assert.AreEqual("Inner", type.Name);
		}

		[Test]
		public void cref_from_normal_inner_property() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Class1");
			type = type.NestedTypes.First(x => x.Name == "Inner");
			var prop = type.Properties.First(x => x.Name == "Name");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("P:TestLibrary1.Class1.Inner.Name", crefOverlay.GetCref(prop));
		}

		[Test]
		public void cref_to_global_namespace_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetTypeDefinition("T:InGlobal");
			Assert.IsNotNull(type);
			Assert.AreEqual("InGlobal", type.Name);
		}

		[Test]
		public void cref_to_global_namespace_guess_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetMemberDefinition("InGlobal");
			Assert.IsNotNull(type);
			Assert.AreEqual("InGlobal", type.Name);
		}

		[Test]
		public void cref_from_global_namespace_type() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "InGlobal");
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:InGlobal", crefOverlay.GetCref(type));
		}

		[Test]
		public void invalid_double_dot_cref_to_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetMemberDefinition("TestLibrary1..Class1");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_trailing_dot_cref_to_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetMemberDefinition("TestLibrary1.Class1.");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_double_dot_namespace_cref_to_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetMemberDefinition("TestLibrary1..Seal.NotSealed");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_empty_cref_to_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			var type = crefOverlay.GetTypeDefinition("T:");
			Assert.IsNull(type);
		}

		[Test]
		public void invalid_cref_type_to_type() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.Throws<NotSupportedException>(() => crefOverlay.GetTypeDefinition("Z:TestLibrary1.Class1"));
		}

		[Test]
		public void invalid_cref_type_to_member() {
			var assembly = GetAssembly();
			var crefOverlay = new CRefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.Throws<NotSupportedException>(() => crefOverlay.GetMemberDefinition("Z:TestLibrary1.Class1.DoubleStatic(System.Int32)"));
		}

	}
}
