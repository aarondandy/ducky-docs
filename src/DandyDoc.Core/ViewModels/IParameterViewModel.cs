using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public interface IParameterViewModel
	{

		string DisplayName { get; }

		ParsedXmlElementBase XmlDoc { get; }

		bool HasXmlDoc { get; }

		ParameterDefinition Definition { get; }

		IDefinitionViewModel Parent { get; }

		string RequiresQuickSummary { get; }

	}
}
