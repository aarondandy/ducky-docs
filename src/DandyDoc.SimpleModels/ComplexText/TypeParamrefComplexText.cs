using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using System.Collections.Generic;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class TypeParamrefComplexText : ComplexTextList
	{

		public TypeParamrefComplexText(string parameterName, IList<IComplexTextNode> children)
			: base(children)
		{
			Contract.Requires(children != null);
			ParameterName = parameterName;
		}

		public string ParameterName { get; private set; }

	}

}
