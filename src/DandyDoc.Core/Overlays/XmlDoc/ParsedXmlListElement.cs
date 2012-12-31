using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParsedXmlListElement : ParsedXmlElementBase
	{

		internal ParsedXmlListElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != docBase);
		}

		public string ListType {
			get {
				var typeAttribute = Element.Attributes["type"];
				return null == typeAttribute ? null : typeAttribute.Value;
			}
		}

		public IEnumerable<ParsedXmlListItemElement> Items {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<ParsedXmlListItemElement>>() != null);
				return Children.OfType<ParsedXmlListItemElement>();
			}
		}

	}
}
