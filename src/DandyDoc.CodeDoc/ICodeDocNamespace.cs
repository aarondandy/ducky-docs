using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocNamespace : ICodeDocEntityContent
    {

        IList<ICodeDocEntity> Types { get; }

        IList<ICodeDocAssembly> Assemblies { get; }

    }
}
