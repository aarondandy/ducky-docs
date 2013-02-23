using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocParameter
	{

		string Name { get; }

        CRefIdentifier TypeCRef { get; }

		bool HasSummary { get; }

	}
}
