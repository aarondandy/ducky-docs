using System;
using System.Diagnostics.Contracts;
using System.Web;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocTextNode : XmlDocNode
	{

		public XmlDocTextNode(XmlCharacterData text)
			: base(text)
		{
			Contract.Requires(text != null);
		}

		public XmlCharacterData CharacterData {
			get {
				Contract.Ensures(Contract.Result<XmlCharacterData>() != null);
				return Node as XmlText;
			}
		}

		public string Text {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return CharacterData.OuterXml;
			}
		}

		public bool IsWhitespace {
			get { return String.IsNullOrWhiteSpace(Text); }
		}

		public string HtmlDecoded {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return HttpUtility.HtmlDecode(Text);
			}
		}

	}
}
