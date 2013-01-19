using System.Collections.Generic;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModelRepository : ISimpleModel
	{

		IList<INamespaceSimpleModel> Namespaces { get; }

		IList<IAssemblySimpleModel> Assemblies { get; }

		ISimpleModel GetModelFromCref(string cref);

		XmlDocOverlay XmlDocOverlay { get; }

		CRefOverlay CRefOverlay { get; }

	}
}
