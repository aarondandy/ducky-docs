using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlBasicElementPart : ParsedXmlElementBase
	{

		public ParsedXmlBasicElementPart(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

	}
}
