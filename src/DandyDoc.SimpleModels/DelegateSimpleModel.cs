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
	public class DelegateSimpleModel : TypeSimpleModel, IDelegateSimpleModel
	{

		private readonly Lazy<ReadOnlyCollection<IParameterSimpleModel>> _parameters;
		private readonly Lazy<IParameterSimpleModel> _return;
		private readonly Lazy<ReadOnlyCollection<IExceptionSimpleModel>> _exceptions;

		public DelegateSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel)
			: base(definition, assemblyModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(assemblyModel != null);
			_parameters = new Lazy<ReadOnlyCollection<IParameterSimpleModel>>(CreateParameters, true);
			_return = new Lazy<IParameterSimpleModel>(CreateReturn, true);
			_exceptions = new Lazy<ReadOnlyCollection<IExceptionSimpleModel>>(CreateExceptions, true);
		}

		protected DelegateTypeDefinitionXmlDoc DelegateXmlDocs { get { return DefinitionXmlDocs as DelegateTypeDefinitionXmlDoc; } }

		public override IList<IFlairTag> FlairTags {
			get {
				var tags = base.FlairTags;

				if (Definition.HasAttributeMatchingName("CanBeNullAttribute"))
					tags.Add(DefaultCanReturnNullTag);
				else if(AllReferenceParamsAndReturnNotNull)
					tags.Add(DefaultNotNullTag);

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
				else if (!HasParameters){
					return false;
				}

				var refParams = Parameters.Where(p => !(p.Type.IsValueType.GetValueOrDefault())).ToList();
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
			get {
				return HasReturn
					&& Definition.HasAttributeMatchingName("NotNullAttribute");
			}
		}

		public virtual bool EnsuresResultNotNullOrEmpty {
			get { return false; }
		}

		public virtual bool RequiresParameterNotNull(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			var parameter = Parameters.FirstOrDefault(p => p.Name == parameterName);
			if (null == parameter)
				return false;

			return parameter.HasAttributeMatchingName("NotNullAttribute");
		}

		public virtual bool RequiresParameterNotNullOrEmpty(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			return false;
		}

		private ReadOnlyCollection<IExceptionSimpleModel> CreateExceptions() {
			var results = new List<IExceptionSimpleModel>();
			var xmlDocs = DelegateXmlDocs;
			if (null != xmlDocs) {
				var xmlExceptions = xmlDocs.Exceptions;
				if (null != xmlExceptions) {
					foreach (var set in xmlExceptions.GroupBy(ex => ex.CRef)) {
						var cRef = set.Key;
						Contract.Assume(!String.IsNullOrEmpty(cRef));
						var conditions = new List<IComplexTextNode>();
						var ensures = new List<IComplexTextNode>();
						foreach (var exceptionItem in set.Where(ex => ex.Children.Count > 0)) {
							var summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(exceptionItem.Children);
							if (null != summary) {
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
			Contract.Assume(Definition.IsDelegateType());
			var returnType = Definition.GetDelegateReturnType();
			if (returnType == null || returnType.FullName == "System.Void")
				return null;

			var xmlDocs = DelegateXmlDocs;
			IComplexTextNode summary = null;
			if (null != xmlDocs) {
				var summaryParsedXml = xmlDocs.Returns;
				if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0) {
					summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
				}
			}
			var paramTypeModel = new ReferenceSimpleMemberPointer(
				returnType,
				FullTypeDisplayNameOverlay.GetDisplayName(returnType));
			return new ReturnSimpleModel(paramTypeModel, summary);
		}

		private ReadOnlyCollection<IParameterSimpleModel> CreateParameters() {
			var results = new List<IParameterSimpleModel>();
			var parameters = Definition.GetDelegateTypeParameters();
			if (parameters.Count > 0) {
				var xmlDocs = DelegateXmlDocs;
				foreach (var parameterDefinition in parameters) {
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
					results.Add(new DefinitionParameterSimpleModel(parameterDefinition, paramTypeModel, summary));
				}
			}
			return new ReadOnlyCollection<IParameterSimpleModel>(results);
		}

		public bool HasParameters { get { return Parameters.Count > 0; } }

		public IList<IParameterSimpleModel> Parameters { get { return _parameters.Value; } }

		public bool HasReturn { get { return Return != null; } }

		public IParameterSimpleModel Return { get { return _return.Value; } }

		public bool HasExceptions { get { return Exceptions.Count > 0; } }

		public IList<IExceptionSimpleModel> Exceptions { get { return _exceptions.Value; } }
	}
}
