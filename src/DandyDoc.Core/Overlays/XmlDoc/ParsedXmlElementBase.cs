using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	public abstract class ParsedXmlElementBase : ParsedXmlNodeBase
	{

		protected ParsedXmlElementBase(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			if (null == element) throw new ArgumentNullException("element");
			if (null == xmlDocBase) throw new ArgumentNullException("xmlDocBase");
			Contract.EndContractBlock();
		}

		public XmlElement Element { get { return (XmlElement)Node; } }

	}
}
