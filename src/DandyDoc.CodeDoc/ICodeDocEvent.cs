using System;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEvent : ICodeDocEntityContent
    {

        [Obsolete("Should be a CodeDoc entity")]
        CRefIdentifier DelegateCRef { get; }

    }
}
