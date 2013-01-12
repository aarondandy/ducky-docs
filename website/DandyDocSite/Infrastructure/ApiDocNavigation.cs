using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels;
using DandyDoc;

namespace DandyDocSite.Infrastructure
{
	public class ApiDocNavigation
	{

		public ApiDocNavigation(AssemblyDefinitionCollection assemblyDefinitionCollection){
			if (null == assemblyDefinitionCollection) throw new ArgumentNullException("assemblyDefinitionCollection");
			Contract.EndContractBlock();
			NavigationRepository = new SimpleModelRepository(assemblyDefinitionCollection);

			// hack to read all required data until cecil is thread safe
			var junk = NavigationRepository.Namespaces.Count;
		}

		public SimpleModelRepository NavigationRepository { get; private set; }

	}
}