using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocProperty : CodeDocEntityContentBase, ICodeDocProperty
    {

        public CodeDocProperty(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        public IList<ICodeDocParameter> Parameters { get; set; }

        public bool HasGetter { get { return Getter != null; } }

        public ICodeDocMethod Getter { get; set; }

        public bool HasSetter { get { return Setter != null; } }

        public ICodeDocMethod Setter { get; set; }

        public ICodeDocEntity ValueType { get; set; }

        public bool HasValueDescription {
            get { return ValueDescription != null; }
        }

        public XmlDocElement ValueDescription {
            get { return XmlDocs == null ? null : XmlDocs.ValueElement; }
        }

        public bool HasValueDescriptionContents { get { return XmlDocs != null && XmlDocs.HasValueContents; } }

        public IList<XmlDocNode> ValueDescriptionContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return XmlDocs != null && XmlDocs.HasValueContents
                    ? XmlDocs.ValueContents
                    : new XmlDocNode[0];
            }
        }

    }
}
