using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParsedXmlSeeElement : ParsedCrefXmlElementBase
	{

		internal ParsedXmlSeeElement(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

	}
}
