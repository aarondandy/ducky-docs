using System.Collections.Generic;
using System.Xml;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

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

        IList<XmlDocNode> Examples { get; }

		bool HasPermissions { get; }

        IList<XmlDocNode> Permissions { get; }

		bool HasRemarks { get; }

        IList<XmlDocNode> Remarks { get; }

		bool HasSeeAlso { get; }

        IList<XmlDocNode> SeeAlso { get; }

		bool HasSummary { get; }

        XmlDocNode Summary { get; }

	}
}
