using System.Collections.Generic;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocGenericDefinition
	{

		bool HasGenericParameters { get; }

		IList<ICodeDocGenericParameter> GenericParameters { get; }

	}
}
