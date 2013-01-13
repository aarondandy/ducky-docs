using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModel
	{

		string Title { get; }

		string SubTitle { get; }

		string ShortName { get; }

		string FullName { get; }

		string CRef { get; }

		string NamespaceName { get; }

		IAssemblySimpleModel ContainingAssembly { get; }

		ISimpleModelRepository RootRepository { get; }

		bool HasFlair { get; }

		IList<IFlairTag> FlairTags { get; }

		bool HasSummary { get; }

		IComplexTextNode Summary { get; }

		bool HasRemarks { get; }

		IList<IComplexTextNode> Remarks { get; }

		bool HasExamples { get; }

		IList<IComplexTextNode> Examples { get; }

		bool HasSeeAlso { get; }

		IList<IComplexTextNode> SeeAlso { get; }

	}
}
