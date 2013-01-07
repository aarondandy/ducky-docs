using System;
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

		internal ParameterViewModel(ParameterDefinition definition, ParsedXmlElementBase xmlDoc) {
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

		public virtual string RequiresQuickSummary { get { return null; } }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Definition != null);
		}

	}
}
