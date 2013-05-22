using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMemberXmlDataProvider : ICodeDocMemberDataProvider
    {

        public CodeDocMemberXmlDataProvider(XmlDocMember xmlDoc) {
            if(xmlDoc == null) throw new ArgumentNullException("xmlDoc");
            XmlDoc = xmlDoc;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(XmlDoc != null);
        }

        public XmlDocMember XmlDoc { get; private set; }

        public bool HasExamples { get { return XmlDoc.HasExampleElements; } }

        public IEnumerable<XmlDocElement> GetExamples() {
            return XmlDoc.ExampleElements;
        }

        public bool HasSummaryContents { get { return XmlDoc.HasSummaryContents; } }

        public IEnumerable<XmlDocNode> GetSummaryContents() {
            return XmlDoc.SummaryContents;
        }

        public bool HasValueDescriptionContents { get { return XmlDoc.HasValueContents; } }

        public IEnumerable<XmlDocNode> GeValueDescriptionContents() {
            return XmlDoc.ValueContents;
        }

        public bool HasPermissions {
            get { return XmlDoc.HasPermissionElements; }
        }

        public IEnumerable<XmlDocRefElement> GetPermissions() {
            return XmlDoc.PermissionElements;
        }

        public bool HasRemarks {
            get { return XmlDoc.HasRemarksElements; }
        }

        public IEnumerable<XmlDocElement> GetRemarks() {
            return XmlDoc.RemarksElements;
        }

        public bool HasSeeAlsos {
            get { return XmlDoc.HasSeeAlsoElements; }
        }

        public IEnumerable<XmlDocRefElement> GetSeeAlsos() {
            return XmlDoc.SeeAlsoElements;
        }

        public bool? IsPure {
            get {
                return XmlDoc.HasPureElement ? true : default(bool?);
            }
        }

        public ExternalVisibility.ExternalVisibilityKind? ExternalVisibility {
            get { return null; }
        }

        public bool? IsStatic {
            get { return null; }
        }

        public bool? IsObsolete {
            get{ return null; }
        }

        public bool HasParameterSummaryContents(string parameterName) {
            if (!XmlDoc.HasParameterSummaries)
                return false;
            var parameterSummary = XmlDoc.GetParameterSummary(parameterName);
            return parameterSummary != null && parameterSummary.HasChildren;
        }

        public IEnumerable<XmlDocNode> GetParameterSummaryContents(string parameterName) {
            if (!XmlDoc.HasParameterSummaries)
                return Enumerable.Empty<XmlDocNode>();
            var parameterSummary = XmlDoc.GetParameterSummary(parameterName);
            return parameterSummary == null ? Enumerable.Empty<XmlDocNode>() : parameterSummary.Children;
        }

        public bool HasReturnSummaryContents {
            get { return XmlDoc.HasReturnsContents; }
        }

        public IEnumerable<XmlDocNode> GetReturnSummaryContents() {
            return XmlDoc.ReturnsContents;
        }

        public bool? RequiresParameterNotEverNull(string parameterName) {
            if (XmlDoc.HasRequiresElements && XmlDoc.RequiresElements.Any(r => r.RequiresParameterNotEverNull(parameterName)))
                return true;

            return null;
        }

        public bool? EnsuresResultNotEverNull {
            get {
                if (XmlDoc.HasEnsuresElements && XmlDoc.EnsuresElements.Any(r => r.EnsuresResultNotEverNull))
                    return true;
                return null;
            }
        }

        public bool HasGenericTypeSummaryContents(string typeParameterName) {
            if (!XmlDoc.HasTypeParameterSummaries)
                return false;
            var summary = XmlDoc.GetTypeParameterSummary(typeParameterName);
            return summary != null && summary.HasChildren;
        }

        public IEnumerable<XmlDocNode> GetGenericTypeSummaryContents(string typeParameterName) {
            if (!XmlDoc.HasTypeParameterSummaries)
                return Enumerable.Empty<XmlDocNode>();
            var summary = XmlDoc.GetTypeParameterSummary(typeParameterName);
            return summary != null
                ? summary.Children
                : Enumerable.Empty<XmlDocNode>();
        }


        public bool HasExceptions {
            get { return XmlDoc.HasExceptionElements; }
        }

        public IEnumerable<XmlDocRefElement> GetExceptions() {
            return XmlDoc.ExceptionElements;
        }

        public bool HasEnsures {
            get { return XmlDoc.HasEnsuresElements; }
        }

        public IEnumerable<XmlDocContractElement> GetEnsures() {
            return XmlDoc.EnsuresElements;
        }

        public bool HasRequires {
            get { return XmlDoc.HasRequiresElements; }
        }

        public IEnumerable<XmlDocContractElement> GetRequires() {
            return XmlDoc.RequiresElements;
        }

        public bool HasInvariants {
            get { return XmlDoc.HasInvariantElements; }
        }

        public IEnumerable<XmlDocContractElement> GetInvariants() {
            return XmlDoc.InvariantElements;
        }
    }
}
