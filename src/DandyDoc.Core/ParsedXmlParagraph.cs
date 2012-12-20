using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlParagraph : ParsedXmlElementBase
	{

		public ParsedXmlParagraph(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

	}
}
