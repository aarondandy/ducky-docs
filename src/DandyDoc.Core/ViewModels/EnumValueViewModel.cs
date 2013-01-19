using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class EnumValueViewModel : FieldViewModel
	{

		public EnumValueViewModel(FieldDefinition definition, TypeViewModel typeViewModelContainer, XmlDocOverlay xmlDocOverlay, CRefOverlay cRefOverlay = null)
			: base(definition, typeViewModelContainer, xmlDocOverlay, cRefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
		}

		public override string Cref {
			get { return null; }
		}

	}
}
