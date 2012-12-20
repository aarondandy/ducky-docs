using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlSeePart : ParsedXmlElementBase
	{

		public ParsedXmlSeePart(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

		public string CrefName {
			get {
				Contract.Assume(null != Element.Attributes);
				var crefAttribute = Element.Attributes["cref"];
				if (null == crefAttribute)
					return null;
				return crefAttribute.Value;
			}
		}

		public IDocumentableEntity CrefTarget {
			get {
				if (null == RelatedEntity)
					return null;
				var crefName = CrefName;
				if (String.IsNullOrEmpty(crefName))
					return null;
				return RelatedEntity.ResolveCref(crefName);
			}
		}

		public string QuickLabel{
			get{
				if (String.IsNullOrWhiteSpace(InnerXml)){
					var target = CrefTarget;
					return null == target ? CrefName : target.Name;
				}
				return InnerXml;
			}
		}

	}
}
