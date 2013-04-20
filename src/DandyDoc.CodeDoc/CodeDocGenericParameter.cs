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

        public IList<ICodeDocEntity> TypeConstraints { get; set; }

        public string Name { get; set; }

        public bool HasSummary {
            get {
                return Summary != null;
            }
        }

        public XmlDocElement Summary { get; set; }

        public bool HasSummaryContents {
            get { return HasSummary && Summary.HasChildren; }
        }

        public IList<XmlDocNode> SummaryContents {
            get {
                return HasSummaryContents
                    ? Summary.Children
                    : new XmlDocNode[0];
            }
        }

        public bool IsContravariant { get; set; }

        public bool IsCovariant { get; set; }

        public bool HasReferenceTypeConstraint { get; set; }

        public bool HasNotNullableValueTypeConstraint { get; set; }

        public bool HasDefaultConstructorConstraint { get; set; }

        public bool HasAnyConstraints {
            get {
                return HasTypeConstraints
                    || HasReferenceTypeConstraint
                    || HasNotNullableValueTypeConstraint
                    || HasDefaultConstructorConstraint;
            }
        }

    }
}
