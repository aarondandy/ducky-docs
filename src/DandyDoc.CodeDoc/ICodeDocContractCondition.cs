using System;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    [Obsolete]
    public interface ICodeDocContractCondition
    {

        CRefIdentifier ExceptionCRef { get; }

        bool HasExceptionDescription { get; }

        XmlDocNode ExceptionDescription { get; }

        bool HasContractDescription { get; }

        XmlDocNode ContractDescription { get; }

    }
}
