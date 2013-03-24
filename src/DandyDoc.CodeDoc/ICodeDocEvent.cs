using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEvent : ICodeDocEntityContent
	{

        CRefIdentifier DelegateCRef { get; }

	}
}
