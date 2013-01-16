using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IExceptionSimpleModel
	{

		ISimpleMemberPointerModel ExceptionType { get; }

		bool HasConditions { get; }

		IList<IComplexTextNode> Conditions { get; }

		bool HasEnsures { get; }

		IList<IComplexTextNode> Ensures { get; } 

	}
}
