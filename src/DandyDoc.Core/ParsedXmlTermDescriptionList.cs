using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.Core
{
	public class ParsedXmlTermDescriptionList : ParsedXmlElementBase
	{

		public class ParsedTermDescription : ParsedXmlElementBase
		{
			public ParsedTermDescription(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
				Contract.Requires(null != element);
				Contract.Requires(null != relatedEntity);
			}

			public bool IsHeader{ get { return "LISTHEADER".Equals(Element.Name, StringComparison.OrdinalIgnoreCase); } }

			public ParsedXmlDoc Term { get { return SelectNodeAsXmlDoc("term"); } }

			public ParsedXmlDoc Description{ get { return SelectNodeAsXmlDoc("description"); } }

			private ParsedXmlDoc SelectNodeAsXmlDoc(string query){
				Contract.Requires(!String.IsNullOrEmpty(query));
				var selection = Element.SelectSingleNode(query);
				if (null == selection)
					return null;
				return new ParsedXmlDoc(selection, RelatedEntity);
			}
		}

		public ParsedXmlTermDescriptionList(XmlElement element, IDocumentableEntity relatedEntity) : base(element, relatedEntity) {
			Contract.Requires(null != element);
			Contract.Requires(null != relatedEntity);
		}

		public string ListType{
			get{
				var listTypeNode = Element.SelectSingleNode("@type");
				if (null == listTypeNode)
					return null;
				return listTypeNode.Value;
			}
		}

		public IEnumerable<ParsedTermDescription> AllRows{
			get{
				foreach (var node in Element.ChildNodes.OfType<XmlElement>()){
					if ("LISTHEADER".Equals(node.Name, StringComparison.OrdinalIgnoreCase) || "ITEM".Equals(node.Name, StringComparison.OrdinalIgnoreCase))
						yield return new ParsedTermDescription(node, RelatedEntity);
				}
			}
		}

		public IEnumerable<ParsedTermDescription> ListHeaders{
			get { return AllRows.Where(x => x.IsHeader); }
		}

		public IEnumerable<ParsedTermDescription> Items{
			get { return AllRows.Where(x => !x.IsHeader); }
		}

	}
}
