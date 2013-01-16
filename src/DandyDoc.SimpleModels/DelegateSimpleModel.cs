using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
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

		public DelegateSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel)
			: base(definition, assemblyModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(assemblyModel != null);
			_parameters = new Lazy<ReadOnlyCollection<IParameterSimpleModel>>(CreateParameters, true);
			_return = new Lazy<IParameterSimpleModel>(CreateReturn, true);
		}

		protected DelegateTypeDefinitionXmlDoc DelegateXmlDocs { get { return DefinitionXmlDocs as DelegateTypeDefinitionXmlDoc; } }

		private IParameterSimpleModel CreateReturn(){
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
			var paramTypeModel = new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(returnType), returnType);
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

		public bool HasParameters { get { return Parameters.Count > 0; } }

		public IList<IParameterSimpleModel> Parameters { get { return _parameters.Value; } }


		public bool HasReturn { get { return Return != null; } }

		public IParameterSimpleModel Return { get { return _return.Value; } }
	}
}
