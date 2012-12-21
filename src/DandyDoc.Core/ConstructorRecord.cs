using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class ConstructorRecord : MethodRecord
	{

		internal ConstructorRecord(TypeRecord parentType, MethodDefinition methodDefinition)
			: base(parentType, methodDefinition)
		{
			Contract.Requires(null != parentType);
			Contract.Requires(null != methodDefinition);
		}

		public string FriendlyName {
			get { return ParentType.Name; }
		}

	}
}
