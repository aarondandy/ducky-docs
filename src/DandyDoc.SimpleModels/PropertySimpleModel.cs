using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class PropertySimpleModel : DefinitionMemberSimpleModelBase<PropertyDefinition>, IPropertySimpleModel
	{

		protected static readonly IFlairTag DefaultAbstractPropertyTag = new SimpleFlairTag("abstract", "Inheritance", "This property is abstract and must be implemented by inheriting types.");
		protected static readonly IFlairTag DefaultVirtualPropertyTag = new SimpleFlairTag("virtual", "Inheritance", "This property is virtual and can be overridden by inheriting types.");
		protected static readonly IFlairTag DefaultNoNullsTag = new SimpleFlairTag("no nulls", "Null Values", "This property does not return or accept null.");
		protected static readonly IFlairTag DefaultNoNullReturnTag = new SimpleFlairTag("no nulls", "Null Values", "This property does not return null.");
		protected static readonly IFlairTag DefaultNoNullInputTag = new SimpleFlairTag("no nulls", "Null Values", "This property does not accept null.");
		protected static readonly IFlairTag DefaultIndexerOperatorTag = new SimpleFlairTag("indexer", "Operator", "This property is invoked through a language index operator.");

		protected static readonly IFlairTag DefaultGetTag = new SimpleFlairTag("get", "Property", "Value can be read externally.");
		protected static readonly IFlairTag DefaultProGetTag = new SimpleFlairTag("proget", "Property", "Value can be read through inheritance.");
		protected static readonly IFlairTag DefaultSetTag = new SimpleFlairTag("set", "Property", "Value can be assigned externally.");
		protected static readonly IFlairTag DefaultProSetTag = new SimpleFlairTag("proset", "Property", "Value can be assigned through inheritance.");

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
						results.Add(ExceptionSimpleModel.Create(set.Key, set));
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

		protected PropertyDefinitionXmlDoc PropertyXmlDocs { get { return DefinitionXmlDocs as PropertyDefinitionXmlDoc; } }

		public override string SubTitle {
			get {
				if (Definition.IsItemIndexerProperty())
					return "Indexer";
				return "Property";
			}
		}

		public ISimpleMemberPointerModel PropertyType {
			get {
				Contract.Assume(Definition.PropertyType != null);
				return new ReferenceSimpleMemberPointer(
					Definition.PropertyType,
					FullTypeDisplayNameOverlay.GetDisplayName(Definition.PropertyType)
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

		public bool AllReferenceParamsAndReturnNotNull {
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

		public bool EnsuresResultNotNull {
			get {
				return HasReturn && (
					Definition.HasAttributeMatchingName("NotNullAttribute")
					|| (HasGetter && Getter.EnsuresResultNotNull)
				);
			}
		}

		public bool EnsuresResultNotNullOrEmpty {
			get {
				return HasReturn && HasGetter && Getter.EnsuresResultNotNullOrEmpty;
			}
		}

		public bool RequiresParameterNotNull(string parameterName) {
			return (HasGetter && Getter.RequiresParameterNotNull(parameterName))
				|| (HasSetter && Setter.RequiresParameterNotNull(parameterName));
		}

		public bool RequiresParameterNotNullOrEmpty(string parameterName) {
			return (HasGetter && Getter.RequiresParameterNotNullOrEmpty(parameterName))
				|| (HasSetter && Setter.RequiresParameterNotNullOrEmpty(parameterName));
		}

		public virtual bool HasReturn {
			get { return HasGetter && Getter.HasReturn; }
		}

		public virtual IParameterSimpleModel Return {
			get { return Getter.Return; }
		}

		public virtual bool IsPure {
			get {
				return Definition.HasPureAttribute()
					|| (PropertyXmlDocs != null && PropertyXmlDocs.HasPureElement);
			}
		}

		public virtual bool CanReturnNull {
			get{
				return Definition.HasAttributeMatchingName("CanBeNullAttribute")
					|| (HasGetter && Getter.CanReturnNull);
			}
		}

		public override IList<IFlairTag> FlairTags {
			get {
				var tags = base.FlairTags;

				if (HasGetter && HasSetter) {
					if (Getter.AllReferenceParamsAndReturnNotNull && Setter.AllReferenceParamsAndReturnNotNull)
						tags.Add(DefaultNoNullsTag);
				}
				else if (HasGetter) {
					if (Getter.AllReferenceParamsAndReturnNotNull)
						tags.Add(DefaultNoNullReturnTag);
				}
				else if (HasSetter) {
					if (Setter.AllReferenceParamsAndReturnNotNull)
						tags.Add(DefaultNoNullInputTag);
				}

				if (IsPure)
					tags.Add(DefaultPureTag);

				if (Definition.IsSealed())
					tags.Add(new SimpleFlairTag("sealed", "Inheritance", "This property is sealed, preventing inheritance."));

				if (!Definition.DeclaringType.IsInterface) {
					if (Definition.IsAbstract())
						tags.Add(DefaultAbstractPropertyTag);
					else if (Definition.IsVirtual() && Definition.IsNewSlot() && !Definition.IsFinal())
						tags.Add(DefaultVirtualPropertyTag);
				}

				if (Definition.HasParameters && "Item".Equals(Definition.Name))
					tags.Add(DefaultIndexerOperatorTag);

				var getMethod = Definition.GetMethod;
				var setMethod = Definition.SetMethod;
				var propertyVisibility = ExternalVisibilityOverlay.Get(Definition);
				if (null != getMethod) {
					var methodVisibility = ExternalVisibilityOverlay.Get(getMethod);
					if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
						tags.Add(DefaultGetTag);
					}
					else if (methodVisibility == ExternalVisibilityKind.Protected) {
						tags.Add(DefaultProGetTag);
					}
				}

				if (null != setMethod) {
					var methodVisibility = ExternalVisibilityOverlay.Get(setMethod);
					if (methodVisibility == propertyVisibility || methodVisibility == ExternalVisibilityKind.Public) {
						tags.Add(DefaultSetTag);
					}
					else if (methodVisibility == ExternalVisibilityKind.Protected) {
						tags.Add(DefaultProSetTag);
					}
				}

				return tags;
			}
		}
	}
}
