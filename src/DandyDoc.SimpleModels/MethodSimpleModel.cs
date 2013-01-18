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

		private readonly MethodDefinitionXmlDoc _methodDefinitionXmlDocOverride;
		private readonly Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>> _genericParameters;
		private readonly Lazy<ReadOnlyCollection<IParameterSimpleModel>> _parameters;
		private readonly Lazy<IParameterSimpleModel> _return;
		private readonly Lazy<ReadOnlyCollection<IExceptionSimpleModel>> _exceptions;
		private readonly Lazy<ReadOnlyCollection<IContractConditionSimpleModel>> _ensures;
		private readonly Lazy<ReadOnlyCollection<IContractConditionSimpleModel>> _requires;

		public MethodSimpleModel(MethodDefinition definition, ITypeSimpleModel declaringModel, MethodDefinitionXmlDoc xmlDocOverride = null)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
			_genericParameters = new Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>>(CreateGenericParameters, true);
			_parameters = new Lazy<ReadOnlyCollection<IParameterSimpleModel>>(CreateParameters, true);
			_return = new Lazy<IParameterSimpleModel>(CreateReturn, true);
			_exceptions = new Lazy<ReadOnlyCollection<IExceptionSimpleModel>>(CreateExceptions, true);
			_ensures = new Lazy<ReadOnlyCollection<IContractConditionSimpleModel>>(CreateEnsures, true);
			_requires = new Lazy<ReadOnlyCollection<IContractConditionSimpleModel>>(CreateRequires, true);
			_methodDefinitionXmlDocOverride = xmlDocOverride;
		}

		private ReadOnlyCollection<IContractConditionSimpleModel> CreateRequires() {
			var results = new List<IContractConditionSimpleModel>();
			var xmlDocs = MethodXmlDocs;
			if (null != xmlDocs) {
				var xmlRequires = xmlDocs.Requires;
				if (null != xmlRequires) {
					foreach (var xmlRequired in xmlRequires) {
						if (xmlRequired.Children.Count > 0) {
							var exceptionCref = xmlRequired.ExceptionCref;
							var exceptionType = String.IsNullOrEmpty(exceptionCref) ? null : new CrefSimpleMemberPointer(exceptionCref);
							var description = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(xmlRequired.Children);
							Contract.Assume(description != null);
							results.Add(new ContractConditionSimpleModel(description, exceptionType));
						}
					}
				}
			}
			return new ReadOnlyCollection<IContractConditionSimpleModel>(results);
		}

		private ReadOnlyCollection<IContractConditionSimpleModel> CreateEnsures() {
			var results = new List<IContractConditionSimpleModel>();
			var xmlDocs = MethodXmlDocs;
			if (null != xmlDocs) {
				var xmlEnsures = xmlDocs.Ensures;
				if (null != xmlEnsures) {
					foreach (var xmlEnsured in xmlEnsures) {
						if (xmlEnsured.Children.Count > 0) {
							ISimpleMemberPointerModel exceptionType = (xmlEnsured.IsEnsuresOnThrow)
								? new CrefSimpleMemberPointer(xmlEnsured.ExceptionCref)
								: null;
							var description = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(xmlEnsured.Children);
							Contract.Assume(description != null);
							results.Add(new ContractConditionSimpleModel(description, exceptionType));
						}
					}
				}
			}
			return new ReadOnlyCollection<IContractConditionSimpleModel>(results);
		}

		private ReadOnlyCollection<IExceptionSimpleModel> CreateExceptions(){
			var results = new List<IExceptionSimpleModel>();
			var xmlDocs = MethodXmlDocs;
			if (null != xmlDocs){
				var xmlExceptions = xmlDocs.Exceptions;
				if (null != xmlExceptions){
					foreach (var set in xmlExceptions.GroupBy(ex => ex.CRef)){
						var cRef = set.Key;
						var conditions = new List<IComplexTextNode>();
						var ensures = new List<IComplexTextNode>();
						foreach (var exceptionItem in set.Where(ex => ex.Children.Count > 0)){
							var summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(exceptionItem.Children);
							if (null != summary){
								(exceptionItem.HasRelatedEnsures ? ensures : conditions).Add(summary);
							}
						}

						var firstReference = set.Select(ex => ex.CrefTarget).FirstOrDefault(ex => ex != null);
						var exceptionPointer = firstReference == null
							? (ISimpleMemberPointerModel)new CrefSimpleMemberPointer(cRef, cRef)
							: new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(firstReference), firstReference);

						results.Add(new ExceptionSimpleModel(exceptionPointer, conditions, ensures));
					}
				}
			}
			return new ReadOnlyCollection<IExceptionSimpleModel>(results);
		}

		private IParameterSimpleModel CreateReturn(){
			if (Definition.ReturnType == null || Definition.ReturnType.FullName == "System.Void")
				return null;

			var xmlDocs = MethodXmlDocs;
			IComplexTextNode summary = null;
			if (null != xmlDocs) {
				var summaryParsedXml = xmlDocs.Returns;
				if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0) {
					summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
				}
			}
			var paramTypeReference = Definition.ReturnType;
			var paramTypeModel = new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(paramTypeReference), paramTypeReference);
			return new ReturnSimpleModel(paramTypeModel, summary);
		}

		private ReadOnlyCollection<IParameterSimpleModel> CreateParameters(){
			var results = new List<IParameterSimpleModel>();
			if (Definition.HasParameters){
				var xmlDocs = MethodXmlDocs;
				foreach (var parameterDefinition in Definition.Parameters){
					IComplexTextNode summary = null;
					if (null != xmlDocs) {
						var summaryParsedXml = xmlDocs.DocsForParameter(parameterDefinition.Name);
						if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0) {
							summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
						}
					}

					var paramTypeReference = parameterDefinition.ParameterType;
					var paramTypeModel = new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(paramTypeReference), paramTypeReference);
					results.Add(new DefinitionParameterSimpleModel(parameterDefinition, paramTypeModel, summary));
				}
			}
			return new ReadOnlyCollection<IParameterSimpleModel>(results);
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

		protected MethodDefinitionXmlDoc MethodXmlDocs {
			get {
				return _methodDefinitionXmlDocOverride
					?? DefinitionXmlDocs as MethodDefinitionXmlDoc;
			}
		}

		public override string SubTitle {
			get {
				if (Definition.IsConstructor)
					return "Constructor";
				if (Definition.IsOperatorOverload())
					return "Operator";
				if (Definition.IsGetter)
					return "Getter";
				if (Definition.IsSetter)
					return "Setter";
				return "Method";
			}
		}

		public bool HasGenericParameters { get { return GenericParameters.Count > 0; } }

		public IList<IGenericParameterSimpleModel> GenericParameters { get { return _genericParameters.Value; } }

		public bool HasParameters { get { return Parameters.Count > 0; } }

		public IList<IParameterSimpleModel> Parameters { get { return _parameters.Value; } }

		public bool HasReturn { get { return _return.Value != null; } }

		public IParameterSimpleModel Return { get { return _return.Value; } }

		public bool HasExceptions { get { return Exceptions.Count > 0; } }

		public IList<IExceptionSimpleModel> Exceptions { get { return _exceptions.Value; } }

		public bool HasEnsures { get { return Ensures.Count > 0; } }

		public IList<IContractConditionSimpleModel> Ensures { get { return _ensures.Value; } }

		public bool HasRequires { get { return Requires.Count > 0; } }

		public IList<IContractConditionSimpleModel> Requires {
			get { return _requires.Value; }
		}
	}
}

