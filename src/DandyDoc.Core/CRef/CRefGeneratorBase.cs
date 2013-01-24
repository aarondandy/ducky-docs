namespace DandyDoc.CRef
{
	public abstract class CRefGeneratorBase : ICRefGenerator
	{

		protected CRefGeneratorBase(bool includeTypePrefix) {
			IncludeTypePrefix = includeTypePrefix;
		}

		public bool IncludeTypePrefix { get; protected set; }

		public abstract string GetCRef(object entity);

	}
}
