using System;
using System.Collections.Generic;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocException
    {

        CRefIdentifier ExceptionCRef { get; }

        bool HasConditions { get; }

        IList<XmlDocNode> Conditions { get; }

        bool HasEnsures { get; }

        [Obsolete("Should this be an ICodeDocContractCondition?")]
        IList<XmlDocNode> Ensures { get; }

    }
}
