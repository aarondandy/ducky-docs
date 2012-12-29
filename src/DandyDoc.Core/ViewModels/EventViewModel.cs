using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.ExternalVisibility;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
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

		public override string Title { get { return Definition.Name + " Event"; } }

		public override string ShortName { get { return Definition.Name; } }

		protected override IEnumerable<string> GetFlairTags() {
			foreach (var item in base.GetFlairTags())
				yield return item;
		}

	}
}
