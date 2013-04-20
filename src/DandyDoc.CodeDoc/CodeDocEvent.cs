using DandyDoc.CRef;
using System.Diagnostics.Contracts;

namespace DandyDoc.CodeDoc
{
    public class CodeDocEvent : CodeDocEntityContentBase, ICodeDocEvent
    {

        public CodeDocEvent(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public ICodeDocEntity DelegateType { get; set; }

    }
}
