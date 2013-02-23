using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityRepository
    {

        ICodeDocEntity GetEntity(string cRef);

        ICodeDocEntity GetEntity(CRefIdentifier cRef);

    }
}
