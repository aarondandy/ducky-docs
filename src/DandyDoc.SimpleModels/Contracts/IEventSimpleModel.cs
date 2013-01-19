namespace DandyDoc.SimpleModels.Contracts
{
	public interface IEventSimpleModel : ISimpleModel
	{

		ISimpleMemberPointerModel DelegateType { get; }

	}
}
