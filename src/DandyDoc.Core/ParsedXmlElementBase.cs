using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Core
{
	public abstract class ParsedXmlElementBase : ParsedXmlNodeBase
	{

		protected ParsedXmlElementBase(XmlElement element, IDocumentableEntity relatedEntity) : base(element){
			if(null == relatedEntity) throw new ArgumentNullException("relatedEntity");
			Contract.Requires(null != element);
			RelatedEntity = relatedEntity;
		}

		public IDocumentableEntity RelatedEntity { get; private set; }

		public XmlElement Element { get { return (XmlElement)Node; } }

		public ParsedXmlDoc SubParts { get { return new ParsedXmlDoc(Element, RelatedEntity); } }

		public string OuterPrefix {
			get{
				var outer = RawXml;
				if (null == outer)
					return String.Empty;
				var inner = InnerXml;
				if (null == inner)
					return String.Empty;
				var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
				if (innerIndex <= 0)
					return String.Empty;
				return outer.Substring(0, innerIndex);
			}
		}

		public string OuterSuffix {
			get {
				var outer = RawXml;
				if (null == outer)
					return String.Empty;
				var inner = InnerXml;
				if (null == inner)
					return String.Empty;
				var innerIndex = outer.IndexOf(inner, StringComparison.Ordinal);
				if (innerIndex < 0)
					return String.Empty;
				var suffixStart = innerIndex + inner.Length;
				if (suffixStart >= outer.Length)
					return String.Empty;
				return outer.Substring(suffixStart);
			}
		}

	}
}
