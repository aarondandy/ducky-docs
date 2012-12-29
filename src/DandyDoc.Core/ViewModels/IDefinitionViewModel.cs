using System.Collections.Generic;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public interface IDefinitionViewModel
	{

		IMemberDefinition Definition { get; }

		ParsedXmlElementBase Summary { get; }

		ParsedXmlElementBase Remarks { get; }

		IList<ParsedXmlElementBase> Examples { get; }

		IList<ParsedXmlSeeElement> SeeAlso { get; }

		string Title { get; }

		string ShortName { get; }

		string Cref { get; }

		IList<string> Flair { get; }

	}
}
