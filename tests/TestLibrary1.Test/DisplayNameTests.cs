using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.DisplayName;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class DisplayNameTests
	{

		private AssemblyDefinition GetAssembly() {
			var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
			Assert.IsNotNull(assemblyDefinition);
			return assemblyDefinition;
		}

		public DisplayNameTests() {
			AssemblyDefinitionCollection = new AssemblyDefinitionCollection{GetAssembly()};
			CrefOverlay = new CrefOverlay(AssemblyDefinitionCollection);
			Default = new DisplayNameOverlay();
			Full = new DisplayNameOverlay(){
				IncludeNamespaceForTypes = true,
				ShowGenericParametersOnDefinition = true,
				ShowTypeNameForMembers = true
			};
			Full.ParameterTypeDisplayNameOverlay = Full;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public DisplayNameOverlay Default { get; private set; }

		public DisplayNameOverlay Full { get; private set; }

		public TypeDefinition GetType(string cref){
			return CrefOverlay.GetTypeDefinition(cref);
		}

		public IMemberDefinition GetMember(string cref){
			return CrefOverlay.GetMemberDefinition(cref);
		}

		[Test]
		public void normal_type(){
			Assert.AreEqual("Class1", Default.GetDisplayName(GetType("T:TestLibrary1.Class1")));
			Assert.AreEqual("TestLibrary1.Class1", Full.GetDisplayName(GetType("T:TestLibrary1.Class1")));
		}

		[Test]
		public void normal_nested_type() {
			Assert.AreEqual("Inner", Default.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
			Assert.AreEqual("TestLibrary1.Class1.Inner", Full.GetDisplayName(GetType("T:TestLibrary1.Class1.Inner")));
		}

		[Test]
		public void generic_type(){
			Assert.AreEqual("Generic1<TA, TB>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2")));
		}

		[Test]
		public void generic_nested_type() {
			Assert.AreEqual("Inner<TC>", Default.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.Inner<TC>", Full.GetDisplayName(GetType("T:TestLibrary1.Generic1`2.Inner`1")));
		}

		[Test]
		public void normal_constructor(){
			Assert.AreEqual("Class1(String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
			Assert.AreEqual("TestLibrary1.Class1.Class1(System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.#ctor(System.String)")));
		}

		[Test]
		public void generic_constructor() {
			Assert.AreEqual("Generic1(TA, TB, IEnumerable<TA>, String)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.Generic1(TA, TB, System.Collections.Generic.IEnumerable<TA>, System.String)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.#ctor(`0,`1,System.Collections.Generic.IEnumerable{`0},System.String)")));
		}

		[Test]
		public void normal_method(){
			Assert.AreEqual("DoubleStatic(Int32)", Default.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
			Assert.AreEqual("TestLibrary1.Class1.DoubleStatic(System.Int32)", Full.GetDisplayName(GetMember("M:TestLibrary1.Class1.DoubleStatic(System.Int32)")));
		}

		[Test]
		public void generic_method(){
			Assert.AreEqual("AMix<TOther>(TA, TOther)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
			Assert.AreEqual("TestLibrary1.Generic1<TA, TB>.AMix<TOther>(TA, TOther)", Full.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.AMix``1(`0,``0)")));
		}

		[Test]
		public void generic_operator(){
			Assert.AreEqual("operator+(Generic1<Int32, Int32[]>, Generic1<TA, TB>)", Default.GetDisplayName(GetMember("M:TestLibrary1.Generic1`2.op_Addition(TestLibrary1.Generic1{System.Int32,System.Int32[]},TestLibrary1.Generic1{`0,`1})")));
		}

	}
}
