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
    }
}
