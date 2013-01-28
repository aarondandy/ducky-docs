using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
	public class XmlDocMember : XmlDocElement
	{

		private struct GutsData
		{
			public XmlDocElement FirstSummary;
			public IList<XmlDocNode> AllSummaryContents;
		}

		private readonly GutsData _guts;

		public XmlDocMember(XmlElement element, IEnumerable<XmlDocNode> children)
			: base(element, children)
		{
			Contract.Requires(element != null);
			Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
			_guts = CreateGuts();
		}

		private GutsData CreateGuts() {
			var result = new GutsData();
			var summaryContents = new List<XmlDocNode>();
			foreach (var child in Children.OfType<XmlDocElement>()) {
				if ("SUMMARY".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if (result.FirstSummary == null)
						result.FirstSummary = child;
					if(child.HasChildren)
						summaryContents.AddRange(child.Children);
				}
			}
			result.AllSummaryContents = new ReadOnlyCollection<XmlDocNode>(summaryContents.ToArray());
			return result;
		}

		public XmlDocElement SummaryElement {
			get { return _guts.FirstSummary; }
		}

		public IList<XmlDocNode> SummaryContents {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
				return _guts.AllSummaryContents;
			}
		}

		public bool HasSummaryContents {
			get { return SummaryContents.Count > 0; }
		}

	}
}
