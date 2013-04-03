namespace DandyDoc.CodeDoc
{
    public interface ICodeDocField : ICodeDocEntityContent, ICodeDocValueEntity
    {
        bool IsLiteral { get; }

        bool IsInitOnly { get; }
    }
}
