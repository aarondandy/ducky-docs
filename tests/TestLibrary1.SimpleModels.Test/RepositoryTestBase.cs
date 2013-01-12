using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc;
using DandyDoc.SimpleModels;

namespace TestLibrary1.SimpleModels.Test
{
	public abstract class RepositoryTestBase
	{

		public AssemblyDefinitionCollection Assemblies { get; private set; }

		public SimpleModelRepository Repository { get; private set; }

		public ReadOnlyCollection<string> AssemblyFilePaths { get; private set; }

		protected RepositoryTestBase()
			: this("./TestLibrary1.dll","./DandyDoc.Core.dll","./DandyDoc.SimpleModels.dll") { }

		protected RepositoryTestBase(params string[] assemblyFilePaths) {
			Contract.Requires(assemblyFilePaths != null);
			AssemblyFilePaths = Array.AsReadOnly(assemblyFilePaths);
			Assemblies = new AssemblyDefinitionCollection(assemblyFilePaths);
			Repository = new SimpleModelRepository(Assemblies);
		}

	}
}
