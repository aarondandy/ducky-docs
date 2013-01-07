using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class FieldViewModel : DefinitionViewModelBase<FieldDefinition>
	{

		public FieldViewModel(FieldDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public FieldDefinitionXmlDoc XmlDoc { get { return (FieldDefinitionXmlDoc)(base.XmlDoc); } }

		public override string SubTitle { get{ return Definition.HasConstant ? "Constant" : "Field"; } }

		public virtual ParsedXmlElementBase ValueDoc { get { return null == XmlDoc ? null : XmlDoc.ValueDoc; } }

		public virtual bool HasValueDoc { get { return ValueDoc != null; } }

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
