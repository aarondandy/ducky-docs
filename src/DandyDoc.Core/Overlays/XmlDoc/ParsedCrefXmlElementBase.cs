using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{
	public abstract class ParsedCrefXmlElementBase : ParsedXmlElementBase
	{

		protected ParsedCrefXmlElementBase(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

		public virtual string CRef {
			get {
				var crefAttribute = Element.Attributes["cref"];
				return null == crefAttribute ? null : crefAttribute.Value;
			}
		}

		public virtual string HRef{
			get {
				var crefAttribute = Element.Attributes["href"];
				return null == crefAttribute ? null : crefAttribute.Value;
			}
		}

		public MemberReference CrefTarget {
			get {
				var cref = CRef;
				return String.IsNullOrEmpty(cref) ? null : CRefOverlay.GetReference(cref);
			}
		}

	}
}
