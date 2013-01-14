using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ITypeSimpleModel : ISimpleModel
	{

		IList<ITypeSimpleModel> NestedTypes { get; }

		IList<IDelegateSimpleModel> NestedDelegates { get; }

		bool HasBaseChain { get; }

		IList<ISimpleMemberPointerModel> BaseChain { get; }

		bool HasDirectInterfaces { get; }

		IList<ISimpleMemberPointerModel> DirectInterfaces { get; }

	}
}
