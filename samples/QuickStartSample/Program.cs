using System;
using System.Linq;
using DandyDoc;
using DandyDoc.SimpleModels;

namespace QuickStartSample
{
	class Program
	{
		static void Main(string[] args) {

			// first you need some assemblies
			var assemblies = new AssemblyDefinitionCollection("QuickStartSample.exe");

			// NOTE: due to threading issues you may want to use immediate mode:
			// new AssemblyDefinitionCollection(true, "QuickStartSample.exe");

			// Next you can create the models. The SimpleModels assembly has a repository class you can use.
			var repository = new SimpleModelRepository(assemblies);

			// A good place to start is with namespaces.
			foreach (var namespaceModel in repository.Namespaces){
				Console.WriteLine("NAMESPACE: {0}", namespaceModel.Title);

				// You can quickly get the types (and delegates) within the namespace.
				foreach (var types in namespaceModel.Types){

					// As an example we can get the flair tags associated with a type.
					var flairTags = String.Join(",", types.FlairTags.Select(x => x.IconId));

					Console.WriteLine("\t{0}: {1}", types.Title, flairTags);
				}
			}

		}
	}
}
