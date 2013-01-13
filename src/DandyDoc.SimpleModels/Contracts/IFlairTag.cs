namespace DandyDoc.SimpleModels.Contracts
{
	public interface IFlairTag
	{

		string IconId { get; }

		string Category { get; }

		IComplexTextNode Description { get; }

	}
}
