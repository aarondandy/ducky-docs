using System;
using System.Diagnostics.Contracts;
using System.Web;
using System.Xml;

namespace DuckyDocs.XmlDoc
{

    /// <summary>
    /// An XML doc text node.
    /// </summary>
    public class XmlDocTextNode : XmlDocNode
    {

        /// <summary>
        /// Creates a new XML doc text node.
        /// </summary>
        /// <param name="text">The raw XML data to wrap.</param>
        public XmlDocTextNode(XmlCharacterData text)
            : base(text) {
            Contract.Requires(text != null);
        }

        /// <summary>
        /// The wrapped XML data.
        /// </summary>
        public XmlCharacterData CharacterData {
            get {
                Contract.Ensures(Contract.Result<XmlCharacterData>() != null);
                return (XmlCharacterData)Node;
            }
        }

        /// <summary>
        /// The raw XML text.
        /// </summary>
        public string Text {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                return CharacterData.OuterXml;
            }
        }

        /// <summary>
        /// Determines if the text is white space.
        /// </summary>
        public bool IsWhiteSpace {
            get { return String.IsNullOrWhiteSpace(Text); }
        }

        /// <summary>
        /// The XML decoded text for display.
        /// </summary>
        public string HtmlDecoded {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                return HttpUtility.HtmlDecode(Text);
            }
        }

    }
}
