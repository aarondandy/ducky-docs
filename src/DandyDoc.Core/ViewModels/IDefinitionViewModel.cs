using System.Collections.Generic;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public interface IDefinitionViewModel
	{

		IMemberDefinition Definition { get; }

		ParsedXmlElementBase Summary { get; }

		bool HasSummary { get; }

		ParsedXmlElementBase Remarks { get; }

		bool HasRemarks { get; }

		IList<ParsedXmlElementBase> Examples { get; }

		bool HasExamples { get; }

		IList<ParsedXmlSeeElement> SeeAlso { get; }

		bool HasSeeAlso { get; }

		string Title { get; }

		string ShortName { get; }

		string Cref { get; }

		IList<string> Flair { get; }

	}
}
