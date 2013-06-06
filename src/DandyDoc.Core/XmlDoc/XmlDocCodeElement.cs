using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Utility;

namespace DandyDoc.XmlDoc
{
    /// <summary>
    /// An XML doc code element.
    /// </summary>
    public class XmlDocCodeElement : XmlDocElement
    {

        /// <summary>
        /// Creates a new XML doc code element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocCodeElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : this("C".Equals(element.Name, StringComparison.OrdinalIgnoreCase), element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// Creates a new XML doc code element.
        /// </summary>
        /// <param name="isInline">Indicates that the contents are to be treated as in-line code.</param>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocCodeElement(bool isInline, XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
            IsInline = isInline;
        }

        /// <summary>
        /// Indicates that contents are to be treated as in-line code.
        /// </summary>
        public bool IsInline { get; private set; }

        /// <summary>
        /// Extracts the specified language for the code element.
        /// </summary>
        public string Language {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                var result = Element.GetAttribute("lang");
                if (!String.IsNullOrEmpty(result))
                    return result;

                result = Element.GetAttribute("language");
                if (!String.IsNullOrEmpty(result))
                    return result;

                return String.Empty;
            }
        }

    }
}
