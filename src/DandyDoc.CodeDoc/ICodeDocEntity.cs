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

		bool HasSummary { get; }

        XmlDocNode Summary { get; }

	}
}
