using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.ExternalVisibility;
using Mono.Cecil;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.ViewModels
{
	public class TypeViewModel : DefinitionViewModelBase<TypeDefinition>
	{

		public class MemberSection
		{

			internal MemberSection(string title, IEnumerable<IDefinitionViewModel> items){
				Contract.Requires(!String.IsNullOrEmpty(title));
				Contract.Requires(null != items);
				Title = title;
				Items = items;
			}

			public string Title { get; private set; }
			public IEnumerable<IDefinitionViewModel> Items { get; private set; }
		}

		private class CategorizedFields
		{

			private static readonly HashSet<string> SpecialFieldNames = new HashSet<string> {
				"$evaluatingInvariant$"
			};

			public enum Category
			{
				Other,
				Instance,
				Static
			}

			private static readonly ReadOnlyCollection<FieldDefinition> EmptyFieldDefinitionCollection = Array.AsReadOnly(new FieldDefinition[0]);

			private static Category Categorize(FieldDefinition fieldDefinition) {
				Contract.Requires(null != fieldDefinition);
				if(fieldDefinition.IsSpecialName || SpecialFieldNames.Contains(fieldDefinition.Name))
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

			private static readonly HashSet<string> SpecialMethodNames = new HashSet<string> {
				"$InvariantMethod$"
			};

			private static readonly ReadOnlyCollection<MethodDefinition> EmptyMethodDefinitionCollection = Array.AsReadOnly(new MethodDefinition[0]);

			private static Category Categorize(MethodDefinition methodDefinition) {
				Contract.Requires(null != methodDefinition);
				if (methodDefinition.IsConstructor)
					return methodDefinition.IsStatic ? Category.StaticConstructor : Category.InstanceConstructor;
				if (methodDefinition.IsOperatorOverload())
					return Category.Operator;
				if (methodDefinition.IsSpecialName || methodDefinition.IsFinalizer() || SpecialMethodNames.Contains(methodDefinition.Name))
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

		private readonly Lazy<CategorizedMethods> _categorizedExposedMethods;
		private readonly Lazy<CategorizedFields> _categorizedExposedFields;
		private readonly Lazy<CategorizedProperties> _categorizedExposedProperties;
		private readonly Lazy<CategorizedEvents> _categorizedExposedEvents;
		private readonly Lazy<ReadOnlyCollection<TypeDefinition>> _nestedExposedTypes;
		private readonly Lazy<ReadOnlyCollection<TypeDefinition>> _delegateExposedTypes;

		public TypeViewModel(TypeDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
			_categorizedExposedMethods = new Lazy<CategorizedMethods>(() => new CategorizedMethods(Definition.Methods.Where(x => x.IsExternallyVisible())));
			_categorizedExposedFields = new Lazy<CategorizedFields>(() => new CategorizedFields(Definition.Fields.Where(x => x.IsExternallyVisible())));
			_categorizedExposedProperties = new Lazy<CategorizedProperties>(() => new CategorizedProperties(Definition.Properties.Where(x => x.IsExternallyVisible())));
			_categorizedExposedEvents = new Lazy<CategorizedEvents>(() => new CategorizedEvents(Definition.Events.Where(x => x.IsExternallyVisible())));
			_nestedExposedTypes = new Lazy<ReadOnlyCollection<TypeDefinition>>(() => Array.AsReadOnly(Definition.NestedTypes.Where(x => x.IsExternallyVisible() && !x.IsDelegateType()).ToArray()));
			_delegateExposedTypes = new Lazy<ReadOnlyCollection<TypeDefinition>>(() => Array.AsReadOnly(Definition.NestedTypes.Where(x => x.IsExternallyVisible() && x.IsDelegateType()).ToArray()));
		}

		public override string Title{
			get{
				return Definition.IsNested ? base.Title : ShortName;
			}
		}

		public override string SubTitle {
			get{
				if (Definition.IsEnum)
					return "Enumeration";
				if (Definition.IsValueType)
					return "Structure";
				if (Definition.IsInterface)
					return "Interface";
				if (Definition.IsDelegateType())
					return "Delegate";
				return "Class";
			}
		}

		protected override IEnumerable<MemberFlair> GetFlairTags(){
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			if (Definition.IsEnum && Definition.HasFlagsAttribute())
				yield return new MemberFlair("flags", "Flags", "Bitwise combination is allowed.");

			if (!Definition.IsValueType && Definition.IsSealed && !Definition.IsDelegateType())
				yield return new MemberFlair("sealed","Inheritance","This type is sealed, preventing inheritance.");
		}
		
		new public TypeDefinitionXmlDoc XmlDoc { get { return (TypeDefinitionXmlDoc)(base.XmlDoc); } }

		public IList<MemberSection> GetDefaultMemberListingSections() {
			Contract.Ensures(Contract.Result<IList<MemberSection>>() != null);
			var results = new List<MemberSection>();

			if(ExposedNestedTypes.Count > 0)
				results.Add(new MemberSection("Nested Types", ToTypeViewModels(ExposedNestedTypes)));
			if (ExposedDelegateTypes.Count > 0)
				results.Add(new MemberSection("Delegates", ToTypeViewModels(ExposedDelegateTypes)));
			if (ExposedInstanceConstructors.Count > 0)
				results.Add(new MemberSection("Constructors", ToMethodViewModels(ExposedInstanceConstructors)));

			var propertyDefinitions = new List<PropertyDefinition>();
			if (ExposedInstanceProperties.Count > 0)
				propertyDefinitions.AddRange(ExposedInstanceProperties);
			if (ExposedStaticProperties.Count > 0)
				propertyDefinitions.AddRange(ExposedStaticProperties);
			if (propertyDefinitions.Count > 0)
				results.Add(new MemberSection("Properties", ToPropertyViewModels(propertyDefinitions)));

			var fieldDefinitions = new List<FieldDefinition>();
			if (ExposedInstanceFields.Count > 0)
				fieldDefinitions.AddRange(ExposedInstanceFields);
			if (ExposedStaticFields.Count > 0)
				fieldDefinitions.AddRange(ExposedStaticFields);
			if (fieldDefinitions.Count > 0)
				results.Add(new MemberSection("Fields", ToFieldViewModels(fieldDefinitions)));

			var methodDefinitions = new List<MethodDefinition>();
			if (ExposedInstanceMethods.Count > 0)
				methodDefinitions.AddRange(ExposedInstanceMethods);
			if (ExposedStaticMethods.Count > 0)
				methodDefinitions.AddRange(ExposedStaticMethods);
			if (methodDefinitions.Count > 0)
				results.Add(new MemberSection("Methods", ToMethodViewModels(methodDefinitions)));

			var eventDefinitions = new List<EventDefinition>();
			if (ExposedInstanceEvents.Count > 0)
				eventDefinitions.AddRange(ExposedInstanceEvents);
			if (ExposedStaticEvents.Count > 0)
				eventDefinitions.AddRange(ExposedStaticEvents);
			if (eventDefinitions.Count > 0)
				results.Add(new MemberSection("Events", ToEventViewModels(eventDefinitions)));
			
			if(ExposedOperators.Count > 0)
				results.Add(new MemberSection("Operators", ToMethodViewModels(ExposedOperators)));

			return results;
		}

		public ReadOnlyCollection<TypeDefinition> ExposedNestedTypes { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<TypeDefinition>>() != null);
			return _nestedExposedTypes.Value;
		} }

		public ReadOnlyCollection<TypeDefinition> ExposedDelegateTypes { get {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<TypeDefinition>>() != null);
			return _delegateExposedTypes.Value;
		} }

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

		public IEnumerable<FieldViewModel> ToFieldViewModels(IEnumerable<FieldDefinition> definitions) {
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<FieldViewModel>>() != null);
			return definitions.Select(d => new FieldViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<EventViewModel> ToEventViewModels(IEnumerable<EventDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<EventViewModel>>() != null);
			return definitions.Select(d => new EventViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<MethodViewModel> ToMethodViewModels(IEnumerable<MethodDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<MethodViewModel>>() != null);
			return definitions.Select(d => new MethodViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<PropertyViewModel> ToPropertyViewModels(IEnumerable<PropertyDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<PropertyViewModel>>() != null);
			return definitions.Select(d => new PropertyViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<TypeViewModel> ToTypeViewModels(IEnumerable<TypeDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<TypeViewModel>>() != null);
			return definitions.Select(d => new TypeViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<EnumValueViewModel> ToEnumValueViewModels(IEnumerable<FieldDefinition> definitions){
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<FieldViewModel>>() != null);
			return definitions.Select(d => new EnumValueViewModel(d, XmlDocOverlay, CrefOverlay));
		}

		public IEnumerable<GenericTypeParameterViewModel> ToGenericParameterViewModels( IEnumerable<GenericParameter> parameters){
			if(null == parameters) throw new ArgumentNullException("parameters");
			Contract.Ensures(Contract.Result<IEnumerable<GenericTypeParameterViewModel>>() != null);
			return parameters.Select(p => new GenericTypeParameterViewModel(p, this));
		}

	}
}
