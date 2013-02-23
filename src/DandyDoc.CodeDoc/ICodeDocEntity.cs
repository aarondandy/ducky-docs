using System.Collections.Generic;
using System.Xml;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocEntity
	{

		string Title { get; }

		string SubTitle { get; }

		string ShortName { get; }

		string FullName { get; }

        CRefIdentifier CRef { get; }

		string NamespaceName { get; }

		bool HasExamples { get; }

		IList<XmlNodeList> Examples { get; }

		bool HasPermissions { get; }

		IList<XmlNodeList> Permissions { get; }

		bool HasRemarks { get; }

		IList<XmlNodeList> Remarks { get; }

		bool HasSeeAlso { get; }

		IList<XmlNodeList> SeeAlso { get; }

		bool HasSummary { get; }

		XmlNodeList Summary { get; }

	}
}
