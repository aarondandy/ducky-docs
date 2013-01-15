using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class ComplexTextList : ComplexTextList<IComplexTextNode>
	{

		public ComplexTextList(IList<IComplexTextNode> nodes) : base(nodes) {
			Contract.Requires(nodes != null);
		}

	}

	public class ComplexTextList<TComplexTextNode> : IComplexTextNode where TComplexTextNode : IComplexTextNode
	{
		public ComplexTextList(IList<TComplexTextNode> nodes) {
			if (null == nodes) throw new ArgumentNullException("nodes");
			Contract.EndContractBlock();
			Children = new ReadOnlyCollection<TComplexTextNode>(nodes);
		}

		public bool HasChildren { get { return Children.Count > 0; } }

		public ReadOnlyCollection<TComplexTextNode> Children { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Children != null);
		}
	}

}
