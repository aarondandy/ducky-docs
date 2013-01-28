using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{

	[Obsolete]
	public class ParsedXmlParagraphElement : ParsedXmlElementBase
	{

		internal ParsedXmlParagraphElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != docBase);
		}

	}
}
