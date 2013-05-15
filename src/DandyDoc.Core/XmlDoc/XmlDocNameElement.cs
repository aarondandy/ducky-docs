using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    /// <summary>
    /// An XML doc element that targets something by name.
    /// </summary>
    public class XmlDocNameElement : XmlDocElement
    {
        /// <summary>
        /// Creates a new XML doc name element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">Parsed XML doc child nodes.</param>
        public XmlDocNameElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// The name of the element that is targeted.
        /// </summary>
        public string TargetName {
            get {
                return Element.GetAttribute("name");
            }
        }

    }
}
