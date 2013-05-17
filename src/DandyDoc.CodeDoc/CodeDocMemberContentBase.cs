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

        private static readonly XmlDocElement[] EmptyXmlDocElementArray = new XmlDocElement[0];
        private static readonly XmlDocRefElement[] EmptyXmlDocRefElementArray = new XmlDocRefElement[0];

        /// <summary>
        /// Consrtructor for a code doc member model.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        protected CodeDocMemberContentBase(CRefIdentifier cRef) : base(cRef) {
            Contract.Requires(cRef != null);
        }

        /// <summary>
        /// Indicates that this model has XML doc examples.
        /// </summary>
        public bool HasExamples { get { return Examples.Count > 0; } }

        /// <summary>
        /// Gets the XML doc example elements.
        /// </summary>
        public IList<XmlDocElement> Examples {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocElementArray : XmlDocs.ExampleElements;
            }
        }

        /// <summary>
        /// Indicates that this model has XML doc permissions.
        /// </summary>
        public bool HasPermissions { get { return Permissions.Count > 0; } }

        /// <summary>
        /// Gets the XML doc permissions elements.
        /// </summary>
        public IList<XmlDocRefElement> Permissions {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocRefElementArray : XmlDocs.PermissionElements;
            }
        }

        /// <summary>
        /// Indicates that this model has XML doc remarks.
        /// </summary>
        public bool HasRemarks { get { return Remarks.Count > 0; } }

        /// <summary>
        /// Gets the XML doc remarks elements.
        /// </summary>
        public IList<XmlDocElement> Remarks {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocElementArray : XmlDocs.RemarksElements;
            }
        }

        /// <summary>
        /// Indicates that this model has XML doc see also links.
        /// </summary>
        public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }

        /// <summary>
        /// Gets the XML doc see also elements.
        /// </summary>
        public IList<XmlDocRefElement> SeeAlso {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocRefElementArray : XmlDocs.SeeAlsoElements;
            }
        }

        /// <summary>
        /// Gets the declaring member model for this member model.
        /// </summary>
        public ICodeDocMember DeclaringType { get; set; }

        /// <summary>
        /// Gets the namespace for this model.
        /// </summary>
        public ICodeDocMember Namespace { get; set; }

        /// <summary>
        /// Gets the assembly for this model.
        /// </summary>
        public ICodeDocMember Assembly { get; set; }

    }
}
