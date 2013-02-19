using System.Collections.Generic;
using System.Xml;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocException
	{

		string ExceptionCRef { get; }

		bool HasConditions { get; }

		IList<XmlNodeList> Conditions { get; } 

		bool HasEnsures { get; }

		IList<XmlNodeList> Ensures { get; }

	}
}
