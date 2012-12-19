using System;
using System.Linq;
using DandyDoc.Core;
using NUnit.Framework;

namespace SimpleApp1.Test
{
	[TestFixture]
	public class SimpleApp1CoreTests
	{

		private AssemblyRecord GetSimpleApp1AssemblyTarget() {
			var result = AssemblyRecord.CreateFromFilePath("./samples/SimpleApp1.exe");
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.CoreAssemblyFilePath);
			Assert.That(result.CoreAssemblyFilePath.Exists);
			return result;
		}

		private TypeRecord GetThing1() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			var record = assemblyTarget.TypeRecords.First(x => x.Name == "Thing1");
			Assert.IsNotNull(record);
			return record;
		}

		[Test]
		public void spew_all_types() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			Assert.Greater(assemblyTarget.TypeRecords.Count(), 0);
		}

		[Test]
		public void check_summary_for_Thing1() {
			var record = GetThing1();
			Assert.IsNotNullOrEmpty(record.Summary.RawText);
			Assert.AreEqual("This is just a thing. Here is some garbage: &amp; &lt; .", record.Summary.RawText);
		}

		[Test]
		public void check_remarks_for_Thing1() {
			var record = GetThing1();
			var remarks = record.Remarks;
			Assert.IsNotNullOrEmpty(remarks.RawText);
			Assert.AreEqual(
				"    This\n   is\n  a\n spacing\nsample!",
				remarks.RawText);
		}

		[Test]
		public void see_also_for_Thing1() {
			var record = GetThing1();
			Assert.IsNotNull(record.SeeAlso);
			Assert.AreEqual(2, record.SeeAlso.Count);
			Assert.AreEqual("The other thing.", record.SeeAlso[0].Description.RawText);
		}

		[Test]
		public void method_summaries_for_Thing1() {
			var record = GetThing1();
			var methods = record.Members.Where(x => x.IsMethod).ToList();
			Assert.Greater(methods.Count, 0);
			Assert.That(methods.Any(x => !String.IsNullOrWhiteSpace(x.Summary.RawText)));
		}

		[Test]
		public void parameter_summaries_for_Thing1_DoSomething() {
			var record = GetThing1();
			var method = record.Members.First(x => x.IsMethod && x.Name == "DoSomething");
			var param = method.Parameters.Single();
			Assert.IsNotNull(param);
			Assert.IsNotNullOrEmpty(param.Summary.RawText);
		}

	}
}
