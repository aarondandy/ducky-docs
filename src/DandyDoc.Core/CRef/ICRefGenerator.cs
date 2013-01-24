namespace DandyDoc.CRef
{
	public interface ICRefGenerator
	{

		string GetCRef(object entity);

		bool IncludeTypePrefix { get; }

	}
}
