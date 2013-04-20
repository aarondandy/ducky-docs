using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocParameter : ICodeDocParameter
    {

        public CodeDocParameter(string name, ICodeDocEntity parameterType, XmlDocElement summary = null) {
            Name = name;
            ParameterType = parameterType;
            Summary = summary;
        }

        public string Name { get; private set; }

        public ICodeDocEntity ParameterType { get; private set; }

        public bool HasSummary {
            get {
                var summary = Summary;
                return summary != null && summary.HasChildren;
            }
        }

        public XmlDocElement Summary { get; private set; }

        public bool HasSummaryContents {
            get { return Summary != null && Summary.HasChildren; }
        }

        public IList<XmlDocNode> SummaryContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return HasSummaryContents
                    ? Summary.Children
                    : new XmlDocNode[0];
            }
        }

        public bool IsOut { get; set; }

        public bool IsByRef { get; set; }

    }
}
