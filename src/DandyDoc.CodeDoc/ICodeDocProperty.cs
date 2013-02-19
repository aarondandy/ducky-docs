namespace DandyDoc.CodeDoc
{
	public interface ICodeDocProperty : ICodeDocEntity, ICodeDocInvokable, ICodeDocValueEntity
	{

		bool HasGetter { get; }

		ICodeDocMethod Getter { get; }

		bool HasSetter { get; }

		ICodeDocMethod Setter { get; }

	}
}
