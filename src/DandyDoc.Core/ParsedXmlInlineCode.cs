using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlInlineCode : ParsedXmlCodeBase
	{

		public ParsedXmlInlineCode(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity){
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

	}
}
