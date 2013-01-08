using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class ReturnViewModel
	{

		private static readonly DisplayNameOverlay FullNameOverlay = new DisplayNameOverlay{
			IncludeNamespaceForTypes = true,
			IncludeParameterNames = true,
			ShowGenericParametersOnDefinition = true,
			ShowTypeNameForMembers = true
		};

		internal ReturnViewModel(IParameterizedDefinitionViewModel parent, ParsedXmlElementBase xmlDoc) {
			if (null == parent) throw new ArgumentNullException("parent");
			Contract.EndContractBlock();
			Parent = parent;
			XmlDoc = xmlDoc;
		}

		public IParameterizedDefinitionViewModel Parent { get; private set; }

		public virtual ParsedXmlElementBase XmlDoc { get; private set; }

		public virtual bool HasXmlDoc { get { return XmlDoc != null; } }

		public virtual TypeReference Type { get { return Parent.ReturnType; } }

		public virtual string TypeDisplayName { get { return FullNameOverlay.GetDisplayName(Type); } }

		public virtual IEnumerable<MemberFlair> Flair{
			get{
				if (Parent.CanReturnNull)
					yield return new MemberFlair("null result", "Null Values", "Can return null.");

				if (Parent.EnsuresResultNotNullOrEmpty)
					yield return new MemberFlair("no nulls", "Null Values", "Ensures: result is not null and not empty.");
				else if (Parent.EnsuresResultNotNull)
					yield return new MemberFlair("no nulls", "Null Values", "Ensures: result is not null.");
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Type != null);
			Contract.Invariant(Parent != null);
		}

	}
}
