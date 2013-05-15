using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{

    /// <summary>
    /// An XML doc definition list.
    /// </summary>
    public class XmlDocDefinitionList : XmlDocElement
    {

        /// <summary>
        /// Constructs a new XML doc definition list.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocDefinitionList(XmlElement element, IEnumerable<XmlDocNode> children)
            : this(element.GetAttribute("type"), element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// Constructs a new XML doc definition list.
        /// </summary>
        /// <param name="listType">The list type such as table, bullet, or number.</param>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        protected XmlDocDefinitionList(string listType, XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
            ListType = String.IsNullOrEmpty(listType) ? "table" : listType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(!String.IsNullOrEmpty(ListType));
        }

        /// <summary>
        /// The list type.
        /// </summary>
        /// <remarks>
        /// Common values are:
        /// <list type="BULLET">
        /// <item><term>BULLET</term></item>
        /// <item><term>NUMBER</term></item>
        /// <item><term>TABLE</term></item>
        /// </list>
        /// </remarks>
        public string ListType { get; private set; }

        /// <summary>
        /// Gets the XML doc items within this list.
        /// </summary>
        public IList<XmlDocDefinitionListItem> Items {
            get { return Children.OfType<XmlDocDefinitionListItem>().ToList(); }
        }

        /// <summary>
        /// Determines if this list has items.
        /// </summary>
        public bool HasItems {
            get { return Items.Count > 0; }
        }

    }
}
