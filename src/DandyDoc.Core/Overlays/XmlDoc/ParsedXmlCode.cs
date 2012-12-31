using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParsedXmlCode : ParsedXmlElementBase
	{

		internal ParsedXmlCode(XmlElement element, bool inline, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
			Inline = inline;
		}

		public bool Inline { get; private set; }

		public string Language {
			get {
				var langNode = Element.SelectSingleNode("@lang") ?? Element.SelectSingleNode("@language"); // TODO: can this be a single query?
				return null == langNode ? null : langNode.Value;
			}
		}

	}
}
