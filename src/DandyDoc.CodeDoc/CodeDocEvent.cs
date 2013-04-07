using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DandyDoc.CRef;
using System.Diagnostics.Contracts;

namespace DandyDoc.CodeDoc
{
    public class CodeDocEvent : CodeDocEntityContentBase, ICodeDocEvent
    {

        public CodeDocEvent(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public CRefIdentifier DelegateCRef { get; set; }

    }
}
