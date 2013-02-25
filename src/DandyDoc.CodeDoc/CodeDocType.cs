using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public class CodeDocType : CodeDocEntityBase, ICodeDocType
	{

		public CodeDocType(CRefIdentifier cRef) : base(cRef){
			Contract.Requires(cRef != null);
		}

		public bool HasBaseChain { get { return BaseChainCRefs.Count > 0; } }

		public IList<string> BaseChainCRefs {
			get{
				Contract.Ensures(Contract.Result<IList<string>>() != null);
				throw new NotImplementedException();
			}
		}

		public bool HasDirectInterfaces { get { return DirectInterfaceCRefs.Count > 0; } }

		public IList<string> DirectInterfaceCRefs {
			get{
				Contract.Ensures(Contract.Result<IList<string>>() != null);
				throw new NotImplementedException();
			}
		}

		public bool HasGenericParameters { get { return GenericParameters.Count > 0; } }

		public IList<ICodeDocGenericParameter> GenericParameters {
			get{
				Contract.Ensures(Contract.Result<IList<ICodeDocGenericParameter>>() != null);
				throw new NotImplementedException();
			}
		}
	}
}
