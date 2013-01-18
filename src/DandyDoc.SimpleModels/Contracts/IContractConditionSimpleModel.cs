namespace DandyDoc.SimpleModels.Contracts
{
	public interface IContractConditionSimpleModel
	{

		bool HasException { get; }

		ISimpleMemberPointerModel Exception { get; }

		IComplexTextNode Description { get; }

	}
}
