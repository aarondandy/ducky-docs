using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocProperty : ICodeDocEntityContent, ICodeDocValueEntity
    {

        bool HasParameters { get; }

        IList<ICodeDocParameter> Parameters { get; }

        bool HasGetter { get; }

        ICodeDocMethod Getter { get; }

        bool HasSetter { get; }

        ICodeDocMethod Setter { get; }

    }
}
