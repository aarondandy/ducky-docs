using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using DuckyDocs.Utility;

namespace DuckyDocs.XmlDoc
{
    /// <summary>
    /// An assembly XML document.
    /// </summary>
    public class XmlAssemblyDocument : IXmlDocMemberProvider
    {

        private static XmlDocument Load(string filePath) {
            if (String.IsNullOrEmpty(filePath)) throw new ArgumentException("File path is not valid.", "filePath");
            Contract.Ensures(Contract.Result<XmlDocument>() != null);
            var doc = new XmlDocument();
            doc.Load(filePath);
            return doc;
        }

        /// <summary>
        /// Creates an assembly XML document.
        /// </summary>
        /// <param name="filePath">The file path of the XML document to load.</param>
        public XmlAssemblyDocument(string filePath)
            : this(Load(filePath)) {
            Contract.Requires(!String.IsNullOrEmpty(filePath));
        }

        /// <summary>
        /// Creates an assembly XML document.
        /// </summary>
        /// <param name="xmlDocument">The raw XML document to wrap.</param>
        public XmlAssemblyDocument(XmlDocument xmlDocument) {
            if (xmlDocument == null) throw new ArgumentNullException("xmlDocument");
            Contract.EndContractBlock();

            Parser = XmlDocParser.Default;

            var members = xmlDocument.SelectNodes("/doc/members/member");
            if (null != members) {
                foreach (var member in members.Cast<XmlElement>()) {
                    var subElements = member.ChildNodes.OfType<XmlElement>().ToList();
                    foreach (var subElement in subElements) {
                        var replacement = xmlDocument.CreateDocumentFragment();
                        replacement.InnerXml = TextUtility.NormalizeAndUnindentElement(subElement.OuterXml) + "\n";
                        Contract.Assume(null != subElement.ParentNode);
                        subElement.ParentNode.ReplaceChild(replacement, subElement);
                    }
                }
            }
            XmlDocument = xmlDocument;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(XmlDocument != null);
            Contract.Invariant(Parser != null);
        }

        /// <summary>
        /// The raw XML document that is wrapped.
        /// </summary>
        protected XmlDocument XmlDocument { get; private set; }

        /// <summary>
        /// The parsed that is used to create XML doc member elements.
        /// </summary>
        public XmlDocParser Parser { get; protected set; }

        /// <summary>
        /// Gets a raw XML documentation element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The raw XML element if found.</returns>
        public XmlElement GetMemberRawElement(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();

            return XmlDocument.SelectSingleNode(String.Format(
                "/doc/members/member[@name=\"{0}\"]", cRef)) as XmlElement;
        }

        /// <summary>
        /// Gets a parsed XML documentation member element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The parsed XML doc member element if found.</returns>
        public virtual XmlDocMember GetMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();
            var node = GetMemberRawElement(cRef);
            if (node == null)
                return null;
            return new XmlDocMember(node, node.ChildNodes.Cast<XmlNode>().Select(Parser.Parse));
        }

    }
}
