namespace DandyDoc.SimpleModels.Contracts
{
	public interface IEventSimpleModel : ISimpleModel
	{

		string SubTitle { get;  }

		ISimpleMemberPointerModel DelegateType { get; }

	}
}
