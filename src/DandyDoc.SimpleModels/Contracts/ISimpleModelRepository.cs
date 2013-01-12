using System.Collections.Generic;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModelRepository
	{

		IList<INamespaceSimpleModel> Namespaces { get; }

		IList<IAssemblySimpleModel> Assemblies { get; }

		ISimpleModel GetModelFromCref(string cref);

	}
}
