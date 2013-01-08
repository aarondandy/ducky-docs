using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class ParameterViewModel
	{

		private static readonly DisplayNameOverlay FullNameOverlay = new DisplayNameOverlay {
			IncludeNamespaceForTypes = true,
			IncludeParameterNames = true,
			ShowGenericParametersOnDefinition = true,
			ShowTypeNameForMembers = true
		};

		internal ParameterViewModel(IParameterizedDefinitionViewModel parent, ParameterDefinition definition, ParsedXmlElementBase xmlDoc) {
			if(null == parent) throw new ArgumentNullException("parent");
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Parent = parent;
			Definition = definition;
			XmlDoc = xmlDoc;
		}

		public virtual IParameterizedDefinitionViewModel Parent { get; private set; }

		public virtual ParameterDefinition Definition { get; private set; }

		public virtual ParsedXmlElementBase XmlDoc { get; private set; }

		public virtual string DisplayName { get { return Definition.Name; } }

		public virtual bool HasXmlDoc { get { return XmlDoc != null; } }


		public virtual string TypeDisplayName{
			get{
				Contract.Assume(null != Definition.ParameterType);
				return FullNameOverlay.GetDisplayName(Definition.ParameterType);
			}
		}

		public virtual IEnumerable<MemberFlair> Flair {
			get {
				if(Definition.HasAttributeMatchingName("CanBeNullAttribute"))
					yield return new MemberFlair("null ok","Null Values", "This parameter can be null.");

				var name = Definition.Name;
				Contract.Assume(!String.IsNullOrEmpty(name));
				if (Parent.RequiresParameterNotNullOrEmpty(name))
					yield return new MemberFlair("no nulls", "Null Values", "Required: not null and not empty.");
				else if (Parent.RequiresParameterNotNull(name))
					yield return new MemberFlair("no nulls", "Null Values", "Required: not null.");

				if(Definition.HasAttributeMatchingName("InstantHandleAttribute"))
					yield return new MemberFlair("instant", "Usage", "Parameter is used only during method execution.");
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parent != null);
			Contract.Invariant(Definition != null);
		}

	}
}
