using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class GenericTypeParameterViewModel : GenericParameterViewModelBase
	{


		internal GenericTypeParameterViewModel(GenericParameter parameter, TypeViewModel parent)
			: base(parameter)
		{
			Contract.Requires(parameter != null);
			Contract.Requires(parent != null);
			Parent = parent;
		}

		public TypeViewModel Parent { get; private set; }

		public override ParsedXmlElementBase XmlDoc{
			get{
				if (!Parent.HasXmlDoc)
					return null;
				Contract.Assume(!String.IsNullOrEmpty(Parameter.Name));
				return Parent.XmlDoc.DocsForTypeparam(Parameter.Name);
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parent != null);
		}

	}
}
