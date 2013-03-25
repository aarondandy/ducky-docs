using System.Collections.Generic;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityContent : ICodeDocEntity
    {

        bool HasExamples { get; }

        IList<XmlDocElement> Examples { get; }

        bool HasPermissions { get; }

        IList<XmlDocRefElement> Permissions { get; }

        bool HasRemarks { get; }

        IList<XmlDocElement> Remarks { get; }

        bool HasSeeAlso { get; }

        IList<XmlDocRefElement> SeeAlso { get; }

    }
}
