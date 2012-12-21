using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class NestedTypeRecord : TypeRecord
	{

		internal NestedTypeRecord(TypeRecord parentType, TypeDefinition core)
			: base(parentType.Parent, core) {
			Contract.Requires(null != parentType);
			Contract.Requires(null != core);
			ParentType = parentType;
		}

		public TypeRecord ParentType { get; private set; }

		public override string Cref {
			get { return ParentType.Cref + '.' + Name; }
		}

	}
}
