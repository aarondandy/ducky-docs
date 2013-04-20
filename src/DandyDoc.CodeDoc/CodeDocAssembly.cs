using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocAssembly : CodeDocEntityContentBase, ICodeDocAssembly
    {

        public CodeDocAssembly(CRefIdentifier cRef)
            : base(cRef)
        {
            Contract.Requires(cRef != null);
        }

        public string AssemblyFileName { get; set; }

        public IList<ICodeDocNamespace> Namespaces { get; set; }

        public IList<CRefIdentifier> TypeCRefs { get; set; }
    }
}
