using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DuckyDocs.XmlDoc
{
    /// <summary>
    /// An XML doc reference element that references other items by
    /// URL, language word or more commonly code reference(cref).
    /// </summary>
    public class XmlDocRefElement : XmlDocElement
    {

        /// <summary>
        /// Creates a new XML doc reference element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">Parsed XML doc child nodes.</param>
        public XmlDocRefElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// The code reference (cref) that may be targeted.
        /// </summary>
        public virtual string CRef { get { return Element.GetAttribute("cref"); } }

        /// <summary>
        /// The hypertext reference (href) that may be targeted.
        /// </summary>
        public virtual string HRef { get { return Element.GetAttribute("href"); } }

        /// <summary>
        /// The language word that may be targeted.
        /// </summary>
        public virtual string LangWord { get { return Element.GetAttribute("langword"); } }

    }
}
