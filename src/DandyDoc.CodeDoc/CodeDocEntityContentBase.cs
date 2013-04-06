using System.Collections.Generic;
using DandyDoc.CRef;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public abstract class CodeDocEntityContentBase : CodeDocSimpleEntity, ICodeDocEntityContent
    {

        private static readonly XmlDocElement[] EmptyXmlDocElementArray = new XmlDocElement[0];
        protected static readonly XmlDocRefElement[] EmptyXmlDocRefElementArray = new XmlDocRefElement[0];

        public CodeDocEntityContentBase(CRefIdentifier cRef) : base(cRef) {
            Contract.Requires(cRef != null);
        }

        public bool HasExamples { get { return Examples.Count > 0; } }

        public IList<XmlDocElement> Examples {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocElementArray : XmlDocs.ExampleElements;
            }
        }

        public bool HasPermissions { get { return Permissions.Count > 0; } }

        public IList<XmlDocRefElement> Permissions {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocRefElementArray : XmlDocs.PermissionElements;
            }
        }

        public bool HasRemarks { get { return Remarks.Count > 0; } }

        public IList<XmlDocElement> Remarks {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocElementArray : XmlDocs.RemarksElements;
            }
        }

        public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }

        public IList<XmlDocRefElement> SeeAlso {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return XmlDocs == null ? EmptyXmlDocRefElementArray : XmlDocs.SeeAlsoElements;
            }
        }

        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        public XmlDocNode Summary {
            get { return XmlDocs == null ? null : XmlDocs.SummaryElement; }
        }

    }
}
