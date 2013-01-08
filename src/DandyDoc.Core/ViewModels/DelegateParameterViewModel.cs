using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class DelegateParameterViewModel : ParameterViewModelBase
	{

		internal DelegateParameterViewModel(ParameterDefinition definition, DelegateViewModel parent, ParsedXmlElementBase xmlDoc)
			: base(definition, xmlDoc)
		{
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.Requires(null != definition);
			Parent = parent;
		}

		public DelegateViewModel Parent { get; private set; }

		public override IEnumerable<MemberFlair> Flair {
			get {
				foreach (var item in base.Flair)
					yield return item;

				var name = Definition.Name;
				Contract.Assume(!String.IsNullOrEmpty(name));
				if (Parent.RequiresParameterNotNullOrEmpty(name))
					yield return new MemberFlair("no nulls","Null Values","Required: not null and not empty.");
				else if (Parent.RequiresParameterNotNull(name))
					yield return new MemberFlair("no nulls","Null Values","Required: not null.");
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parent != null);
		}

	}
}
