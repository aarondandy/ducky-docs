using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParsedXmlListItemElement : ParsedXmlElementBase
	{

		internal ParsedXmlListItemElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != docBase);
		}

		public bool IsHeader {
			get { return "listheader".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); }
		}

		public ParsedXmlNodeBase Term {
			get {
				var node = Element.SelectSingleNode("term");
				if (null == node)
					return null;
				return Parse(node, DocBase);
			}
		}

		public ParsedXmlNodeBase Description {
			get {
				var node = Element.SelectSingleNode("description");
				if (null == node)
					return null;
				return Parse(node, DocBase);
			}
		}

	}
}
