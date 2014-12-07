using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// A collection of assembly XML documents.
    /// </summary>
    public class XmlAssemblyDocumentCollection :
        Collection<XmlAssemblyDocument>,
        IXmlDocMemberProvider
    {

        /// <summary>
        /// A default empty collection.
        /// </summary>
        public XmlAssemblyDocumentCollection()
            : base() { }

        /// <summary>
        /// Creates a new collection containing the given <paramref name="items"/>.
        /// </summary>
        /// <param name="items">The items that will be added to the collection.</param>
        public XmlAssemblyDocumentCollection(IEnumerable<XmlAssemblyDocument> items)
            : this(null == items ? new List<XmlAssemblyDocument>() : items.ToList()) { }

        /// <summary>
        /// Creates a new collection wrapping the given list of <paramref name="items"/>.
        /// </summary>
        /// <param name="items">The list that will be wrapped by the collection.</param>
        public XmlAssemblyDocumentCollection(IList<XmlAssemblyDocument> items)
            : base(items) {
            Contract.Requires(items != null);
        }

        /// <summary>
        /// Gets a raw XML documentation element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The raw XML element if found.</returns>
        public XmlElement GetMemberRawElement(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();
            return this
                .Select(x => x.GetMemberRawElement(cRef))
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Gets a parsed XML documentation member element for a given code reference (<paramref name="cRef"/>).
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The parsed XML doc member element if found.</returns>
        public virtual XmlDocMember GetMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();
            return this
                .Select(x => x.GetMember(cRef))
                .FirstOrDefault(x => x != null);
        }

    }
}
