using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocDefinitionList : XmlDocElement
	{

		public XmlDocDefinitionList(XmlElement element, IEnumerable<XmlDocNode> children)
			: this(element.GetAttribute("type"), element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
		}

		public XmlDocDefinitionList(string listType, XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
			ListType = String.IsNullOrEmpty(listType) ? "table" : listType;
		}

		public string ListType { get; private set; }

		public IList<XmlDocDefinitionListItem> Items {
			get { return Children.OfType<XmlDocDefinitionListItem>().ToList(); }
		}

		public bool HasItems {
			get { return Items.Count > 0; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(!String.IsNullOrEmpty(ListType));
		}

	}
}
