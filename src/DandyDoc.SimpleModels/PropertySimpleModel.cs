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
	public class PropertySimpleModel : DefinitionMemberSimpleModelBase<PropertyDefinition>, IPropertySimpleModel
	{

		private readonly Lazy<ReadOnlyCollection<IParameterSimpleModel>> _parameters;
		private readonly Lazy<ReadOnlyCollection<IExceptionSimpleModel>> _exceptions;

		public PropertySimpleModel(PropertyDefinition definition, IMethodSimpleModel getter, IMethodSimpleModel setter, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
			Getter = getter;
			Setter = setter;
			_parameters = new Lazy<ReadOnlyCollection<IParameterSimpleModel>>(CreateParameters, true);
			_exceptions = new Lazy<ReadOnlyCollection<IExceptionSimpleModel>>(CreateExceptions, true);
		}

		private ReadOnlyCollection<IExceptionSimpleModel> CreateExceptions() {
			var results = new List<IExceptionSimpleModel>();
			var xmlDocs = PropertyXmlDocs;
			if (null != xmlDocs) {
				var xmlExceptions = xmlDocs.Exceptions;
				if (null != xmlExceptions) {
					foreach (var set in xmlExceptions.GroupBy(ex => ex.CRef)) {
						var cRef = set.Key;
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
							? (ISimpleMemberPointerModel)new CrefSimpleMemberPointer(cRef, cRef)
							: new ReferenceSimpleMemberPointer(NestedTypeDisplayNameOverlay.GetDisplayName(firstReference), firstReference);

						results.Add(new ExceptionSimpleModel(exceptionPointer, conditions, ensures));
					}
				}
			}
			return new ReadOnlyCollection<IExceptionSimpleModel>(results);
		}

		private ReadOnlyCollection<IParameterSimpleModel> CreateParameters() {
			var results = new List<IParameterSimpleModel>();
			if (Definition.HasParameters) {
				var xmlDocs = PropertyXmlDocs;
				foreach (var parameterDefinition in Definition.Parameters) {
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

		protected PropertyDefinitionXmlDoc PropertyXmlDocs { get { return DefinitionXmlDocs as PropertyDefinitionXmlDoc; } }

		public override string SubTitle {
			get {
				if (Definition.IsItemIndexerProperty())
					return "Indexer";
				return "Property";
			}
		}

		public ISimpleMemberPointerModel PropertyType {
			get { return new ReferenceSimpleMemberPointer(
					RegularTypeDisplayNameOverlay.GetDisplayName(Definition.PropertyType),
					Definition.PropertyType
				);
			}
		}

		public bool HasValueDescription {
			get { return ValueDescription != null; }
		}

		public IComplexTextNode ValueDescription {
			get {
				var xmlDoc = PropertyXmlDocs;
				if (xmlDoc == null)
					return null;
				var valueDoc = xmlDoc.ValueDoc;
				if (null == valueDoc)
					return null;
				return ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(valueDoc.Children);
			}
		}

		public bool HasParameters { get { return Parameters.Count > 0; } }

		public IList<IParameterSimpleModel> Parameters { get { return _parameters.Value; } }

		public bool HasExceptions { get { return Exceptions.Count > 0; } }

		public IList<IExceptionSimpleModel> Exceptions { get { return _exceptions.Value; } }

		public bool HasGetter { get { return Getter != null; } }

		public IMethodSimpleModel Getter { get; private set; }

		public bool HasSetter { get { return Setter != null; } }

		public IMethodSimpleModel Setter { get; private set; }
	}
}
