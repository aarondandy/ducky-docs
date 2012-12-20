using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public abstract class ParsedXmlCodeBase : ParsedXmlElementBase
	{

		protected ParsedXmlCodeBase(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

		public string Language {
			get {
				var langNode = Element.SelectSingleNode("@lang") ?? Element.SelectSingleNode("@language");
				if (null == langNode)
					return null;
				return langNode.Value;
			}
		}

	}
}
