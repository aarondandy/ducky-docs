using System;
using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocException
    {

        ICodeDocEntity ExceptionType { get; }

        bool HasConditions { get; }

        IList<XmlDocNode> Conditions { get; }

        bool HasEnsures { get; }

        [Obsolete("Should this be an ICodeDocContractCondition?")]
        IList<XmlDocNode> Ensures { get; }

    }
}
