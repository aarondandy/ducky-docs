using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocParameter
    {

        string Name { get; }

        CRefIdentifier TypeCRef { get; }

        bool HasSummary { get; }

        XmlDocNameElement Summary { get; }

        bool IsOut { get; }

        bool IsByRef { get; }

    }
}
