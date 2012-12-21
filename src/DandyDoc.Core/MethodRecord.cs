using System;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class MethodRecord : MemberRecord
	{

		internal MethodRecord(TypeRecord parentType, MethodDefinition methodDefinition)
			: base(parentType, methodDefinition)
		{
			Contract.Requires(null != parentType);
			Contract.Requires(null != methodDefinition);
		}

		public MethodDefinition MethodDefinition { get { return (MethodDefinition)MemberDefinition; } }

		public override string Cref {
			get {
				var cref = base.Cref;
				var md = MethodDefinition;
				if (md.HasGenericParameters) {
					cref += "``" + md.GenericParameters.Count;
				}
				cref += '(' + String.Join(",",md.Parameters.Select(ToCrefTypeName)) + ')';
				return cref;
			}
		}

		private static string ToCrefTypeName(ParameterDefinition tr) {
			return NameUtilities.GetCref(tr.ParameterType);
		}

	}
}
