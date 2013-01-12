namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModel
	{

		string Title { get; }

		string SubTitle { get; }

		string DisplayName { get; }

		string FullName { get; }

		string CRef { get; }

		string NamespaceName { get; }

		IAssemblySimpleModel ContainingAssembly { get; }

		ISimpleModelRepository RootRepository { get; }

	}
}
