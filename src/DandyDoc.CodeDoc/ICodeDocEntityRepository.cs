using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEntityRepository
    {

        ICodeDocEntityContent GetEntity(CRefIdentifier cRef);

    }
}
