using System.Diagnostics.Contracts;
using System.Web;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocTextNode : XmlDocNode
	{

		public XmlDocTextNode(XmlText text)
			: base(text)
		{
			Contract.Requires(text != null);
		}

		public XmlText TextNode {
			get {
				Contract.Ensures(Contract.Result<XmlText>() != null);
				return Node as XmlText;
			}
		}

		public string Text {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return TextNode.OuterXml;
			}
		}

		public string HtmlDecoded {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return HttpUtility.HtmlDecode(Text);
			}
		}

	}
}
