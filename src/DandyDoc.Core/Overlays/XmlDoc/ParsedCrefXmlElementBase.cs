using System;
using System.Diagnostics.Contracts;
using System.Xml;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public abstract class ParsedCrefXmlElementBase : ParsedXmlElementBase
	{

		protected ParsedCrefXmlElementBase(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

		public virtual string Cref {
			get {
				var crefAttribute = Element.Attributes["cref"];
				return null == crefAttribute ? null : crefAttribute.Value;
			}
		}

		public MemberReference CrefTarget {
			get {
				var cref = Cref;
				return String.IsNullOrEmpty(cref) ? null : CrefOverlay.GetMemberReference(cref);
			}
		}

	}
}
