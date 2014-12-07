using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// An XML doc definition list item element.
    /// </summary>
    public class XmlDocDefinitionListItem : XmlDocElement
    {

        private struct GutsData
        {
            public XmlDocElement TermElement;
            public IList<XmlDocNode> TermContents;
            public XmlDocElement DescriptionElement;
            public IList<XmlDocNode> DescriptionContents;
        }

        internal static bool IsItemElement(XmlElement element) {
            Contract.Requires(element != null);
            return "ITEM".Equals(element.Name, StringComparison.OrdinalIgnoreCase)
                || "LISTHEADER".Equals(element.Name, StringComparison.OrdinalIgnoreCase);
        }

        private readonly Lazy<GutsData> _lazyGuts;

        /// <summary>
        /// Creates a new XML doc definition list item.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocDefinitionListItem(XmlElement element, IEnumerable<XmlDocNode> children)
            : this("LISTHEADER".Equals(element.Name, StringComparison.OrdinalIgnoreCase), element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// Creates a new XML doc definition list item.
        /// </summary>
        /// <param name="isHeader">Indicates that this list item is to act as a header.</param>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocDefinitionListItem(bool isHeader, XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
            IsHeader = isHeader;
            _lazyGuts = new Lazy<GutsData>(CreateGuts, false); // NOTE: No need to go through the overhead of being thread safe.
        }

        private GutsData CreateGuts() {
            var result = new GutsData();
            foreach (var child in Children.OfType<XmlDocElement>()) {
                if (result.TermElement == null && "TERM".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    result.TermElement = child;
                    if (null != result.DescriptionElement)
                        break;
                }
                else if (result.DescriptionElement == null && "DESCRIPTION".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    result.DescriptionElement = child;
                    if (null != result.TermElement)
                        break;
                }
            }

            result.TermContents = result.TermElement == null
                ? EmptyXmlDocNodeList
                : result.TermElement.Children;

            result.DescriptionContents = result.DescriptionElement == null
                ? EmptyXmlDocNodeList
                : result.DescriptionElement.Children;

            return result;
        }

        /// <summary>
        /// Indicates that this list item is to act as a header.
        /// </summary>
        public bool IsHeader { get; private set; }

        /// <summary>
        /// The XML doc term element.
        /// </summary>
        public XmlDocElement TermElement {
            get { return _lazyGuts.Value.TermElement; }
        }

        /// <summary>
        /// Determines if the list item has a term element.
        /// </summary>
        public bool HasTerm { get { return TermElement != null; } }

        /// <summary>
        /// The XML doc contents of the term element.
        /// </summary>
        public IList<XmlDocNode> TermContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return _lazyGuts.Value.TermContents;
            }
        }

        /// <summary>
        /// Determines if the term element has XML doc contents.
        /// </summary>
        public bool HasTermContents { get { return TermContents.Count > 0; } }

        /// <summary>
        /// The XML doc description element.
        /// </summary>
        public XmlDocElement DescriptionElement {
            get { return _lazyGuts.Value.DescriptionElement; }
        }

        /// <summary>
        /// Determines if the list item has a description element.
        /// </summary>
        public bool HasDescription { get { return DescriptionElement != null; } }

        /// <summary>
        /// The XML doc contents of the term element.
        /// </summary>
        public IList<XmlDocNode> DescriptionContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return _lazyGuts.Value.DescriptionContents;
            }
        }

        /// <summary>
        /// Determines if the description element has XML doc contents.
        /// </summary>
        public bool HasDescriptionContents { get { return DescriptionContents.Count > 0; } }

    }
}
