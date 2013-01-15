using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class MethodSimpleModel : DefinitionMemberSimpleModelBase<MethodDefinition>, IMethodSimpleModel
	{
		private readonly Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>> _genericParameters;

		public MethodSimpleModel(MethodDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
			_genericParameters = new Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>>(CreateGenericParameters, true);
		}

		private ReadOnlyCollection<IGenericParameterSimpleModel> CreateGenericParameters() {
			var results = new List<IGenericParameterSimpleModel>();
			if (Definition.HasGenericParameters) {
				var xmlDocs = MethodXmlDocs;
				foreach (var genericParameterDefinition in Definition.GenericParameters) {
					Contract.Assume(!String.IsNullOrEmpty(genericParameterDefinition.Name));
					IComplexTextNode summary = null;
					if (null != xmlDocs) {
						var summaryParsedXml = xmlDocs.DocsForTypeparam(genericParameterDefinition.Name);
						if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0){
							summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
						}
					}

					var constraints = new List<IGenericParameterConstraint>();
					if (genericParameterDefinition.HasNotNullableValueTypeConstraint)
						constraints.Add(new ValueTypeGenericConstraint());
					else if (genericParameterDefinition.HasReferenceTypeConstraint)
						constraints.Add(new ReferenceTypeGenericConstraint());

					var constraintReferenceTypes = (IEnumerable<TypeReference>)genericParameterDefinition.Constraints;
					if (genericParameterDefinition.HasNotNullableValueTypeConstraint)
						constraintReferenceTypes = constraintReferenceTypes.Where(x => x.FullName != "System.ValueType");
					var pointerConstraints = constraintReferenceTypes
						.Select(x => new MemberPointerGenericConstraint(new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(x), x)));
					constraints.AddRange(pointerConstraints);

					if (genericParameterDefinition.HasDefaultConstructorConstraint && !genericParameterDefinition.HasNotNullableValueTypeConstraint)
						constraints.Add(new DefaultConstructorGenericConstraint());

					results.Add(new DefinitionGenericParameterSimpleModel(genericParameterDefinition, summary, constraints));
				}
			}
			return new ReadOnlyCollection<IGenericParameterSimpleModel>(results);
		}

		protected MethodDefinitionXmlDoc MethodXmlDocs { get { return DefinitionXmlDocs as MethodDefinitionXmlDoc; } }

		public override string SubTitle {
			get {
				if (Definition.IsConstructor)
					return "Constructor";
				if (Definition.IsOperatorOverload())
					return "Operator";
				return "Method";
			}
		}

		public bool HasGenericParameters { get { return GenericParameters.Count > 0; } }

		public IList<IGenericParameterSimpleModel> GenericParameters { get { return _genericParameters.Value; } }

		public bool HasParameters {
			get { return Parameters.Count > 0; }
		}

		public IList<IParameterSimpleModel> Parameters {
			get {
				// TODO: extract the parameters
				return new IParameterSimpleModel[0];
			}
		}
	}
}

