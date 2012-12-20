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

		private TypeRecord GetThing2() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			var record = assemblyTarget.TypeRecords.First(x => x.Name == "Thing2");
			Assert.IsNotNull(record);
			return record;
		}

		[Test]
		public void spew_all_types() {
			var assemblyTarget = GetSimpleApp1AssemblyTarget();
			Assert.Greater(assemblyTarget.TypeRecords.Count(), 0);
		}

		[Test]
		public void check_raw_summary_for_Thing1() {
			var record = GetThing1();
			Assert.IsNotNullOrEmpty(record.Summary.NormalizedInnerXml);
			Assert.AreEqual("This is just a thing. Here is some garbage: &amp; &lt; . See <see cref=\"T:SimpleApp1.Thing2\">Thing #2</see> for another thing.", record.Summary.NormalizedInnerXml);
		}

		[Test]
		public void check_parsed_summary_for_Thing1(){
			var record = GetThing1();
			Assert.IsNotNullOrEmpty(record.Summary.NormalizedInnerXml);
			var parsedSummary = record.Summary.ParsedNormalized;
			Assert.IsNotNull(parsedSummary);
			Assert.Greater(parsedSummary.Count, 0);
			var seePart = parsedSummary.OfType<ParsedXmlSeePart>().FirstOrDefault();
			Assert.IsNotNull(seePart);
			var thing2 = record.Parent.TypeRecords.First(x => x.Name == "Thing2");
			Assert.IsNotNull(thing2);
			Assert.AreSame(thing2, seePart.CrefTarget);
			Assert.AreEqual("Thing #2", seePart.InnerXml);

		}

		[Test]
		public void check_remarks_for_Thing1() {
			var record = GetThing1();
			var remarks = record.Remarks;
			Assert.IsNotNullOrEmpty(remarks.Single().NormalizedInnerXml);
			Assert.AreEqual(
				"    This\n   is\n  a\n spacing\nsample!",
				remarks.Single().NormalizedInnerXml);
		}

		[Test]
		public void see_also_for_Thing1() {
			var record = GetThing1();
			Assert.IsNotNull(record.SeeAlso);
			Assert.AreEqual(2, record.SeeAlso.Count);
			Assert.AreEqual("The other thing.", record.SeeAlso[0].Description.NormalizedInnerXml);
		}

		[Test]
		public void method_summaries_for_Thing1() {
			var record = GetThing1();
			var methods = record.Members.Where(x => x.IsMethod).ToList();
			Assert.Greater(methods.Count, 0);
			Assert.That(methods.Any(x => !String.IsNullOrWhiteSpace(x.Summary.NormalizedInnerXml)));
		}

		[Test]
		public void parameter_summaries_for_Thing1_DoSomething() {
			var record = GetThing1();
			var method = record.Members.First(x => x.IsMethod && x.Name == "DoSomething");
			var param = method.Parameters.Single();
			Assert.IsNotNull(param);
			Assert.IsNotNullOrEmpty(param.Summary.NormalizedInnerXml);
		}

		[Test]
		public void nested_summary_elements_for_Thing1_DoNothing(){
			var record = GetThing1();
			var method = record.Members.First(x => x.IsMethod && x.Name == "DoNothing");
			var summary = method.Summary;
			var summaryParts = summary.ParsedNormalized;
			Assert.IsNotNull(summaryParts);
			Assert.Greater(summaryParts.Count, 0);
			var seePart = summaryParts.OfType<ParsedXmlSeePart>().First();
			var seeInnerParts = seePart.SubParts.ParsedNormalized;
			Assert.IsNotNull(seeInnerParts);
			Assert.AreEqual(1, seeInnerParts.Count);
			var seeGuts = seeInnerParts.Single() as ParsedXmlInlineCode;
			Assert.IsNotNull(seeGuts);
			Assert.AreEqual("This is <b>Shtuff</b>", seeGuts.InnerXml);
			var boldShtuff = seeGuts.SubParts.ParsedNormalized.OfType<ParsedXmlBasicElementPart>().First();
			Assert.IsNotNull(boldShtuff);
			Assert.AreEqual("<b>", boldShtuff.OuterPrefix);
			Assert.AreEqual("</b>", boldShtuff.OuterSuffix);
			Assert.AreEqual("Shtuff", boldShtuff.InnerXml);
			Assert.AreEqual("Shtuff", boldShtuff.InnerText);
			Assert.AreEqual("b", boldShtuff.Node.Name);
		}

	}
}
