namespace DandyDoc.CodeDoc
{
	public interface ICodeDocParameter
	{

		string Name { get; }

		string TypeCRef { get; }

		bool HasSummary { get; }

	}
}
