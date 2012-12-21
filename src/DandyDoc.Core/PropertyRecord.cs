using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class PropertyRecord : MemberRecord
	{

		internal PropertyRecord(TypeRecord parentType, PropertyDefinition propertyDefinition) : base(parentType, propertyDefinition) {
			Contract.Requires(null != parentType);
			Contract.Requires(null != propertyDefinition);
		}

		public PropertyDefinition CoreDefinition { get { return (PropertyDefinition)MemberDefinition; } }

		public TypeReference Type {
			get {
				return CoreDefinition.PropertyType;
			}
		}

	}
}
