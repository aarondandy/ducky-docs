using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DandyDoc;
using DandyDoc.SimpleModels;
using NUnit.Framework;

namespace TestLibrary1.SimpleModels.Test
{
	[TestFixture]
	public class ThreadProblemsTest : RepositoryTestBase
	{

		private static readonly string[] TestCRefs = new []{
			"M:TestLibrary1.Class1.DoubleStatic(System.Int32)",
			"T:TestLibrary1.Class1",
			"M:TestLibrary1.Class1.NoDocs.#ctor",
			"M:TestLibrary1.Generic1`2.Constraints`1.GetStuff``1(`2,``0)",
			"P:TestLibrary1.Class1.Item(System.Int32)",
			"T:TestLibrary1.PublicExposedTestClass.ProtectedInternalClass.ProtectedInternalDelegate"
		};

		[Test]
		public void should_never_cause_problems(){
			var assemblies = new AssemblyDefinitionCollection(base.AssemblyFilePaths);
			var simpleModelRepository = new SimpleModelRepository(assemblies);
			foreach (var cRef in TestCRefs) {
				var model = simpleModelRepository.GetModelFromCref(cRef);
				Assert.IsNotNull(model);
			}
		}

		/// <summary>
		/// This test makes sure that Mono.Cecil is NOT thread safe. The intent being that when this test fails the code can be changes to depend on Cecil being thread safe.
		/// </summary>
		[Test]
		public void cause_thread_problem(){
			var assemblies = new AssemblyDefinitionCollection(base.AssemblyFilePaths);
			var simpleModelRepository = new SimpleModelRepository(assemblies);

			var cRefs = TestCRefs.Concat(TestCRefs).Concat(TestCRefs).Concat(TestCRefs).ToArray();

			Assert.Throws(Is.InstanceOf(typeof(Exception)), () => {
				for (int i = 0; i < 10; i++) {
					var results = cRefs.AsParallel().Select(simpleModelRepository.GetModelFromCref).ToArray();
					Assert.AreEqual(cRefs.Length, results.Length);
				}
			}, "If this test fails that would mean that Mono.Cecil is now thread safe!");
		}

		[Test]
		public void thread_safe_workaround(){

			Thread.Sleep(250);

			var assemblies = new AssemblyDefinitionCollection(true, AssemblyFilePaths);
			var simpleModelRepository = new SimpleModelRepository(assemblies);

			var cRefs = TestCRefs.ToArray();

			for (int i = 0; i < 10; i++) {
				var results = cRefs.AsParallel().Select(simpleModelRepository.GetModelFromCref).ToArray();
				Assert.AreEqual(cRefs.Length, results.Length);
			}
		}

	}
}
