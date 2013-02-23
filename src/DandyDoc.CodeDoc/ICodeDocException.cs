using System.Collections.Generic;
using System.Xml;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocException
	{

        CRefIdentifier ExceptionCRef { get; }

		bool HasConditions { get; }

		IList<XmlNodeList> Conditions { get; } 

		bool HasEnsures { get; }

		IList<XmlNodeList> Ensures { get; }

	}
}
