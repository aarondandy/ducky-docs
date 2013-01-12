using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DandyDoc.SimpleModels;

namespace TestLibrary1.SimpleModels.Test
{
	[TestFixture]
	public class TypeSimpleModelTest : RepositoryTestBase
	{

		[Test]
		public void root_class_display_name(){
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1");
			Assert.AreEqual("Class1", type.DisplayName);
		}

		[Test]
		public void root_class_full_name() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1");
			Assert.AreEqual("TestLibrary1.Class1", type.FullName);
		}

		[Test]
		public void root_class_cref() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1");
			Assert.AreEqual("T:TestLibrary1.Class1", type.CRef);
		}

		[Test]
		public void root_class_title() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1");
			Assert.AreEqual("Class1", type.Title);
		}

		[Test]
		public void class_subtitle() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1");
			Assert.AreEqual("Class", type.SubTitle);
		}

		[Test]
		public void enum_subtitle() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.FlagsEnum");
			Assert.AreEqual("Enumeration", type.SubTitle);
		}

		[Test]
		public void nested_class_display_name() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1.Inner");
			Assert.AreEqual("Class1.Inner", type.DisplayName);
		}

		[Test]
		public void nested_class_full_name() {
			var type = (TypeSimpleModel)GetTypeModelFromCref("TestLibrary1.Class1.Inner");
			Assert.AreEqual("TestLibrary1.Class1.Inner", type.FullName);
		}


	}
}
