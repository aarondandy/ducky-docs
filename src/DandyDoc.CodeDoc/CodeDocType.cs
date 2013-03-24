using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public class CodeDocType : CodeDocEntityContentBase, ICodeDocType
	{

		public CodeDocType(CRefIdentifier cRef) : base(cRef){
			Contract.Requires(cRef != null);
		}

		public bool HasBaseChain { get { return BaseChainCRefs.Count > 0; } }

        public IList<CRefIdentifier> BaseChainCRefs {
			get{
                Contract.Ensures(Contract.Result<IList<CRefIdentifier>>() != null);
				throw new NotImplementedException();
			}
		}

		public bool HasDirectInterfaces { get { return DirectInterfaceCRefs.Count > 0; } }

        public IList<CRefIdentifier> DirectInterfaceCRefs {
			get{
                Contract.Ensures(Contract.Result<IList<CRefIdentifier>>() != null);
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
