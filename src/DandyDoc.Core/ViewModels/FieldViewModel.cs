using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
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

		public override string Title { get { return Definition.Name + " Field"; } }

		public override string ShortName { get { return Definition.Name; } }

		public ParsedXmlElementBase ValueDoc {
			get { return null == XmlDoc ? null : XmlDoc.ValueDoc; }
		}


	}
}
