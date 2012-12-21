using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Core;
using NUnit.Framework;

namespace SimpleApp1.Test
{
	[TestFixture]
	public class GenericTests
	{

		private AssemblyRecord GetSimpleApp1AssemblyTarget() {
			var result = AssemblyRecord.CreateFromFilePath("./samples/SimpleApp1.exe");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.CoreAssemblyFilePath);
			Assert.That(result.CoreAssemblyFilePath.Exists);
			return result;
		}

		private TypeRecord GetGeneric1() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			var record = assemblyTarget.TypeRecords.First(x => x.Name == "Generic1`2");
			Assert.IsNotNull(record);
			return record;
		}

		private TypeRecord GetGeneric1Inner() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			var record = assemblyTarget.TypeRecords.First(x => x is NestedTypeRecord && x.Name == "Inner`1");
			Assert.IsNotNull(record);
			return record;
		}

		[Test]
		public void TestGeneric1Cref() {
			var generic1 = GetGeneric1();
			Assert.AreEqual("SimpleApp1.Generic1`2", generic1.Cref);
		}

		[Test]
		public void TestGeneric1InnerCref() {
			var generic1Inner = GetGeneric1Inner();
			Assert.AreEqual("SimpleApp1.Generic1`2.Inner`1", generic1Inner.Cref);
		}

		[Test]
		public void TestGeneric1Junk1Cref() {
			var generic1 = GetGeneric1();
			var method = generic1.Methods.First(x => x.Name.StartsWith("Junk1"));
			Assert.AreEqual("SimpleApp1.Generic1`2.Junk1``1(``0)", method.Cref);
		}

		[Test]
		public void TestGeneric1InnerJunk2Cref() {
			var generic1Inner = GetGeneric1Inner();
			var method = generic1Inner.Methods.First(x => x.Name.StartsWith("Junk2"));
			Assert.AreEqual("SimpleApp1.Generic1`2.Inner`1.Junk2``1(``0)", method.Cref);
		}

		[Test]
		public void TestGeneric1InnerJunk3Cref() {
			var generic1Inner = GetGeneric1Inner();
			var method = generic1Inner.Methods.First(x => x.Name.StartsWith("Junk3"));
			Assert.AreEqual("SimpleApp1.Generic1`2.Inner`1.Junk3``1(`2,`1,`0,``0)", method.Cref);
		}

	}
}
