using System;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocParameter : ICodeDocParameter
    {

        public CodeDocParameter(string name, CRefIdentifier typeCRef, XmlDocElement summary = null) {
            Name = name;
            TypeCRef = typeCRef;
            Summary = summary;
        }

        public string Name { get; private set; }

        [Obsolete("Should be a CodeDoc model")]
        public CRefIdentifier TypeCRef { get; private set; }

        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        public XmlDocElement Summary { get; private set; }

        public bool IsOut { get; set; }

        public bool IsByRef { get; set; }

    }
}
