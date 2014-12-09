using System.Runtime.Serialization;
using DuckyDocs.CRef;
using System.Diagnostics.Contracts;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// A code doc model for an event member.
    /// </summary>
    [DataContract]
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
        [DataMember]
        public CodeDocType DelegateType { get; set; }

    }
}
