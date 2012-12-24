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

		public virtual ParsedXmlNodeBase Summary {
			get { return SelectParsedXmlNode("summary"); }
		}

		public virtual ParsedXmlNodeBase Remarks {
			get { return SelectParsedXmlNode("remarks"); }
		}

		public virtual ParsedXmlNodeBase SelectParsedXmlNode(string query) {
			if (String.IsNullOrEmpty(query)) throw new ArgumentException("Invalid query.", "query");
			Contract.EndContractBlock();
			var node = Node.SelectSingleNode(query);
			return null == node
				? null
				: ParsedXmlNodeBase.Parse(node, this);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != Node);
			Contract.Invariant(null != CrefOverlay);
		}

	}
}
