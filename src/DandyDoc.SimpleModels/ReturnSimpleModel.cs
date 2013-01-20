using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class ReturnSimpleModel : IParameterSimpleModel
	{

		public ReturnSimpleModel(ISimpleMemberPointerModel parameterType, IComplexTextNode summary) {
			if(parameterType == null) throw new ArgumentNullException("parameterType");
			Contract.EndContractBlock();
			Type = parameterType;
			Summary = summary;

		}

		public IComplexTextNode DisplayName {
			get { return Type.Description; }
		}

		public bool HasSummary {
			get { return Summary != null; }
		}

		public IComplexTextNode Summary { get; private set; }

		public ISimpleMemberPointerModel Type { get; private set; }

		public bool HasFlair {
			get { return Flair.Count > 0; }
		}

		public IList<IFlairTag> Flair {
			get { return new IFlairTag[0]; }
		}

	}
}
