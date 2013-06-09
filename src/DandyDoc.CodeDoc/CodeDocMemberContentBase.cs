using System.Collections.Generic;
using DandyDoc.CRef;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// Base code doc member content model.
    /// </summary>
    public abstract class CodeDocMemberContentBase : CodeDocSimpleMember
    {

        /// <summary>
        /// Constructor for a code doc member model.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        protected CodeDocMemberContentBase(CRefIdentifier cRef) : base(cRef) {
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// Indicates that this model has XML doc examples.
        /// </summary>
        public bool HasExamples { get { return Examples != null && Examples.Count > 0; } }

        /// <summary>
        /// Gets the XML doc example elements.
        /// </summary>
        public IList<XmlDocElement> Examples { get; set; }

        /// <summary>
        /// Indicates that this model has XML doc permissions.
        /// </summary>
        public bool HasPermissions { get { return Permissions != null && Permissions.Count > 0; } }

        /// <summary>
        /// Gets the XML doc permissions elements.
        /// </summary>
        public IList<XmlDocRefElement> Permissions { get; set; }

        /// <summary>
        /// Indicates that this model has XML doc remarks.
        /// </summary>
        public bool HasRemarks { get { return Remarks != null && Remarks.Count > 0; } }

        /// <summary>
        /// Gets the XML doc remarks elements.
        /// </summary>
        public IList<XmlDocElement> Remarks { get; set; }

        /// <summary>
        /// Indicates that this model has XML doc see also links.
        /// </summary>
        public bool HasSeeAlso { get { return SeeAlso != null && SeeAlso.Count > 0; } }

        /// <summary>
        /// Gets the XML doc see also elements.
        /// </summary>
        public IList<XmlDocRefElement> SeeAlso { get; set; }

        /// <summary>
        /// Gets the declaring member model for this member model.
        /// </summary>
        public ICodeDocMember DeclaringType { get; set; }

        /// <summary>
        /// Gets the assembly for this model.
        /// </summary>
        public CodeDocSimpleAssembly Assembly { get; set; }

        /// <summary>
        /// Indicates that this member is obsolete.
        /// </summary>
        public bool? IsObsolete { get; set; }

        /// <summary>
        /// Indicates that this member is static.
        /// </summary>
        public bool? IsStatic { get; set; }

    }
}
