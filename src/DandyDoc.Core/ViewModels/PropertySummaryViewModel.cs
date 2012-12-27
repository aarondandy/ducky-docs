using System;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class PropertySummaryViewModel : MemberSummaryViewModel<PropertyDefinition>
	{

		public PropertySummaryViewModel(
			PropertyDefinition definition,
			string displayName,
			string cref,
			ParsedXmlNodeBase summary
		) : base(definition,displayName,cref,summary) {
			Contract.Requires(null != definition);
			Contract.Requires(!String.IsNullOrEmpty(displayName));
			Contract.Requires(!String.IsNullOrEmpty(cref));
		}

		public bool HasGet { get { return Definition.GetMethod != null; } }
		public bool HasSet { get { return Definition.SetMethod != null; } }

	}
}
