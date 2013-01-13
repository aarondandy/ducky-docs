using System.Collections.Generic;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IAssemblySimpleModel : ISimpleModel
	{
		string AssemblyFileName { get; }

		IList<ITypeSimpleModel> AllTypes { get; }

		IList<ITypeSimpleModel> RootTypes { get; }

		CrefOverlay CrefOverlay { get; }

		XmlDocOverlay XmlDocOverlay { get; }

		ISimpleModel GetModelFromCref(string cref);

		ISimpleModelMembersCollection GetMembers(ITypeSimpleModel model);

	}
}
