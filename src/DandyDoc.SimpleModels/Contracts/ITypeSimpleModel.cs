using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ITypeSimpleModel : ISimpleModel, ISimpleModelMembersCollection
	{

		bool IsEnum { get; }

		bool HasBaseChain { get; }

		IList<ISimpleMemberPointerModel> BaseChain { get; }

		bool HasDirectInterfaces { get; }

		IList<ISimpleMemberPointerModel> DirectInterfaces { get; }

		bool HasGenericParameters { get; }

		IList<IGenericParameterSimpleModel> GenericParameters { get; } 

	}
}
