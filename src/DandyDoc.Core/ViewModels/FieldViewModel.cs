﻿using System.Collections.Generic;
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

		public override string SubTitle {
			get{
				if (Definition.HasConstant)
					return "Constant";
				return "Field";
			}
		}

		public ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}

		protected override IEnumerable<MemberFlair> GetFlairTags(){
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			if(Definition.HasConstant)
				yield return new MemberFlair("constant", "Value", "This field is a constant.");
		}

	}
}
