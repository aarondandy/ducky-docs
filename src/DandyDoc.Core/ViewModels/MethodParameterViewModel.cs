using System;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class MethodParameterViewModel : ParameterViewModel
	{

		internal MethodParameterViewModel(ParameterDefinition definition, MethodViewModel parent, ParsedXmlElementBase xmlDoc)
			: base(definition, xmlDoc)
		{
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.Requires(null != definition);
			Parent = parent;
		}

		public MethodViewModel Parent { get; private set; }

		public override string RequiresQuickSummary{
			get{
				var name = Definition.Name;
				Contract.Assume(!String.IsNullOrEmpty(name));
				if (Parent.RequiresParameterNotNullOrEmpty(name))
					return "not null and not empty";
				if (Parent.RequiresParameterNotNull(name))
					return "not null";
				return null;
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parent != null);
		}

	}
}
