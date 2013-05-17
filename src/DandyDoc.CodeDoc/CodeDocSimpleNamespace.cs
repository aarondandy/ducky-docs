using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc namespace model containing only code references.
    /// </summary>
    public class CodeDocSimpleNamespace : CodeDocSimpleMember
    {

        /// <summary>
        /// Creates a new namespace code doc model.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocSimpleNamespace(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// The code references for all exposed types within this namespace. 
        /// </summary>
        public IList<CRefIdentifier> TypeCRefs { get; set; }

        /// <summary>
        /// The code references for all assemblies that contain this namespace. 
        /// </summary>
        public IList<CRefIdentifier> AssemblyCRefs { get; set; }

    }
}
