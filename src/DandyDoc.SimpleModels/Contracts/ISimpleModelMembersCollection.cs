using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModelMembersCollection
	{
		IList<ITypeSimpleModel> NestedTypes { get; }
		IList<IDelegateSimpleModel> NestedDelegates { get; }
		IList<IMethodSimpleModel> Constructors { get; }
		IList<IMethodSimpleModel> Methods { get; }
		IList<IMethodSimpleModel> Operators { get; }
		IList<IPropertySimpleModel> Properties { get; }
		IList<IFieldSimpleModel> Fields { get; }
		IList<IEventSimpleModel> Events { get; }
	}
}
