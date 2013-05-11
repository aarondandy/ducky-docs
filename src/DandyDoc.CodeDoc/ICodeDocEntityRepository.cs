using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityRepository
    {

        ICodeDocEntityContent GetContentEntity(string cRef);

        ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef);

        ICodeDocEntity GetSimpleEntity(string cRef);

        ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef);

        IList<ICodeDocAssembly> Assemblies { get; }

        IList<ICodeDocNamespace> Namespaces { get; }

    }
}
