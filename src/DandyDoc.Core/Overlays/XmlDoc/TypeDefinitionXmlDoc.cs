using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class TypeDefinitionXmlDoc : IDefinitionXmlDoc
	{

		internal TypeDefinitionXmlDoc(TypeDefinition typeDefinition, XmlNode xmlNode, CrefOverlay crefOverlay){
			TypeDefinition = typeDefinition;
			Node = xmlNode;
			CrefOverlay = crefOverlay;
		}

		public TypeDefinition TypeDefinition { get; private set; }

		public XmlNode Node { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public ParsedXmlNodeBase Summary {
			get { return SelectParsedXmlNode("summary"); }
		}

		public ParsedXmlNodeBase Remarks {
			get { return SelectParsedXmlNode("remarks"); }
		}

		public ParsedXmlNodeBase SelectParsedXmlNode(string query) {
			if(String.IsNullOrEmpty(query)) throw new ArgumentException("Invalid query.", "query");
			var node = Node.SelectSingleNode(query);
			return null == node
				? null
				: ParsedXmlNodeBase.Parse(node, CrefOverlay);
		}
	}
}
