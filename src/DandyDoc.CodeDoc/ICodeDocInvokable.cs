using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocInvokable
    {

        bool HasParameters { get; }

        IList<ICodeDocParameter> Parameters { get; }

        bool HasReturn { get; }

        ICodeDocParameter Return { get; }

        bool HasExceptions { get; }

        IList<ICodeDocException> Exceptions { get; }

        bool HasEnsures { get; }

        IList<XmlDocContractElement> Ensures { get; }

        bool HasNormalTerminationEnsures { get; }

        IEnumerable<XmlDocContractElement> NormalTerminationEnsures { get; }

        bool HasRequires { get; }

        IList<XmlDocContractElement> Requires { get; }

        bool IsPure { get; }

        bool IsExtensionMethod { get; }

        bool IsOperatorOverload { get; }

        bool IsSealed { get; }

        bool IsAbstract { get; }

        bool IsVirtual { get; }

    }
}
