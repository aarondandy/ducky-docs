using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocAssembly : CodeDocSimpleEntity, ICodeDocAssembly
    {

        public CodeDocAssembly(CRefIdentifier cRef)
            : base(cRef)
        {
            Contract.Requires(cRef != null);
        }

        public string AssemblyFileName { get; set; }

        public IList<CRefIdentifier> RootTypes { get; set; }

        public IList<CRefIdentifier> AllTypes { get; set; }

        public IList<ICodeDocNamespace> Namespaces { get; set; }

    }
}
