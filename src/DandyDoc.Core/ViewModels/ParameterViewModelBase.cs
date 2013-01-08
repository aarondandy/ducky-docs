using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class ParameterViewModelBase
	{

		private static readonly DisplayNameOverlay FullNameOverlay = new DisplayNameOverlay {
			IncludeNamespaceForTypes = true,
			IncludeParameterNames = true,
			ShowGenericParametersOnDefinition = true,
			ShowTypeNameForMembers = true
		};

		internal ParameterViewModelBase(ParameterDefinition definition, ParsedXmlElementBase xmlDoc) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Definition = definition;
			XmlDoc = xmlDoc;
		}

		public virtual string DisplayName { get { return Definition.Name; } }

		public virtual ParsedXmlElementBase XmlDoc { get; private set; }

		public virtual bool HasXmlDoc { get { return XmlDoc != null; } }

		public virtual ParameterDefinition Definition { get; private set; }

		public virtual string TypeDisplayName { get { return FullNameOverlay.GetDisplayName(Definition.ParameterType); } }

		public virtual IEnumerable<MemberFlair> Flair {
			get {
				if(Definition.HasAttributeMatchingName("CanBeNullAttribute"))
					yield return new MemberFlair("nulls","Null Values", "This parameter can be null.");
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Definition != null);
		}

	}
}
