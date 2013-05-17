using DandyDoc.CRef;
using System.Diagnostics.Contracts;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// A code doc model for an event member.
    /// </summary>
    public class CodeDocEvent : CodeDocMemberContentBase
    {

        /// <summary>
        /// Creates a new model for an event member.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocEvent(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// The delegate type of the event.
        /// </summary>
        public ICodeDocMember DelegateType { get; set; }

    }
}
