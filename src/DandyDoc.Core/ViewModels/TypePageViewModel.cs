using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DandyDoc.Core.Overlays.Cref;
using Mono.Cecil;
using DandyDoc.Core.Overlays.XmlDoc;

namespace DandyDoc.Core.ViewModels
{
	public class TypePageViewModel
	{

		private class CategorizedFields
		{
			public enum Category
			{
				Other,
				Instance,
				Static
			}

			private static readonly ReadOnlyCollection<FieldDefinition> EmptyFieldDefinitionCollection = Array.AsReadOnly(new FieldDefinition[0]);

			private static Category Categorize(FieldDefinition fieldDefinition) {
				Contract.Requires(null != fieldDefinition);
				if(fieldDefinition.IsSpecialName)
					return Category.Other;
				return fieldDefinition.IsStatic ? Category.Static : Category.Instance;
			}

			private readonly Dictionary<Category, ReadOnlyCollection<FieldDefinition>> _sorted;

			public CategorizedFields(IEnumerable<FieldDefinition> definitions) {
				Contract.Requires(null != definitions);
				_sorted = definitions
					.GroupBy(Categorize)
					.ToDictionary(x => x.Key, x => Array.AsReadOnly(x.ToArray()));
			}

			public ReadOnlyCollection<FieldDefinition> GetOrDefault(Category category) {
				ReadOnlyCollection<FieldDefinition> result;
				return _sorted.TryGetValue(category, out result)
					? result
					: EmptyFieldDefinitionCollection;
			}

		}

		private class CategorizedProperties
		{
			public enum Category
			{
				Other,
				Instance,
				Static
			}

			private static readonly ReadOnlyCollection<PropertyDefinition> EmptyFieldDefinitionCollection = Array.AsReadOnly(new PropertyDefinition[0]);

			private static Category Categorize(PropertyDefinition definition) {
				Contract.Requires(null != definition);
				if (definition.IsSpecialName)
					return Category.Other;
				return definition.IsStatic() ? Category.Static : Category.Instance;
			}

			private readonly Dictionary<Category, ReadOnlyCollection<PropertyDefinition>> _sorted;

			public CategorizedProperties(IEnumerable<PropertyDefinition> definitions) {
				Contract.Requires(null != definitions);
				_sorted = definitions
					.GroupBy(Categorize)
					.ToDictionary(x => x.Key, x => Array.AsReadOnly(x.ToArray()));
			}

			public ReadOnlyCollection<PropertyDefinition> GetOrDefault(Category category) {
				ReadOnlyCollection<PropertyDefinition> result;
				return _sorted.TryGetValue(category, out result)
					? result
					: EmptyFieldDefinitionCollection;
			}

		}

		private class CategorizedEvents
		{
			public enum Category
			{
				Other,
				Instance,
				Static
			}

			private static readonly ReadOnlyCollection<EventDefinition> EmptyFieldDefinitionCollection = Array.AsReadOnly(new EventDefinition[0]);

			private static Category Categorize(EventDefinition definition) {
				Contract.Requires(null != definition);
				if (definition.IsSpecialName)
					return Category.Other;
				return definition.IsStatic() ? Category.Static : Category.Instance;
			}

			private readonly Dictionary<Category, ReadOnlyCollection<EventDefinition>> _sorted;

			public CategorizedEvents(IEnumerable<EventDefinition> definitions) {
				Contract.Requires(null != definitions);
				_sorted = definitions
					.GroupBy(Categorize)
					.ToDictionary(x => x.Key, x => Array.AsReadOnly(x.ToArray()));
			}

			public ReadOnlyCollection<EventDefinition> GetOrDefault(Category category) {
				ReadOnlyCollection<EventDefinition> result;
				return _sorted.TryGetValue(category, out result)
					? result
					: EmptyFieldDefinitionCollection;
			}

		}

		private class CategorizedMethods
		{

			public enum Category
			{
				Other,
				InstanceConstructor,
				StaticConstructor,
				InstanceMethod,
				StaticMethod,
				Operator
			}

			private static readonly ReadOnlyCollection<MethodDefinition> EmptyMethodDefinitionCollection = Array.AsReadOnly(new MethodDefinition[0]);

			private static Category Categorize(MethodDefinition methodDefinition) {
				Contract.Requires(null != methodDefinition);
				if (methodDefinition.IsConstructor)
					return methodDefinition.IsStatic ? Category.StaticConstructor : Category.InstanceConstructor;
				if (methodDefinition.IsOperatorOverload())
					return Category.Operator;
				if (methodDefinition.IsSpecialName || methodDefinition.IsFinalizer())
					return Category.Other;
				return methodDefinition.IsStatic ? Category.StaticMethod : Category.InstanceMethod;
			}

			private readonly Dictionary<Category, ReadOnlyCollection<MethodDefinition>> _sorted;

			public CategorizedMethods(IEnumerable<MethodDefinition> definitions) {
				Contract.Requires(null != definitions);
				_sorted = definitions
					.GroupBy(Categorize)
					.ToDictionary(x => x.Key, x => Array.AsReadOnly(x.ToArray()));
			}

			public ReadOnlyCollection<MethodDefinition> GetOrDefault(Category category) {
				ReadOnlyCollection<MethodDefinition> result;
				return _sorted.TryGetValue(category, out result)
					? result
					: EmptyMethodDefinitionCollection;
			}

		}

		private readonly Lazy<TypeDefinitionXmlDoc> _xmlDoc;
		private readonly Lazy<CategorizedMethods> _categorizedExposedMethods;
		private readonly Lazy<CategorizedFields> _categorizedExposedFields;
		private readonly Lazy<CategorizedProperties> _categorizedExposedProperties;
		private readonly Lazy<CategorizedEvents> _categorizedExposedEvents; 

		public TypePageViewModel(TypeDefinition typeDefinition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null){
			if(null == typeDefinition) throw new ArgumentNullException("typeDefinition");
			if(null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = typeDefinition;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_xmlDoc = new Lazy<TypeDefinitionXmlDoc>(() => XmlDocOverlay.GetDocumentation(Definition));
			_categorizedExposedMethods = new Lazy<CategorizedMethods>(() => new CategorizedMethods(Definition.Methods.Where(x => x.IsExternallyExposed())));
			_categorizedExposedFields = new Lazy<CategorizedFields>(() => new CategorizedFields(Definition.Fields.Where(x => x.IsExternallyExposed())));
			_categorizedExposedProperties = new Lazy<CategorizedProperties>(() => new CategorizedProperties(Definition.Properties.Where(x => x.IsExternallyExposed())));
			_categorizedExposedEvents = new Lazy<CategorizedEvents>(() => new CategorizedEvents(Definition.Events.Where(x => x.IsExternallyExposed())));
		}

		public string Title {
			get {
				var name = Definition.Name;
				return name + ' ' + (
					Definition.IsValueType
					? "Structure"
					: Definition.IsInterface
					? "Interface"
					: Definition.IsDelegateType()
					? "Delegate"
					: "Class"
				);
			}
		}

		public TypeDefinition Definition { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public TypeDefinitionXmlDoc XmlDoc { get { return _xmlDoc.Value; } }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

		public ParsedXmlElementBase Summary {
			get { return null == XmlDoc ? null : XmlDoc.Summary; }
		}

		public ParsedXmlElementBase Remarks {
			get { return null == XmlDoc ? null : XmlDoc.Remarks; }
		}

		public IList<ParsedXmlElementBase> Examples {
			get { return null == XmlDoc ? null : XmlDoc.Examples; }
		} 

		public ReadOnlyCollection<MethodDefinition> ExposedInstanceConstructors { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.InstanceConstructor);
		} }
		public ReadOnlyCollection<MethodDefinition> ExposedStaticConstructors { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.StaticConstructor);
		} }
		public ReadOnlyCollection<MethodDefinition> ExposedInstanceMethods { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.InstanceMethod);
		} }
		public ReadOnlyCollection<MethodDefinition> ExposedStaticMethods { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.StaticMethod);
		} }
		public ReadOnlyCollection<MethodDefinition> ExposedOperators { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.Operator);
		} }
		public ReadOnlyCollection<MethodDefinition> ExposedOtherMethods { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<MethodDefinition>>() != null);
			return _categorizedExposedMethods.Value.GetOrDefault(CategorizedMethods.Category.Other);
		} }

		public ReadOnlyCollection<FieldDefinition> ExposedStaticFields { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
			return _categorizedExposedFields.Value.GetOrDefault(CategorizedFields.Category.Static);
		} }

		public ReadOnlyCollection<FieldDefinition> ExposedInstanceFields { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
			return _categorizedExposedFields.Value.GetOrDefault(CategorizedFields.Category.Instance);
		} }

		public ReadOnlyCollection<FieldDefinition> ExposedOtherFields { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
			return _categorizedExposedFields.Value.GetOrDefault(CategorizedFields.Category.Other);
		} }

		public ReadOnlyCollection<PropertyDefinition> ExposedStaticProperties { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<PropertyDefinition>>() != null);
			return _categorizedExposedProperties.Value.GetOrDefault(CategorizedProperties.Category.Static);
		} }

		public ReadOnlyCollection<PropertyDefinition> ExposedInstanceProperties { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<PropertyDefinition>>() != null);
			return _categorizedExposedProperties.Value.GetOrDefault(CategorizedProperties.Category.Instance);
		} }

		public ReadOnlyCollection<PropertyDefinition> ExposedOtherProperties { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<PropertyDefinition>>() != null);
			return _categorizedExposedProperties.Value.GetOrDefault(CategorizedProperties.Category.Other);
		} }

		public ReadOnlyCollection<EventDefinition> ExposedStaticEvents { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
			return _categorizedExposedEvents.Value.GetOrDefault(CategorizedEvents.Category.Static);
		} }

		public ReadOnlyCollection<EventDefinition> ExposedInstanceEvents { get {
				Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
				return _categorizedExposedEvents.Value.GetOrDefault(CategorizedEvents.Category.Instance);
		} }

		public ReadOnlyCollection<EventDefinition> ExposedOtherEvents { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<FieldDefinition>>() != null);
			return _categorizedExposedEvents.Value.GetOrDefault(CategorizedEvents.Category.Other);
		} }

		public IEnumerable<PropertySummaryViewModel> ToPropertySummaries(IEnumerable<PropertyDefinition> definitions) {
			return definitions.Select(item => {
				var doc = XmlDocOverlay.GetDocumentation(item);
				var summary = null == doc ? null : doc.Summary;
				return new PropertySummaryViewModel(item, item.Name, CrefOverlay.GetCref(item), summary);
			});
		} 

		public IEnumerable<FieldSummaryViewModel> ToFieldSummaries(IEnumerable<FieldDefinition> definitions) {
			return definitions.Select(item => {
				var doc = XmlDocOverlay.GetDocumentation(item);
				var summary = null == doc ? null : doc.Summary;
				return new FieldSummaryViewModel(item, item.Name, CrefOverlay.GetCref(item), summary);
			});
		}

		public IEnumerable<EventSummaryViewModel> ToEventSummaries(IEnumerable<EventDefinition> definitions) {
			return definitions.Select(item => {
				var doc = XmlDocOverlay.GetDocumentation(item);
				var summary = null == doc ? null : doc.Summary;
				return new EventSummaryViewModel(item, item.Name, CrefOverlay.GetCref(item), summary);
			});
		}

		public IEnumerable<MethodSummaryViewModel> ToMethodSummaries(IEnumerable<MethodDefinition> definitions) {
			foreach (var methodGroup in definitions.GroupBy(x => x.Name)) {
				var items = methodGroup.ToList();
				if (items.Count == 1) {
					var item = items[0];
					var doc = XmlDocOverlay.GetDocumentation(item);
					var summary = null == doc ? null : doc.Summary;
					yield return new MethodSummaryViewModel(item, item.Name, CrefOverlay.GetCref(item), summary);
				}
				else {
					foreach (var item in items) {
						var doc = XmlDocOverlay.GetDocumentation(item);
						var summary = null == doc ? null : doc.Summary;
						var name = item.Name;
						if (item.HasParameters) {
							name = String.Concat(
								name,
								'(',
								String.Join(",", item.Parameters.Select(x => x.ParameterType.Name)),
								')');
						}
						yield return new MethodSummaryViewModel(item, name, CrefOverlay.GetCref(item), summary);
					}
				}
			}
		}

		public IEnumerable<MethodSummaryViewModel> ToConstructorSummaries(IEnumerable<MethodDefinition> definitions) {
			return definitions.Select(item => {
				var name = item.DeclaringType.Name;
				if (item.HasParameters) {
					name = String.Concat(
						name,
						'(',
						String.Join(",", item.Parameters.Select(x => x.ParameterType.Name)),
						')');
				}
				var doc = XmlDocOverlay.GetDocumentation(item);
				var summary = null == doc ? null : doc.Summary;
				return new MethodSummaryViewModel(item, name, CrefOverlay.GetCref(item), summary);
			});
		}

		public IEnumerable<MethodSummaryViewModel> ToOperatorSummaries(IEnumerable<MethodDefinition> definitions) {
			foreach (var methodGroup in definitions.GroupBy(x => x.Name)) {
				var items = methodGroup.ToList();
				if (items.Count == 1) {
					var item = items[0];
					var doc = XmlDocOverlay.GetDocumentation(item);
					var summary = null == doc ? null : doc.Summary;
					var name = item.Name;
					if (name.StartsWith("op_"))
						name = name.Substring(3);
					yield return new MethodSummaryViewModel(item, name, CrefOverlay.GetCref(item), summary);
				}
				else {
					foreach (var item in items) {
						var doc = XmlDocOverlay.GetDocumentation(item);
						var summary = null == doc ? null : doc.Summary;
						var name = item.Name;
						if (item.HasParameters) {
							name = String.Concat(
								name,
								'(',
								String.Join(",", item.Parameters.Select(x => x.ParameterType.Name)),
								')');
						}
						if (name.StartsWith("op_"))
							name = name.Substring(3);
						yield return new MethodSummaryViewModel(item, name, CrefOverlay.GetCref(item), summary);
					}
				}
			}
		}
		
		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Definition);
			Contract.Invariant(null != XmlDocOverlay);
		}

	}
}
