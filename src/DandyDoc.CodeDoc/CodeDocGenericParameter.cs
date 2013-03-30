using System;
using System.Collections.Generic;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocGenericParameter : ICodeDocGenericParameter
    {
        public bool HasTypeConstraints {
            get { return TypeConstraints != null && TypeConstraints.Count > 0; }
        }

        [Obsolete("This should be some kind of ICodeDocEntity (pointer) to keep the CRef yet generate full name & a CRef that can be linked.")]
        public IList<CRefIdentifier> TypeConstraints { get; set; }

        public string Name { get; set; }

        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        public XmlDocNode Summary { get; set; }

        public bool IsContravariant { get; set; }

        public bool IsCovariant { get; set; }

        public bool HasReferenceTypeConstraint { get; set; }

        public bool HasNotNullableValueTypeConstraint { get; set; }

        public bool HasDefaultConstructorConstraint { get; set; }

    }
}
