using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public interface IReturnViewModel
	{

		ParsedXmlElementBase XmlDoc { get; }

		bool HasXmlDoc { get; }

		TypeReference Type { get; }

		IDefinitionViewModel Parent { get; }

		string EnsuresQuickSummary { get; }

	}
}
