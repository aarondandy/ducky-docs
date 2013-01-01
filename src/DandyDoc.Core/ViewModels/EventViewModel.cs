using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class EventViewModel : DefinitionViewModelBase<EventDefinition>
	{

		public EventViewModel(EventDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public EventDefinitionXmlDoc XmlDoc { get { return (EventDefinitionXmlDoc)(base.XmlDoc); } }

		public override string Title { get { return base.Title + " Event"; } }


	}
}
