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

		protected static readonly IFlairTag DefaultExtensionMethodTag = new SimpleFlairTag("extension", "Extension", "This method is an extension method.");
		protected static readonly IFlairTag DefaultAbstractMethodTag = new SimpleFlairTag("abstract", "Inheritance", "This method is abstract and must be implemented by inheriting types.");
		protected static readonly IFlairTag DefaultVirtualMethodTag = new SimpleFlairTag("virtual", "Inheritance", "This method is virtual and can be overridden by inheriting types.");
		protected static readonly IFlairTag DefaultStringFormatMethodTag = new SimpleFlairTag("string format", "Signature", "Invoked as a String.Format styled method.");

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

		public virtual bool CanReturnNull {
			get { return Definition.HasAttributeMatchingName("CanBeNullAttribute"); }
		}

		public override IList<IFlairTag> FlairTags {
			get {
				var tags = base.FlairTags;

				if (Definition.IsExtensionMethod())
					tags.Add(DefaultExtensionMethodTag);

				if (CanReturnNull)
					tags.Add(DefaultCanReturnNullTag);
				else if (AllReferenceParamsAndReturnNotNull)
					tags.Add(DefaultNotNullTag);

				if(IsPure)
					tags.Add(DefaultPureTag);

				if (Definition.IsOperatorOverload())
					tags.Add(DefaultOperatorTag);

				if (Definition.IsSealed()) {
					var subject =
						Definition.IsGetter
							? "getter"
						: Definition.IsSetter
							? "setter"
						: "method";
					var description = String.Format("This {0} is sealed, preventing inheritance.", subject);
					Contract.Assume(!String.IsNullOrEmpty(description));
					tags.Add(new SimpleFlairTag("sealed", "Inheritance", description));
				}

				if (!Definition.DeclaringType.IsInterface) {
					if (Definition.IsAbstract && !Definition.DeclaringType.IsInterface)
						tags.Add(DefaultAbstractMethodTag);
					else if (Definition.IsVirtual && Definition.IsNewSlot && !Definition.IsFinal)
						tags.Add(DefaultVirtualMethodTag);
				}

				if (Definition.HasAttributeMatchingName("StringFormatMethodAttribute"))
					tags.Add(DefaultStringFormatMethodTag);

				return tags;
			}
		}

		public virtual bool AllReferenceParamsAndReturnNotNull {
			get {

				var hasReferenceReturn = HasReturn && !(Return.Type.IsValueType.GetValueOrDefault());
				if (hasReferenceReturn) {
					if (!EnsuresResultNotNull && !EnsuresResultNotNullOrEmpty) {
						return false;
					}
				}
				else {
					if (!Definition.HasParameters)
						return false;
				}

				var refParams = Definition.Parameters.Where(p => !(p.ParameterType.IsValueType)).ToList();
				if (0 == refParams.Count) {
					if (!hasReferenceReturn)
						return false;
				}
				else {
					foreach (var paramName in refParams.Select(p => p.Name)) {
						Contract.Assume(!String.IsNullOrEmpty(paramName));
						if (!RequiresParameterNotNull(paramName) && !RequiresParameterNotNullOrEmpty(paramName))
							return false;
					}
				}

				return true;
			}
		}

		public virtual bool EnsuresResultNotNull {
			get{
				return HasReturn && (
					Definition.HasAttributeMatchingName("NotNullAttribute")
					|| (
						MethodXmlDocs != null
						&& MethodXmlDocs.Ensures.Count > 0
						&& MethodXmlDocs.Ensures.Any(x => x.EnsuresResultNotNull)));
			}
		}

		public virtual bool EnsuresResultNotNullOrEmpty {
			get {
				return HasReturn
					&& MethodXmlDocs != null
					&& MethodXmlDocs.Ensures.Count > 0
					&& MethodXmlDocs.Ensures.Any(x => x.EnsuresResultNotNullOrEmpty);
			}
		}

		public virtual bool RequiresParameterNotNull(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			var parameter = Definition.Parameters.FirstOrDefault(p => p.Name == parameterName);
			if (null != parameter) {
				if (parameter.HasAttributeMatchingName("NotNullAttribute"))
					return true;
			}
			if (MethodXmlDocs == null || MethodXmlDocs.Requires.Count == 0)
				return false;
			return MethodXmlDocs.Requires.Any(x => x.RequiresParameterNotNull(parameterName));
		}

		public virtual bool RequiresParameterNotNullOrEmpty(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (MethodXmlDocs == null || MethodXmlDocs.Requires.Count == 0)
				return false;
			return MethodXmlDocs.Requires.Any(x => x.RequiresParameterNotNullOrEmpty(parameterName));
		}

		public virtual bool IsPure {
			get{
				return Definition.HasPureAttribute()
					|| (MethodXmlDocs != null && MethodXmlDocs.HasPureElement);
			}
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
						Contract.Assume(!String.IsNullOrEmpty(cRef));
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
							? (ISimpleMemberPointerModel)new CrefSimpleMemberPointer(cRef)
							: new ReferenceSimpleMemberPointer(
								firstReference,
								NestedTypeDisplayNameOverlay.GetDisplayName(firstReference));

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
			Contract.Assume(paramTypeReference != null);
			var paramTypeModel = new ReferenceSimpleMemberPointer(
				paramTypeReference,
				FullTypeDisplayNameOverlay.GetDisplayName(paramTypeReference));
			return new ReturnSimpleModel(paramTypeModel, this, summary);
		}

		private ReadOnlyCollection<IParameterSimpleModel> CreateParameters(){
			var results = new List<IParameterSimpleModel>();
			if (Definition.HasParameters){
				var xmlDocs = MethodXmlDocs;
				foreach (var parameterDefinition in Definition.Parameters){
					IComplexTextNode summary = null;
					if (null != xmlDocs) {
						Contract.Assume(!String.IsNullOrEmpty(parameterDefinition.Name));
						var summaryParsedXml = xmlDocs.DocsForParameter(parameterDefinition.Name);
						if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0) {
							summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
						}
					}

					var paramTypeReference = parameterDefinition.ParameterType;
					Contract.Assume(paramTypeReference != null);
					var paramTypeModel = new ReferenceSimpleMemberPointer(
						paramTypeReference,
						FullTypeDisplayNameOverlay.GetDisplayName(paramTypeReference));
					results.Add(new DefinitionParameterSimpleModel(parameterDefinition, this, paramTypeModel, summary));
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
						.Select(x => new MemberPointerGenericConstraint(new ReferenceSimpleMemberPointer(
							x,
							NestedTypeDisplayNameOverlay.GetDisplayName(x))));
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

