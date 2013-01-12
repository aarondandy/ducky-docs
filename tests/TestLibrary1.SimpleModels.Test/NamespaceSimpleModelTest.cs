using System.Linq;
using NUnit.Framework;

namespace TestLibrary1.SimpleModels.Test
{
	public class NamespaceSimpleModelTest : RepositoryTestBase
	{

		[Test]
		public void has_test_lib_namespace(){
			Assert.That(Repository.Namespaces.Select(n => n.NamespaceName), Has.Some.EqualTo("TestLibrary1"));
		}

		[Test]
		public void has_multiple_namespaces(){
			Assert.Greater(Repository.Namespaces.Count, 0);
		}

	}
}
