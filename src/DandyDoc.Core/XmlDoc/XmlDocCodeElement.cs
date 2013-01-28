using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Utility;

namespace DandyDoc.XmlDoc
{
	public class XmlDocCodeElement : XmlDocElement
	{

		public XmlDocCodeElement(XmlElement element, IEnumerable<XmlDocNode> children)
			: this("C".Equals(element.Name, StringComparison.OrdinalIgnoreCase), element, children) {
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public XmlDocCodeElement(bool isInline, XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
			IsInline = isInline;
		}

		public bool IsInline { get; private set; }

		public string Language {
			get {
				return Element.Attributes.GetValueOrDefault("lang")
					?? Element.Attributes.GetValueOrDefault("language");
			}
		}

	}
}
