using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ITypeSimpleModel : ISimpleModel, ISimpleModelMembersCollection
	{

		bool HasBaseChain { get; }

		IList<ISimpleMemberPointerModel> BaseChain { get; }

		bool HasDirectInterfaces { get; }

		IList<ISimpleMemberPointerModel> DirectInterfaces { get; }

	}
}
