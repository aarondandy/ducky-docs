using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IPropertySimpleModel : ISimpleModel
	{

		ISimpleMemberPointerModel PropertyType { get; }

		bool HasValueDescription { get; }

		IComplexTextNode ValueDescription { get; }

		bool HasParameters { get; }

		IList<IParameterSimpleModel> Parameters { get; }

		bool HasExceptions { get; }

		IList<IExceptionSimpleModel> Exceptions { get; }

		bool HasGetter { get; }

		IMethodSimpleModel Getter { get; }

		bool HasSetter { get; }

		IMethodSimpleModel Setter { get; }

	}
}
