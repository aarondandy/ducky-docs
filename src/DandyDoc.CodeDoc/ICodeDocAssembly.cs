using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocAssembly : ICodeDocEntityContent
    {

        string AssemblyFileName { get; }

        IList<CRefIdentifier> TypeCRefs { get; }

        IList<CRefIdentifier> NamespaceCRefs { get; }

    }
}
