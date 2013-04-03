using System;
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

        [Obsolete("Should be a CodeDocSimpleEntity")]
        public CRefIdentifier ValueTypeCRef { get; set; }

        public bool HasValueDescription{
            get {
                var valueDesc = ValueDescription;
                return valueDesc != null && valueDesc.HasChildren;
            }
        }

        public XmlDocElement ValueDescription { get { return XmlDocs == null ? null : XmlDocs.ValueElement; } }

        public bool IsLiteral { get; set; }

        public bool IsInitOnly { get; set; }

    }
}
