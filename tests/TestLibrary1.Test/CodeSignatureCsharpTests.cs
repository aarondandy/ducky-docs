using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc;
using DandyDoc.Overlays.CodeSignature;
using DandyDoc.Overlays.Cref;
using Mono.Cecil;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class CodeSignatureCsharpTests
	{

		private static AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		public CodeSignatureCsharpTests() {
			AssemblyDefinitionCollection = new AssemblyDefinitionCollection{GetAssembly()};
			CrefOverlay = new CrefOverlay(AssemblyDefinitionCollection);
			Generator = new CodeSignatureGeneratorCSharp();
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public CodeSignatureGeneratorCSharp Generator { get; private set; }

		public IMemberDefinition GetMemberDefinition(string cref) {
			return CrefOverlay.GetMemberDefinition(cref);
		}

		public MethodDefinition GetMethod(string cref) {
			return (MethodDefinition)GetMemberDefinition(cref);
		}

		public TypeDefinition GetType(string cref) {
			return CrefOverlay.GetTypeDefinition(cref);
		}

		public PropertyDefinition GetProperty(string cref) {
			return (PropertyDefinition)GetMemberDefinition(cref);
		}

		public FieldDefinition GetField(string cref) {
			return (FieldDefinition)GetMemberDefinition(cref);
		}

		public EventDefinition GetEvent(string cref) {
			return (EventDefinition)GetMemberDefinition(cref);
		}

		[Test]
		public void csharp_type() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.Class1"));
			Assert.AreEqual("public class Class1 : Object", result.Code);
		}

		[Test]
		public void csharp_inner_type() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.Class1.NoRemarks"));
			Assert.AreEqual("public class NoRemarks : NoDocs", result.Code);
		}

		[Test]
		public void csharp_protected_struct() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.Class1.ProtectedStruct"));
			Assert.AreEqual("protected struct ProtectedStruct : IThing", result.Code);
		}

		[Test]
		public void csharp_interface() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.Class1.IThing"));
			Assert.AreEqual("public interface IThing", result.Code);
		}

		[Test]
		public void csharp_enum() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.FlagsEnum"));
			Assert.AreEqual("public enum FlagsEnum", result.Code);
		}

		[Test]
		public void csharp_delegate() {
			var result = Generator.GenerateSignature(GetType("T:TestLibrary1.Class1.MyFunc"));
			Assert.AreEqual("public delegate Int32 MyFunc(Int32 a, Int32 b)", result.Code);
		}

		[Test]
		public void csharp_constructor() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Class1.#ctor(System.String)"));
			Assert.AreEqual("public Class1(String crap)", result.Code);
		}

		[Test]
		public void csharp_static_constructor() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Class1.#cctor()"));
			Assert.AreEqual("private static Class1()", result.Code);
		}

		[Test]
		public void csharp_static_method() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Class1.DoubleStatic(System.Double)"));
			Assert.AreEqual("public static Double DoubleStatic(Double n)", result.Code);
		}

		[Test]
		public void csharp_instance_method() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Class1.VirtualInstanceMethod(System.String)"));
			Assert.AreEqual("public virtual String VirtualInstanceMethod(String stuff)", result.Code);
		}

		[Test]
		public void csharp_operator() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Class1.op_Addition(TestLibrary1.Class1,TestLibrary1.Class1)"));
			Assert.AreEqual("public static Class1 operator +(Class1 a, Class1 b)", result.Code);
		}

		[Test]
		public void csharp_instance_property_get_set() {
			var result = Generator.GenerateSignature(GetProperty("P:TestLibrary1.Class1.HasTableInRemarks"));
			Assert.AreEqual("public Boolean HasTableInRemarks { private get; set; }", result.Code);
		}

		[Test]
		public void csharp_instance_property_get_indexer() {
			var result = Generator.GenerateSignature(GetProperty("P:TestLibrary1.Class1.Item(System.Int32)"));
			Assert.AreEqual("public Int32 this[Int32 n] { get; }", result.Code);
		}

		[Test]
		public void csharp_instance_field() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.Class1.SomeField"));
			Assert.AreEqual("public static Double SomeField", result.Code);
		}

		[Test]
		public void csharp_const() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.Class1.MyConst"));
			Assert.AreEqual("public const Int32 MyConst", result.Code);
		}

		[Test]
		public void csharp_readonly_field() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.Class1.ReadonlyField"));
			Assert.AreEqual("protected readonly Int32 ReadonlyField", result.Code);
		}

		[Test]
		public void csharp_instance_event() {
			var result = Generator.GenerateSignature(GetEvent("E:TestLibrary1.Class1.DoStuffInstance"));
			Assert.AreEqual("protected event MyFunc DoStuffInstance", result.Code);
		}

		[Test]
		public void csharp_static_event() {
			var result = Generator.GenerateSignature(GetEvent("E:TestLibrary1.Class1.DoStuff"));
			Assert.AreEqual("public static event MyFunc DoStuff", result.Code);
		}

		[Test]
		public void csharp_implicit_impl() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Seal.NotSealed.Stuff(System.Int32)"));
			Assert.AreEqual("public Int32 Stuff(Int32 a)", result.Code);
		}

		[Test]
		public void csharp_abstract() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Seal.BaseClassToSeal.SealMe(System.Int32)"));
			Assert.AreEqual("public abstract Int32 SealMe(Int32 a)", result.Code);
		}

		[Test]
		public void csharp_abstract_override() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Seal.KickSealingCan.SealMe(System.Int32)"));
			Assert.AreEqual("public abstract override Int32 SealMe(Int32 a)", result.Code);
		}

		[Test]
		public void csharp_override_prop() {
			var result = Generator.GenerateSignature(GetProperty("P:TestLibrary1.Seal.KickSealingCan.Prop"));
			Assert.AreEqual("public override String Prop { get; set; }", result.Code);
		}

		[Test]
		public void csharp_sealed_method() {
			var result = Generator.GenerateSignature(GetMethod("M:TestLibrary1.Seal.SealIt.SealMe(System.Int32)"));
			Assert.AreEqual("public sealed override Int32 SealMe(Int32 a)", result.Code);
		}

		[Test]
		public void csharp_sealed_prop() {
			var result = Generator.GenerateSignature(GetProperty("P:TestLibrary1.Seal.SealIt.Prop"));
			Assert.AreEqual("public sealed override String Prop { get; set; }", result.Code);
		}

		[Test]
		public void csharp_private_field() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.PublicExposedTestClass.PrivateField"));
			Assert.AreEqual("private Int32 PrivateField", result.Code);
		}

		[Test]
		public void csharp_internal_field() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.PublicExposedTestClass.InternalField"));
			Assert.AreEqual("internal Int32 InternalField", result.Code);
		}

		[Test]
		public void csharp_protected_internal_field() {
			var result = Generator.GenerateSignature(GetField("F:TestLibrary1.PublicExposedTestClass.ProtectedInternalField"));
			Assert.AreEqual("protected internal Int32 ProtectedInternalField", result.Code);
		}

	}
}
