﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class MethodReturnViewModel : ReturnViewModelBase
	{

		internal MethodReturnViewModel(TypeReference type, MethodViewModel parent, ParsedXmlElementBase xmlDoc)
			: base(type, xmlDoc)
		{
			if(null == parent) throw new ArgumentNullException("parent");
			Contract.Requires(null != type);
			Parent = parent;
		}

		public MethodViewModel Parent { get; private set; }

		public override IEnumerable<MemberFlair> Flair {
			get {
				if (Parent.Definition.HasAttributeMatchingName("CanBeNullAttribute"))
					yield return new MemberFlair("nulls", "Null Values", "Can return null.");
				
				if (Parent.EnsuresResultNotNullOrEmpty)
					yield return new MemberFlair("no nulls","Null Values", "Ensures: result is not null and not empty.");
				else if (Parent.EnsuresResultNotNull)
					yield return new MemberFlair("no nulls","Null Values", "Ensures: result is not null.");
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Parent != null);
		}

	}
}
