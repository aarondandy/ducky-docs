using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IParameterSimpleModel
	{

		IInvokableSimpleModel Parent { get; }

		string Name { get; }

		IComplexTextNode DisplayName { get; }

		bool HasSummary { get; }

		IComplexTextNode Summary { get; }

		ISimpleMemberPointerModel Type { get; }

		bool HasFlair { get; }

		IList<IFlairTag> Flair { get; }

		bool HasAttributeMatchingName(string name);

	}
}
