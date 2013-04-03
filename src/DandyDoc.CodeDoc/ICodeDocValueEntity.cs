using System;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocValueEntity
    {

        [Obsolete("Should be an ICodeDocEntity")]
        CRefIdentifier ValueTypeCRef { get; }

        bool HasValueDescription { get; }

        XmlDocElement ValueDescription { get; }

    }
}
