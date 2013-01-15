using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class DefinitionGenericParameterSimpleModel : IGenericParameterSimpleModel
	{

		public DefinitionGenericParameterSimpleModel(GenericParameter parameter, IComplexTextNode summary, IList<IGenericParameterConstraint> constraints) {
			if (parameter == null) throw new ArgumentNullException("parameter");
			if (constraints == null) throw new ArgumentNullException("constraints");
			Contract.EndContractBlock();
			Parameter = parameter;
			Constraints = new ReadOnlyCollection<IGenericParameterConstraint>(constraints);
			Summary = summary;
		}

		protected GenericParameter Parameter { get; private set; }

		public bool HasConstraints {
			get { return Constraints.Count > 0; }
		}

		public IList<IGenericParameterConstraint> Constraints { get; private set; }

		public string DisplayName {
			get { return Parameter.Name; }
		}

		public bool HasSummary {
			get { return Summary != null; }
		}

		public IComplexTextNode Summary { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Parameter != null);
		}

		public bool IsContravariant {
			get { return Parameter.IsContravariant; }
		}

		public bool IsCovariant {
			get { return Parameter.IsCovariant; }
		}
	}
}
