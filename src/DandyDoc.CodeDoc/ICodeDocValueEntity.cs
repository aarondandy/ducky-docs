using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocValueEntity
    {

        ICodeDocEntity ValueType { get; }

        bool HasValueDescription { get; }

        XmlDocElement ValueDescription { get; }

        bool HasValueDescriptionContents { get; }

        IList<XmlDocNode> ValueDescriptionContents { get; }

    }
}
