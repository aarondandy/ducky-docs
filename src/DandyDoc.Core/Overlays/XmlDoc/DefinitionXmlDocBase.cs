using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public abstract class DefinitionXmlDocBase
	{

		protected DefinitionXmlDocBase(IMemberDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay) {
			if(null == definition) throw new ArgumentNullException("definition");
			if(null == xmlNode) throw new ArgumentNullException("xmlNode");
			if(null == crefOverlay) throw new ArgumentNullException("crefOverlay");
			Contract.EndContractBlock();
			Definition = definition;
			Node = xmlNode;
			CrefOverlay = crefOverlay;
		}

		public IMemberDefinition Definition { get; private set; }

		public XmlNode Node { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public virtual ParsedXmlElementBase Summary {
			get { return (ParsedXmlElementBase)SelectParsedXmlNode("summary"); }
		}

		public virtual ParsedXmlElementBase Remarks {
			get { return (ParsedXmlElementBase)SelectParsedXmlNode("remarks"); }
		}

		public virtual IList<ParsedXmlElementBase> Examples {
			get { return SelectParsedXmlNodes("example").ConvertAll(n => (ParsedXmlElementBase)n); }
		}

		public virtual IList<ParsedXmlPermission> Permissions{
			get { return SelectParsedXmlNodes("permission").ConvertAll(n => (ParsedXmlPermission)n); }
		}

		public virtual IList<ParsedXmlSeeElement> SeeAlso{
			get { return SelectParsedXmlNodes("seealso").ConvertAll(n => (ParsedXmlSeeElement)n); }
		} 

		public virtual ParsedXmlNodeBase SelectParsedXmlNode(string query) {
			if (String.IsNullOrEmpty(query)) throw new ArgumentException("Invalid query.", "query");
			Contract.EndContractBlock();
			var node = Node.SelectSingleNode(query);
			return null == node
				? null
				: ParsedXmlNodeBase.Parse(node, this);
		}

		public virtual IList<ParsedXmlNodeBase> SelectParsedXmlNodes(string query) {
			if (String.IsNullOrEmpty(query)) throw new ArgumentException("Invalid query.", "query");
			Contract.EndContractBlock();
			var nodes = Node.SelectNodes(query);
			if (null == nodes)
				return null;
			return nodes
				.Cast<XmlNode>()
				.Select(n => ParsedXmlNodeBase.Parse(n, this))
				.ToList();
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != Node);
			Contract.Invariant(null != CrefOverlay);
		}

	}
}
