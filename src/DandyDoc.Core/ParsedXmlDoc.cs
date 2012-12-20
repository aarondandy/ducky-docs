using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DandyDoc.Core.Utility;

namespace DandyDoc.Core
{
	public class ParsedXmlDoc
	{

		public ParsedXmlDoc(XmlNode node, IDocumentableEntity boundEntity){
			Node = node;
			BoundEntity = boundEntity;
		}

		public XmlNode Node { get; private set; }
		public string OuterXml { get { return null == Node ? null : Node.OuterXml; } }
		public string InnerXml { get { return null == Node ? null : Node.InnerXml; } }
		public string NormalizedOuterXml { get { return TextUtilities.ExtractIndentedNormalizedInnerText(OuterXml ?? String.Empty); } }
		public string NormalizedInnerXml { get { return TextUtilities.ExtractIndentedNormalizedInnerText(InnerXml ?? String.Empty); } }
		public IDocumentableEntity BoundEntity { get; private set; }

		public IList<IParsedXmlPart> ParsedNormalized {
			get{
				var results = new List<IParsedXmlPart>();
				if (null == Node)
					return results;
				foreach (var child in Node.ChildNodes.Cast<XmlNode>()){
					if(child is XmlText)
						results.Add(new ParsedXmlTextPart(child));
					else if(child is XmlElement)
						results.Add(ParseSubElement(child as XmlElement));
					else
						throw new NotImplementedException();
				}

				return results;
			}
		}

		private IParsedXmlPart ParseSubElement(XmlElement element) {
			if ("SEE".Equals(element.Name, StringComparison.OrdinalIgnoreCase)) {
				return new ParsedXmlSeePart(element, BoundEntity);
			}
			if ("C".Equals(element.Name, StringComparison.OrdinalIgnoreCase)) {
				return new ParsedXmlInlineCode(element, BoundEntity);
			}
			if ("CODE".Equals(element.Name, StringComparison.OrdinalIgnoreCase)) {
				return new ParsedXmlCodeBlock(element, BoundEntity);
			}
			if ("LIST".Equals(element.Name, StringComparison.OrdinalIgnoreCase)){
				return new ParsedXmlTermDescriptionList(element, BoundEntity);
			}
			if ("PARA".Equals(element.Name, StringComparison.OrdinalIgnoreCase)){
				return new ParsedXmlParagraph(element, BoundEntity);
			}
			return new ParsedXmlBasicElementPart(element, BoundEntity);
		}

	}
}
