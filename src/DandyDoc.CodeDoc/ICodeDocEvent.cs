namespace DandyDoc.CodeDoc
{
    public interface ICodeDocEvent : ICodeDocEntityContent
    {

        ICodeDocEntity DelegateType { get; }

    }
}
