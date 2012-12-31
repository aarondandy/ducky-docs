using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Overlays.Cref;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParsedXmlElement : ParsedXmlElementBase
	{

		internal ParsedXmlElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			if (null == element) throw new ArgumentNullException("element");
			if (null == docBase) throw new ArgumentNullException("docBase");
			Contract.EndContractBlock();
		}

	}
}
