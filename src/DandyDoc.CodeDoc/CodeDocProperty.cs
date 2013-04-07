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

        [Obsolete("Should be a model")]
        public CRefIdentifier ValueTypeCRef { get; set; }

        public bool HasValueDescription {
            get { return ValueDescription != null; }
        }

        public XmlDocElement ValueDescription {
            get { return XmlDocs == null ? null : XmlDocs.ValueElement; }
        }
    }
}
