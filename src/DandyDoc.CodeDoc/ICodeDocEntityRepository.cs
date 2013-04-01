using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityRepository
    {

        ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef);

        ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef);

    }
}
