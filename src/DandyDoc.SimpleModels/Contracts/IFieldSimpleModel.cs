namespace DandyDoc.SimpleModels.Contracts
{
	public interface IFieldSimpleModel : ISimpleModel
	{

		ISimpleMemberPointerModel FieldType { get; }

		bool HasValueDescription { get; }

		IComplexTextNode ValueDescription { get; }

	}
}
