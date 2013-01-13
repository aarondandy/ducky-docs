using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TestLibrary1.SimpleModels.Test
{
	[TestFixture]
	public class AssemblySimpleModelTest : RepositoryTestBase
	{

		[Test]
		public void can_get_all_assemblies() {
			var result = Repository.Assemblies;
			Assert.IsNotNull(result);
			Assert.AreEqual(AssemblyFilePaths.Count, result.Count);
			Assert.That(result, Has.None.Null);
		}

		[Test]
		public void check_assembly_file_name() {
			var testAssembly = Repository.Assemblies.Single(x => x.ShortName.StartsWith("TestLibrary1"));
			Assert.AreEqual("TestLibrary1.dll", testAssembly.AssemblyFileName);
		}

		[Test]
		public void check_assembly_display_name() {
			var testAssembly = Repository.Assemblies.Single(x => x.ShortName.StartsWith("TestLibrary1"));
			Assert.AreEqual("TestLibrary1", testAssembly.ShortName);
		}

		[Test]
		public void check_assembly_full_name() {
			var testAssembly = Repository.Assemblies.Single(x => x.ShortName.StartsWith("TestLibrary1"));
			var matchRegex = new Regex(@"^TestLibrary1, Version=[\d\.]+, Culture=neutral, PublicKeyToken=null$");
			Assert.That(matchRegex.IsMatch(testAssembly.FullName), "FullName is: " + testAssembly.FullName);
		}

		[Test]
		public void check_assembly_cref() {
			var testAssembly = Repository.Assemblies.Single(x => x.ShortName.StartsWith("TestLibrary1"));
			var matchRegex = new Regex(@"^A\:TestLibrary1, Version=[\d\.]+, Culture=neutral, PublicKeyToken=null$");
			Assert.That(matchRegex.IsMatch(testAssembly.CRef), "FullName is: " + testAssembly.CRef);
		}

	}
}
