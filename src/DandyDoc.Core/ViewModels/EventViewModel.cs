using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class EventViewModel : DefinitionViewModelBase<EventDefinition>
	{

		public EventViewModel(EventDefinition definition, TypeViewModel typeViewModelContainer, XmlDocOverlay xmlDocOverlay, CRefOverlay cRefOverlay = null)
			: base(definition, typeViewModelContainer, xmlDocOverlay, cRefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		new public EventDefinitionXmlDoc XmlDoc { get { return (EventDefinitionXmlDoc)(base.XmlDoc); } }

		public override string SubTitle { get { return "Event"; } }

	}
}
