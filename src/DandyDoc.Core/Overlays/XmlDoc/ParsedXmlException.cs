using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Utility;

namespace DandyDoc.Overlays.XmlDoc
{

	[Obsolete]
	public class ParsedXmlException : ParsedCrefXmlElementBase
	{

		internal ParsedXmlException(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

		public ParsedXmlContractCondition RelatedEnsures {
			get {
				var previousSibling = Element.FindPreviousSiblingElement();
				if (null == previousSibling)
					return null;
				if("ensuresOnThrow".Equals(previousSibling.Name))
					return new ParsedXmlContractCondition(previousSibling, DocBase);
				return null;
			}
		}

		public bool HasRelatedEnsures {
			get {
				var previousSibling = Element.FindPreviousSiblingElement();
				return null != previousSibling
					&& "ensuresOnThrow".Equals(previousSibling.Name);
			}
		}

	}
}
