using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocRefElement : XmlDocElement
	{

		public XmlDocRefElement(XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public string CRef {
			get { return Element.GetAttribute("cref"); }
		}

		public string HRef {
			get { return Element.GetAttribute("href"); }
		}

		public string LangWord {
			get { return Element.GetAttribute("langword"); }
		}

	}
}
