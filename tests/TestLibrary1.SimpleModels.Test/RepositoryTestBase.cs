using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc;
using DandyDoc.SimpleModels;
using DandyDoc.SimpleModels.Contracts;
using NUnit.Framework;

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

		protected ISimpleModel GetModelFromCref(string cref){
			Contract.Requires(!String.IsNullOrEmpty(cref));
			var result = Repository.GetModelFromCref(cref);
			Assert.IsNotNull(result);
			return result;
		}

		protected ITypeSimpleModel GetTypeModelFromCref(string cref){
			Contract.Requires(!String.IsNullOrEmpty(cref));
			var result = GetModelFromCref(cref) as ITypeSimpleModel;
			Assert.IsNotNull(result);
			return result;
		}

	}
}
