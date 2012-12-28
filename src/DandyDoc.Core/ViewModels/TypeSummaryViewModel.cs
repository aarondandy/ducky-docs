using System;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class TypeSummaryViewModel : MemberSummaryViewModel<TypeDefinition>
	{

		public TypeSummaryViewModel(
			TypeDefinition definition,
			string displayName,
			string cref,
			ParsedXmlNodeBase summary
		) : base(definition,displayName,cref,summary){
			Contract.Requires(null != definition);
			Contract.Requires(!String.IsNullOrEmpty(displayName));
			Contract.Requires(!String.IsNullOrEmpty(cref));
		}

	}
}
