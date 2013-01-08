using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public abstract class ReturnViewModelBase
	{

		private static readonly DisplayNameOverlay FullNameOverlay = new DisplayNameOverlay{
			IncludeNamespaceForTypes = true,
			IncludeParameterNames = true,
			ShowGenericParametersOnDefinition = true,
			ShowTypeNameForMembers = true
		};

		internal ReturnViewModelBase(TypeReference type, ParsedXmlElementBase xmlDoc) {
			if(null == type) throw new ArgumentNullException("type");
			Contract.EndContractBlock();
			Type = type;
			XmlDoc = xmlDoc;
		}

		public virtual ParsedXmlElementBase XmlDoc { get; private set; }

		public virtual bool HasXmlDoc { get { return XmlDoc != null; } }

		public virtual TypeReference Type { get; private set; }

		public virtual string TypeDisplayName { get { return FullNameOverlay.GetDisplayName(Type); } }

		public abstract IEnumerable<MemberFlair> Flair { get; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Type != null);
		}

	}
}
