using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
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
