using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocNameElement : XmlDocElement
	{

		public XmlDocNameElement(XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public string TargetName {
			get { return Element.GetAttribute("name"); }
		}

	}
}
