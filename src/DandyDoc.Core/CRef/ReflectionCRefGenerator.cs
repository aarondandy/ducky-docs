using System;

namespace DandyDoc.CRef
{
	public class ReflectionCRefGenerator : CRefGeneratorBase
	{

		public ReflectionCRefGenerator()
			: this(true) { }

		public ReflectionCRefGenerator(bool includeTypePrefix)
			: base(includeTypePrefix) { }

		public override string GetCRef(object entity) {
			throw new NotImplementedException();
		}

	}
}
