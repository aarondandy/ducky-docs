using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityContent : ICodeDocEntity
    {

        bool HasExamples { get; }

        IList<XmlDocNode> Examples { get; }

        bool HasPermissions { get; }

        IList<XmlDocNode> Permissions { get; }

        bool HasRemarks { get; }

        IList<XmlDocNode> Remarks { get; }

        bool HasSeeAlso { get; }

        IList<XmlDocNode> SeeAlso { get; }

    }
}
