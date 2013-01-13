using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class ComplexTextList : IComplexTextNode
	{

		public ComplexTextList(IList<IComplexTextNode> nodes){
			if(null == nodes) throw new ArgumentNullException("nodes");
			Contract.EndContractBlock();
			Children = new ReadOnlyCollection<IComplexTextNode>(nodes);
		}

		public bool HasChildren { get { return Children.Count > 0; } }

		public ReadOnlyCollection<IComplexTextNode> Children { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Children != null);
		}

	}
}
