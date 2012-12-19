namespace DandyDoc.Core
{
	public interface IParsedXmlPart
	{

		int StartIndex { get; }
		int Length { get; }
		string RawText { get; }

	}
}
