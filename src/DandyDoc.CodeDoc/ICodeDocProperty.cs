namespace DandyDoc.CodeDoc
{
    public interface ICodeDocProperty : ICodeDocEntityContent, ICodeDocInvokable, ICodeDocValueEntity
	{

		bool HasGetter { get; }

		ICodeDocMethod Getter { get; }

		bool HasSetter { get; }

		ICodeDocMethod Setter { get; }

	}
}
