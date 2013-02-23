using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocEvent : ICodeDocEntity
	{

        CRefIdentifier DelegateCRef { get; }

	}
}
