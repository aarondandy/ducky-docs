using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class FieldRecord : MemberRecord
	{

		internal FieldRecord(TypeRecord parentType, FieldDefinition fieldDefinition) : base(parentType, fieldDefinition) {
			Contract.Requires(null != parentType);
			Contract.Requires(null != fieldDefinition);
		}

		public FieldDefinition CoreDefinition { get { return (FieldDefinition)MemberDefinition; } }

		public TypeReference Type {
			get {
				return CoreDefinition.FieldType;
			}
		}

	}
}
