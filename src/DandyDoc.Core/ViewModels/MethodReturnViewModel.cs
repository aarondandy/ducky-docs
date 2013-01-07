using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class MethodReturnViewModel : ReturnViewModel
	{

		internal MethodReturnViewModel(TypeReference type, MethodViewModel parent, ParsedXmlElementBase xmlDoc)
			: base(type, xmlDoc)
		{
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.Requires(null != type);
			Parent = parent;
		}

		public MethodViewModel Parent { get; private set; }

		public override string EnsuresQuickSummary{
			get {
				if (Parent.EnsuresResultNotNullOrEmpty)
					return "not null and not empty";
				if (Parent.EnsuresResultNotNull)
					return "not null";
				return null;
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Parent != null);
		}

	}
}
