using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// Code doc entity representing an assembly.
    /// </summary>
    public class CodeDocAssembly : CodeDocEntityContentBase, ICodeDocAssembly
    {
        /// <summary>
        /// Creates a new assembly with the given cRef identifier.
        /// </summary>
        /// <param name="cRef">The assembly code reference.</param>
        /// <remarks>
        /// Assembly code references should be prefixed with 'A:' followed by the assembly short name or full name.
        /// </remarks>
        public CodeDocAssembly(CRefIdentifier cRef) : base(cRef) { 
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// The file name of the assembly.
        /// </summary>
        /// <remarks>
        /// The assembly file name should not be a full path but just the file name such as <c>MyLibrary.dll</c> .
        /// </remarks>
        public string AssemblyFileName { get; set; }

        /// <summary>
        /// The code references for the namespaces related to all exposed types within this assembly.
        /// </summary>
        /// <remarks>
        /// The object model often organizes types as being contained within namespaces.
        /// Because of this only references to the namespace are given rather than entities.
        /// Use the related repository to get a namespace entity.
        /// </remarks>
        public IList<CRefIdentifier> NamespaceCRefs { get; set; }

        /// <summary>
        /// The code references for all exposed types within this assembly. 
        /// </summary>
        /// <remarks>
        /// The object model often organizes types as being contained within namespaces.
        /// Because of this only references to the types are given rather than entities.
        /// Use the related repository to get a type entity.
        /// </remarks>
        public IList<CRefIdentifier> TypeCRefs { get; set; }
    }

}
