using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class GenericParameterMethodViewModel : GenericParameterViewModelBase
	{

		internal GenericParameterMethodViewModel(GenericParameter parameter, MethodViewModel parent)
			: base(parameter)
		{
			Contract.Requires(parameter != null);
			Contract.Requires(parent != null);
			Parent = parent;
		}

		public MethodViewModel Parent { get; private set; }

		public override ParsedXmlElementBase XmlDoc{
			get{
				if (!Parent.HasXmlDoc)
					return null;
				Contract.Assume(!String.IsNullOrEmpty(Parameter.Name));
				return Parent.MethodXmlDoc.DocsForTypeparam(Parameter.Name);
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parent != null);
		}

	}
}
