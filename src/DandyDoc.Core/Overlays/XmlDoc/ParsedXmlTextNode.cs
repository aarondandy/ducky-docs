using System;
using System.Diagnostics.Contracts;
using System.Web;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParsedXmlTextNode : ParsedXmlNodeBase
	{

		internal ParsedXmlTextNode(XmlText textNode, DefinitionXmlDocBase xmlDocBase)
			: base(textNode, xmlDocBase)
		{
			if(null == textNode) throw new ArgumentNullException("textNode");
			Contract.Requires(null != xmlDocBase);
		}

		new public XmlText Node{
			get{
				return (XmlText) (base.Node);
			}
		}

		public string HtmlDecoded{
			get{
				return HttpUtility.HtmlDecode(Node.OuterXml);
			}
		}

	}
}
