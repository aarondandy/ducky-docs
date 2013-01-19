using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class FieldViewModel : DefinitionViewModelBase<FieldDefinition>
	{

		public FieldViewModel(FieldDefinition definition, TypeViewModel typeViewModelContainer, XmlDocOverlay xmlDocOverlay, CRefOverlay cRefOverlay = null)
			: base(definition, typeViewModelContainer, xmlDocOverlay, cRefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public FieldDefinitionXmlDoc XmlDoc { get { return (FieldDefinitionXmlDoc)(base.XmlDoc); } }

		public override string SubTitle { get{ return Definition.HasConstant ? "Constant" : "Field"; } }

		public virtual ParsedXmlElementBase ValueDoc { get { return null == XmlDoc ? null : XmlDoc.ValueDoc; } }

		public virtual bool HasValueDoc { get { return ValueDoc != null; } }

		public virtual TypeReference FieldType {
			get {
				Contract.Ensures(Contract.Result<TypeReference>() != null);
				Contract.Assume(Definition.FieldType != null);
				return Definition.FieldType;
			}
		}

		public virtual string FieldTypeDisplayName {
			get { return ShortNameOverlay.GetDisplayName(FieldType); }
		}

		protected override IEnumerable<MemberFlair> GetFlairTags(){
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			if(Definition.HasConstant)
				yield return new MemberFlair("constant", "Value", "This field is a constant.");
			if(Definition.IsInitOnly)
				yield return new MemberFlair("readonly", "Value", "This field is only assignable on initialization.");
		}

	}
}
