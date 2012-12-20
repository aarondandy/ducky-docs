using System.Diagnostics.Contracts;
using System.Web;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlTextPart : ParsedXmlNodeBase
	{

		public ParsedXmlTextPart(XmlNode node) : base(node){
			Contract.Requires(null != node);
		}

		public string HtmlDecoded { 
			get { return HttpUtility.HtmlDecode(RawXml); }
		}

	}
}
