using System;
using System.Collections.ObjectModel;
using DandyDoc;

namespace DandyDocSite.Infrastructure
{
	public class AssemblyCollectionGenerator
	{

		public AssemblyCollectionGenerator(params string[] assemblyPaths){
			AssemblyPaths = Array.AsReadOnly(assemblyPaths);
		}

		public ReadOnlyCollection<string> AssemblyPaths { get; private set; }

		public AssemblyDefinitionCollection GenerateDefinitions(){
			return new AssemblyDefinitionCollection(AssemblyPaths);
		}

	}
}