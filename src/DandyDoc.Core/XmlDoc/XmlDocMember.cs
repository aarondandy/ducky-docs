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
			public XmlDocElement FirstReturnsSummary;
			public IList<XmlDocNode> AllReturnsSummary;
			public XmlDocElement FirstValueSummary;
			public IList<XmlDocNode> AllValueSummary; 
			public IList<XmlDocElement> RemarksElements;
			public IList<XmlDocRefElement> ExceptionElements;
			public IList<XmlDocNameElement> ParameterSummaries;
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
			var returnsContents = new List<XmlDocNode>();
			var valueContents = new List<XmlDocNode>();
			var remarksElements = new List<XmlDocElement>();
			var exceptionElements = new List<XmlDocRefElement>();
			var parameterSummaries = new List<XmlDocNameElement>();
			foreach (var child in Children.OfType<XmlDocElement>()) {
				if ("SUMMARY".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if (child.HasChildren) {
						if (result.FirstSummary == null)
							result.FirstSummary = child;
						summaryContents.AddRange(child.Children);
					}
				}
				else if ("RETURNS".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if (child.HasChildren) {
						if (result.FirstReturnsSummary == null)
							result.FirstReturnsSummary = child;
						returnsContents.AddRange(child.Children);
					}
				}
				else if ("VALUE".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if (child.HasChildren) {
						if (result.FirstValueSummary == null)
							result.FirstValueSummary = child;
						valueContents.AddRange(child.Children);
					}
				}
				else if ("REMARKS".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if (child.HasChildren)
						remarksElements.Add(child);
				}
				else if ("EXCEPTION".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
					if(child is XmlDocRefElement)
						exceptionElements.Add((XmlDocRefElement)child);
				}
				else if (child is XmlDocNameElement) {
					if("PARAM".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
						parameterSummaries.Add((XmlDocNameElement)child);
				}
			}
			result.AllSummaryContents = new ReadOnlyCollection<XmlDocNode>(summaryContents.ToArray());
			result.AllReturnsSummary = new ReadOnlyCollection<XmlDocNode>(returnsContents.ToArray());
			result.AllValueSummary = new ReadOnlyCollection<XmlDocNode>(valueContents.ToArray());
			result.RemarksElements = new ReadOnlyCollection<XmlDocElement>(remarksElements.ToArray());
			result.ExceptionElements = new ReadOnlyCollection<XmlDocRefElement>(exceptionElements.ToArray());
			result.ParameterSummaries = new ReadOnlyCollection<XmlDocNameElement>(parameterSummaries.ToArray());
			return result;
		}

		public XmlDocElement SummaryElement {
			get {
				Contract.Ensures(
					Contract.Result<XmlDocElement>() == null
					|| Contract.Result<XmlDocElement>().HasChildren
				);
				return _guts.FirstSummary;
			}
		}

		public IList<XmlDocNode> SummaryContents {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				return _guts.AllSummaryContents;
			}
		}

		public bool HasSummaryContents {
			get { return SummaryContents.Count > 0; }
		}

		public XmlDocElement ReturnsElement {
			get {
				Contract.Ensures(
					Contract.Result<XmlDocElement>() == null
					|| Contract.Result<XmlDocElement>().HasChildren
				);
				return _guts.FirstReturnsSummary;
			}
		}

		public IList<XmlDocNode> ReturnsContents {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				return _guts.AllReturnsSummary;
			}
		}

		public bool HasReturnsContents {
			get { return ReturnsContents.Count > 0; }
		}

		public XmlDocElement ValueElement {
			get {
				Contract.Ensures(
					Contract.Result<XmlDocElement>() == null
					|| Contract.Result<XmlDocElement>().HasChildren
				);
				return _guts.FirstValueSummary;
			}
		}

		public IList<XmlDocNode> ValueContents {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				return _guts.AllValueSummary;
			}
		}

		public bool HasValueContents {
			get { return ValueContents.Count > 0; }
		}

		public IList<XmlDocElement> RemarksElements {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null && x.HasChildren));
				return _guts.RemarksElements;
			}
		}

		public bool HasRemarksElements { get { return RemarksElements.Count > 0; } }

		public IList<XmlDocRefElement> ExceptionElements {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
				return _guts.ExceptionElements;
			}
		}

		public bool HasExceptionElements { get { return ExceptionElements.Count > 0; } }

		public bool HasParameterSummaries { get { return ParameterSummaries.Count > 0; } }

		public IList<XmlDocNameElement> ParameterSummaries {
			get {
				Contract.Ensures(Contract.Result<IList<XmlDocNameElement>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNameElement>>(), x => x != null && x.HasChildren));
				return _guts.ParameterSummaries;
			}
		}

		public XmlDocNameElement GetParameterSummary(string parameterName) {
			if(String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			return ParameterSummaries.FirstOrDefault(x => x.TargetName == parameterName);
		}

	}
}
