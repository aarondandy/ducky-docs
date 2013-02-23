using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocValueEntity
	{

        CRefIdentifier ValueTypeCRef { get; }

		bool HasValueDescription { get; }

	}
}
