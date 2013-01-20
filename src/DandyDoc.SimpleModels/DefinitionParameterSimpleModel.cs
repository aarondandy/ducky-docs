using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class DefinitionParameterSimpleModel : IParameterSimpleModel
	{

		public DefinitionParameterSimpleModel(ParameterDefinition parameter, ISimpleMemberPointerModel parameterType, IComplexTextNode summary){
			if(parameter == null) throw new ArgumentNullException("parameter");
			if(parameterType == null) throw new ArgumentNullException("parameterType");
			Contract.EndContractBlock();
			Parameter = parameter;
			Type = parameterType;
			Summary = summary;

		}

		public ParameterDefinition Parameter { get; private set; }

		public IComplexTextNode DisplayName {
			get { return new StandardComplexText(Parameter.Name); }
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
