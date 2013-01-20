namespace DandyDoc.SimpleModels.Contracts
{
	public interface IPropertySimpleModel : ISimpleModel, IInvokableSimpleModel
	{

		ISimpleMemberPointerModel PropertyType { get; }

		bool HasValueDescription { get; }

		IComplexTextNode ValueDescription { get; }

		bool HasGetter { get; }

		IMethodSimpleModel Getter { get; }

		bool HasSetter { get; }

		IMethodSimpleModel Setter { get; }

	}
}
