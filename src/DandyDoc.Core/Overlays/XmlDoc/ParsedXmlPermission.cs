using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParsedXmlPermission : ParsedCrefXmlElementBase
	{

		internal ParsedXmlPermission(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

	}
}
