using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Overlays.MsdnLinks;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class MsdnDynamicLinkOverlayTests
	{

		public MsdnDynamicLinkOverlay LinkGenerator { get; private set; }

		[SetUp]
		public void SetUp(){
			LinkGenerator = new MsdnDynamicLinkOverlay();
		}

		[TearDown]
		public void TearDown(){
			LinkGenerator = null;
		}

		[Test]
		public void find_system_int32(){
			var result = LinkGenerator.Search("System.Int32").Single();
			Assert.AreEqual("system.int32", result.Alias);
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_system_int32_maxValue() {
			var result = LinkGenerator.Search("System.Int32.MaxValue").Single();
			Assert.AreEqual("system.int32.maxvalue", result.Alias);
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_system_int32_parse() {
			var result = LinkGenerator.Search("System.Int32.Parse(System.String)").Single();
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_system_collections_generic_list1() {
			var result = LinkGenerator.Search("System.Collections.Generic.List`1").Single();
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_system_collections_generic_list1_count() {
			var result = LinkGenerator.Search("System.Collections.Generic.List`1.Count").Single();
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_system_collections_generic_list1_add() {
			var result = LinkGenerator.Search("System.Collections.Generic.List`1.Add(`0)").Single();
			Assert.IsNotNullOrEmpty(result.ContentId);
		}

		[Test]
		public void find_a_bunch_of_stuff(){
			var searches = new[]{
				"System.Int32",
				"System.Int32.MaxValue",
				"System.Int32.Parse(System.String)",
				"System.Collections.Generic.List`1",
				"System.Collections.Generic.List`1.Count",
				"System.Collections.Generic.List`1.Add(`0)"
			};
			foreach (var search in searches){
				var results = LinkGenerator.Search(search).ToList();
				Assert.AreEqual(1, results.Count);
				Assert.IsNotNull(results[0].ContentId);
			}
	}


	}
}
