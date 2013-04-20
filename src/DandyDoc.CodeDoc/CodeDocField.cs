using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocField : CodeDocEntityContentBase, ICodeDocField
    {

        public CodeDocField(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public ICodeDocEntity ValueType { get; set; }

        public bool HasValueDescription{
            get {
                var valueDesc = ValueDescription;
                return valueDesc != null && valueDesc.HasChildren;
            }
        }

        public XmlDocElement ValueDescription { get { return XmlDocs == null ? null : XmlDocs.ValueElement; } }

        public bool HasValueDescriptionContents { get { return XmlDocs != null && XmlDocs.HasValueContents; } }

        public IList<XmlDocNode> ValueDescriptionContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return XmlDocs != null && XmlDocs.HasValueContents
                    ? XmlDocs.ValueContents
                    : new XmlDocNode[0];
            }
        }

        public bool IsLiteral { get; set; }

        public bool IsInitOnly { get; set; }

    }
}
