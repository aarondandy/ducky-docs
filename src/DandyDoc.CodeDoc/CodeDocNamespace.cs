using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocNamespace : CodeDocSimpleEntity, ICodeDocNamespace
    {

        public CodeDocNamespace(CRefIdentifier cRef)
            : base(cRef)
        {
            Contract.Requires(cRef != null);
        }

        public IList<ICodeDocEntity> Types { get; set; }

        public IList<ICodeDocAssembly> Assemblies { get; set; }
    }
}
