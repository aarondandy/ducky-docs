using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// A general XML doc element wrapper.
    /// </summary>
    public class XmlDocElement : XmlDocNode
    {

        /// <summary>
        /// Creates a new XML doc element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        public XmlDocElement(XmlElement element)
            : this(element, null) {
            Contract.Requires(element != null);
        }

        /// <summary>
        /// Creates a new XML doc element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// The raw wrapped XML element.
        /// </summary>
        public XmlElement Element {
            get {
                Contract.Ensures(Contract.Result<XmlElement>() != null);
                return Node as XmlElement;
            }
        }

        /// <summary>
        /// The element tag name.
        /// </summary>
        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return Element.Name;
            }
        }

        /// <summary>
        /// Extracts the opening tag XML.
        /// </summary>
        public string OpenTagXml {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                var inner = Node.InnerXml;
                if (String.IsNullOrEmpty(inner))
                    return Node.OuterXml;
                var outer = Node.OuterXml;
                var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
                if (innerIndex <= 0)
                    return String.Empty;
                return outer.Substring(0, innerIndex);
            }
        }

        /// <summary>
        /// Extracts the closing tag XML.
        /// </summary>
        public string CloseTagXml {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                var inner = Node.InnerXml;
                if (String.IsNullOrEmpty(inner))
                    return String.Empty;
                var outer = Node.OuterXml;
                var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
                if (innerIndex < 0)
                    return String.Empty;
                return outer.Substring(innerIndex + inner.Length);
            }
        }

        /// <summary>
        /// Traverses the prior sibling XML doc elements.
        /// </summary>
        /// <remarks>
        /// Be aware that this may skip some XML doc nodes that are not elements
        /// such as <seealso cref="DuckyDocs.XmlDoc.XmlDocTextNode"/>.
        /// </remarks>
        public IEnumerable<XmlDocElement> PriorElements {
            get {
                return PriorSiblings.OfType<XmlDocElement>();
            }
        }

        /// <summary>
        /// Gets the prior sibling XML doc element if one exists.
        /// </summary>
        public XmlDocElement PriorElement {
            get {
                return PriorElements.FirstOrDefault();
            }
        }

        /// <summary>
        /// Traverses the next sibling XML doc elements.
        /// </summary>
        /// <remarks>
        /// Be aware that this may skip some XML doc nodes that are not elements
        /// such as <seealso cref="DuckyDocs.XmlDoc.XmlDocTextNode"/>.
        /// </remarks>
        public IEnumerable<XmlDocElement> NextElements {
            get {
                return NextSiblings.OfType<XmlDocElement>();
            }
        }

        /// <summary>
        /// Gets the next sibling XML doc element if one exists.
        /// </summary>
        public XmlDocElement NextElement {
            get {
                return NextElements.FirstOrDefault();
            }
        }

    }
}
