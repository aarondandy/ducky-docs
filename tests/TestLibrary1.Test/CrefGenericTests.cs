using System.Linq;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class CrefGenericTests
	{

		private AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		private TypeDefinition GetType(AssemblyDefinition assemblyDefinition, string typeName) {
			if (null == assemblyDefinition)
				assemblyDefinition = GetAssembly();
			var type = assemblyDefinition.Modules.SelectMany(x => x.Types).FirstOrDefault(t => t.Name == typeName);
			Assert.IsNotNull(type);
			return type;
		}

		[Test]
		public void cref_to_generic_class(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection{assembly});
			var type = crefOverlay.GetTypeDefinition("T:TestLibrary1.Generic1`2");
			Assert.IsNotNull(type);
			Assert.AreEqual("Generic1`2", type.Name);
		}

		[Test]
		public void cref_from_generic_class(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:TestLibrary1.Generic1`2", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_generic_method_one_param() {
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var method = crefOverlay.GetMemberDefinition("M:TestLibrary1.Generic1`2.Junk1``1(``0)") as MethodDefinition;
			Assert.IsNotNull(method);
			Assert.AreEqual("Junk1", method.Name);
			Assert.AreEqual(1, method.Parameters.Count);
			Assert.IsTrue(method.Parameters[0].ParameterType.IsGenericParameter);
		}

		[Test]
		public void cref_from_generic_method_one_param() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2");
			var method = type.Methods.First(x => x.Name == "Junk1");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Generic1`2.Junk1``1(``0)", crefOverlay.GetCref(method));
		}

		[Test]
		public void cref_to_generic_property(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var prop = crefOverlay.GetMemberDefinition("P:TestLibrary1.Generic1`2.A") as PropertyDefinition;
			Assert.IsNotNull(prop);
			Assert.AreEqual("A", prop.Name);
		}

		[Test]
		public void cref_from_generic_property(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2");
			var prop = type.Properties.First(x => x.Name == "A");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("P:TestLibrary1.Generic1`2.A", crefOverlay.GetCref(prop));
		}

		[Test]
		public void cref_to_generic_field(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var f = crefOverlay.GetMemberDefinition("F:TestLibrary1.Generic1`2.B") as FieldDefinition;
			Assert.IsNotNull(f);
			Assert.AreEqual("B", f.Name);
		}

		[Test]
		public void cref_from_generic_field() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2");
			var prop = type.Fields.First(x => x.Name == "B");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("F:TestLibrary1.Generic1`2.B", crefOverlay.GetCref(prop));
		}

		[Test]
		public void cref_to_delegate_with_generics_from_parent() {
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var t = crefOverlay.GetTypeDefinition("T:TestLibrary1.Generic1`2.MyFunc") as TypeDefinition;
			Assert.IsNotNull(t);
			Assert.AreEqual("MyFunc", t.Name);
		}

		[Test]
		public void cref_from_delegate_with_generics_from_parent(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2").NestedTypes.First(x => x.Name == "MyFunc");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:TestLibrary1.Generic1`2.MyFunc", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_delegate_with_own_generic() {
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var t = crefOverlay.GetTypeDefinition("T:TestLibrary1.Generic1`2.MyFunc`1") as TypeDefinition;
			Assert.IsNotNull(t);
			Assert.AreEqual("MyFunc`1", t.Name);
			Assert.IsTrue(t.HasGenericParameters);
			Assert.AreEqual(3, t.GenericParameters.Count);
		}

		[Test]
		public void cref_from_delegate_with_own_generic() {
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2").NestedTypes.First(x => x.Name == "MyFunc`1");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("T:TestLibrary1.Generic1`2.MyFunc`1", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_generic_event(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var e = crefOverlay.GetMemberDefinition("E:TestLibrary1.Generic1`2.E") as EventDefinition;
			Assert.IsNotNull(e);
			Assert.AreEqual("E", e.Name);
		}

		[Test]
		public void cref_from_generic_event(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2").Events.First(x => x.Name == "E");
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("E:TestLibrary1.Generic1`2.E", crefOverlay.GetCref(type));
		}

		[Test]
		public void cref_to_generic_operator() {
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var op = crefOverlay.GetMemberDefinition("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})") as MethodDefinition;
			Assert.IsNotNull(op);
			Assert.AreEqual("op_Addition", op.Name);
			Assert.That(op.IsStatic);
			Assert.AreEqual(2, op.Parameters.Count);
			Assert.That(
				op.Parameters.Select(x => x.ParameterType.FullName),
				Has.All.StartsWith("TestLibrary1.Generic1"));
		}

		[Test]
		public void cref_from_generic_operator() {
			var assembly = GetAssembly();
			var ev = GetType(assembly, "Generic1`2").Methods.First(x => x.Name.Contains("Addition"));
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})", crefOverlay.GetCref(ev));
		}

		[Test]
		public void cref_to_generic_inner_mixed_params(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var m = crefOverlay.GetMemberDefinition("M:TestLibrary1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)") as MethodDefinition;
			Assert.IsNotNull(m);
			Assert.That(m.Name.StartsWith("Junk3"));
			Assert.AreEqual(4,m.Parameters.Count);
		}

		[Test]
		public void cref_from_generic_inner_mixed_params(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2").NestedTypes.First(x => x.Name == "Inner`1");
			var method = type.Methods.First(x => x.Name.StartsWith("Junk3"));
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)", crefOverlay.GetCref(method));
		}

		[Test]
		public void cref_to_generic_inner_method_with_only_class_generics(){
			var assembly = GetAssembly();
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			var m = crefOverlay.GetMemberDefinition("M:TestLibrary1.Generic1`2.Inner`1.Junk4(`2,`1,`0)") as MethodDefinition;
			Assert.IsNotNull(m);
			Assert.That(m.Name.StartsWith("Junk4"));
			Assert.AreEqual(3, m.Parameters.Count);
		}

		[Test]
		public void cref_from_generic_inner_method_with_only_class_generics(){
			var assembly = GetAssembly();
			var type = GetType(assembly, "Generic1`2").NestedTypes.First(x => x.Name == "Inner`1");
			var method = type.Methods.First(x => x.Name.StartsWith("Junk4"));
			var crefOverlay = new CrefOverlay(new AssemblyDefinitionCollection { assembly });
			Assert.AreEqual("M:TestLibrary1.Generic1`2.Inner`1.Junk4(`2,`1,`0)", crefOverlay.GetCref(method));
		}

	}
}
