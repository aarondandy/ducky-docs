namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModel
	{

		string DisplayName { get; }

		string FullName { get; }

		string CRef { get; }

		ISimpleModelRepository RootRepository { get; }

	}
}
