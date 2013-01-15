using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class AssemblySimpleModel : IAssemblySimpleModel
	{

		private class DefinitionModelCollection<TDefinition,TModel>
			where TDefinition : MemberReference, IMemberDefinition
			where TModel : class, ISimpleModel
		{

			public DefinitionModelCollection(IEnumerable<TDefinition> definitions, Converter<TDefinition, TModel> converter, Comparison<TModel> comparison) {
				Contract.Requires(definitions != null);
				Contract.Requires(converter != null);
				Contract.Requires(comparison != null);
				var definitionsArray = definitions.ToArray();
				var modelsArray = Array.ConvertAll(definitionsArray, converter);
				_definitionModelLookup = new Dictionary<TDefinition, TModel>(definitionsArray.Length);
				_definitionModelReverseLookup = new Dictionary<TModel, TDefinition>(definitionsArray.Length);
				for (int i = 0; i < definitionsArray.Length; i++) {
					_definitionModelLookup[definitionsArray[i]] = modelsArray[i];
					_definitionModelReverseLookup[modelsArray[i]] = definitionsArray[i];
				}
				
				Array.Sort(modelsArray, comparison);

				Definitions = Array.AsReadOnly(definitionsArray);
				SortedModels = Array.AsReadOnly(modelsArray);
			}

			private readonly Dictionary<TDefinition, TModel> _definitionModelLookup;

			private readonly Dictionary<TModel, TDefinition> _definitionModelReverseLookup;

			public ReadOnlyCollection<TDefinition> Definitions { get; private set; }

			public ReadOnlyCollection<TModel> SortedModels { get; private set; }

			public TModel GetModel(TDefinition key){
				Contract.Requires(key != null);
				TModel result;
				_definitionModelLookup.TryGetValue(key, out result);
				return result;
			}

			public TDefinition GetDefinition(TModel key){
				Contract.Requires(key != null);
				TDefinition result;
				_definitionModelReverseLookup.TryGetValue(key, out result);
				return result;
			}

			[ContractInvariantMethod]
			private void CodeContractInvariant(){
				Contract.Invariant(Definitions != null);
				Contract.Invariant(SortedModels != null);
			}

		}

		private class TypeDefinitionModelCollection : DefinitionModelCollection<TypeDefinition, ITypeSimpleModel>
		{
			public TypeDefinitionModelCollection(IEnumerable<TypeDefinition> definitions, Converter<TypeDefinition, ITypeSimpleModel> converter, Comparison<ITypeSimpleModel> comparison) 
				: base(definitions, converter, comparison)
			{
				Contract.Requires(definitions != null);
				Contract.Requires(converter != null);
				Contract.Requires(comparison != null);
				var roots = SortedModels.Where(m =>{
					var d = GetDefinition(m);
					return d.DeclaringType == null;
				}).ToArray();
				SortedRootModels = Array.AsReadOnly(roots);
			}

			public ReadOnlyCollection<ITypeSimpleModel> SortedRootModels { get; private set; }

		}

		private class SimpleModelMembersCollection : ISimpleModelMembersCollection
		{

			private readonly DefinitionModelCollection<TypeDefinition, ITypeSimpleModel> _typesCollection;
			private readonly DefinitionModelCollection<TypeDefinition, IDelegateSimpleModel> _delegatesCollection;
			private readonly DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> _constructorsCollection;
			private readonly DefinitionModelCollection<PropertyDefinition, IPropertySimpleModel> _propertiesCollection;
			private readonly DefinitionModelCollection<FieldDefinition, IFieldSimpleModel> _fieldsCollection;
			private readonly DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> _methodsCollection;
			private readonly DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> _operatorsCollection;
			private readonly DefinitionModelCollection<EventDefinition, IEventSimpleModel> _eventsCollection;

			public SimpleModelMembersCollection(
				DefinitionModelCollection<TypeDefinition, ITypeSimpleModel> types,
				DefinitionModelCollection<TypeDefinition, IDelegateSimpleModel> delegates,
				DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> constructors,
				DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> methods,
				DefinitionModelCollection<MethodDefinition, IMethodSimpleModel> operators,
				DefinitionModelCollection<PropertyDefinition, IPropertySimpleModel> properties,
				DefinitionModelCollection<FieldDefinition, IFieldSimpleModel> fields,
				DefinitionModelCollection<EventDefinition, IEventSimpleModel> events
			){
				Contract.Requires(types != null);
				Contract.Requires(delegates != null);
				Contract.Requires(constructors != null);
				Contract.Requires(methods != null);
				Contract.Requires(operators != null);
				Contract.Requires(properties != null);
				Contract.Requires(fields != null);
				Contract.Requires(events != null);
				_typesCollection = types;
				_delegatesCollection = delegates;
				_constructorsCollection = constructors;
				_methodsCollection = methods;
				_operatorsCollection = operators;
				_propertiesCollection = properties;
				_fieldsCollection = fields;
				_eventsCollection = events;
			}

			public IList<ITypeSimpleModel> NestedTypes {
				get {
					Contract.Ensures(Contract.Result<IList<ITypeSimpleModel>>() != null);
					return _typesCollection.SortedModels;
				}
			}

			public IList<IDelegateSimpleModel> NestedDelegates {
				get {
					Contract.Ensures(Contract.Result<IList<IDelegateSimpleModel>>() != null);
					return _delegatesCollection.SortedModels;
				}
			}

			public IList<IMethodSimpleModel> Constructors {
				get {
					Contract.Ensures(Contract.Result<IList<IMethodSimpleModel>>() != null);
					return _constructorsCollection.SortedModels;
				}
			}

			public IList<IMethodSimpleModel> Methods {
				get {
					Contract.Ensures(Contract.Result<IList<IMethodSimpleModel>>() != null);
					return _methodsCollection.SortedModels;
				}
			}

			public IList<IMethodSimpleModel> Operators {
				get {
					Contract.Ensures(Contract.Result<IList<IMethodSimpleModel>>() != null);
					return _operatorsCollection.SortedModels;
				}
			}

			public IList<IPropertySimpleModel> Properties {
				get {
					Contract.Ensures(Contract.Result<IList<IPropertySimpleModel>>() != null);
					return _propertiesCollection.SortedModels;
				}
			}

			public IList<IFieldSimpleModel> Fields {
				get {
					Contract.Ensures(Contract.Result<IList<IFieldSimpleModel>>() != null);
					return _fieldsCollection.SortedModels;
				}
			}

			public IList<IEventSimpleModel> Events {
				get {
					Contract.Ensures(Contract.Result<IList<IEventSimpleModel>>() != null);
					return _eventsCollection.SortedModels;
				}
			}

		}

		private readonly Lazy<TypeDefinitionModelCollection> _types;
		private readonly ConcurrentDictionary<ITypeSimpleModel, SimpleModelMembersCollection> _membersCache;

		public AssemblySimpleModel(AssemblyDefinition assemblyDefinition, ISimpleModelRepository repository) {
			if (null == assemblyDefinition) throw new ArgumentNullException("assemblyDefinition");
			if (null == repository) throw new ArgumentNullException("repository");
			Contract.EndContractBlock();
			Definition = assemblyDefinition;
			RootRepository = repository;
			XmlDocOverlay = new XmlDocOverlay(new CrefOverlay(new AssemblyDefinitionCollection(new[]{assemblyDefinition})));
			_types = new Lazy<TypeDefinitionModelCollection>(GenerateTypeViewModels, true);
			_membersCache = new ConcurrentDictionary<ITypeSimpleModel, SimpleModelMembersCollection>();
		}

		private static IEnumerable<TypeDefinition> ExtractAllTypeDefinitions(TypeDefinition node){
			Contract.Requires(null != node);
			yield return node;
			if (node.HasNestedTypes){
				foreach (var result in node.NestedTypes.SelectMany(ExtractAllTypeDefinitions))
					yield return result;
			}
		}

		private TypeDefinitionModelCollection GenerateTypeViewModels() {
			Contract.Ensures(Contract.Result<DefinitionModelCollection<TypeDefinition, ITypeSimpleModel>>() != null);
			Contract.Assume(Definition.Modules != null);
			var rootDefinitions = Definition.Modules.SelectMany(x => x.Types.Where(TypeFilter));
			var definitions = rootDefinitions.SelectMany(ExtractAllTypeDefinitions).Where(TypeFilter);
			return new TypeDefinitionModelCollection(definitions, CreateTypeSimpleModelInstance, TypeModelComparison);
		}

		protected virtual ITypeSimpleModel CreateTypeSimpleModelInstance(TypeDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<ITypeSimpleModel>() != null);
			if(definition.IsDelegateType())
				return new DelegateSimpleModel(definition, this);
			return new TypeSimpleModel(definition, this);
		}

		private static int DefaultSimpleModelComparison(ISimpleModel a, ISimpleModel b) {
			if (a == null)
				return b == null ? 0 : -1;
			if (b == null)
				return 1;
			return StringComparer.OrdinalIgnoreCase.Compare(a.ShortName, b.ShortName);
		}

		protected virtual bool PropertyFilter(PropertyDefinition definition) {
			Contract.Requires(definition != null);
			if (definition.IsSpecialName)
				return false;
			return definition.IsExternallyVisible();
		}

		protected virtual int PropertyModelComparison(IPropertySimpleModel a, IPropertySimpleModel b) {
			return DefaultSimpleModelComparison(a, b);
		}

		protected virtual bool FieldFilter(FieldDefinition definition) {
			Contract.Requires(definition != null);
			if (definition.Name.Length >= 2 && definition.Name[0] == '$' && definition.Name[definition.Name.Length - 1] == '$')
				return false;
			if (definition.IsSpecialName)
				return false;
			return definition.IsExternallyVisible();
		}

		protected virtual int FieldModelComparison(IFieldSimpleModel a, IFieldSimpleModel b) {
			return DefaultSimpleModelComparison(a, b);
		}

		protected virtual bool EventFilter(EventDefinition definition) {
			Contract.Requires(definition != null);
			if (definition.IsSpecialName)
				return false;
			return definition.IsExternallyVisible();
		}

		protected virtual int EventModelComparison(IEventSimpleModel a, IEventSimpleModel b) {
			return DefaultSimpleModelComparison(a, b);
		}

		protected virtual bool MethodFilter(MethodDefinition definition) {
			Contract.Requires(definition != null);
			if (definition.Name.Length >= 2 && definition.Name[0] == '$' && definition.Name[definition.Name.Length - 1] == '$')
				return false;
			if (definition.IsSpecialName || definition.IsFinalizer())
				return false;
			return definition.IsExternallyVisible();
		}

		protected virtual int MethodModelComparison(IMethodSimpleModel a, IMethodSimpleModel b) {
			return DefaultSimpleModelComparison(a, b);
		}

		protected virtual bool TypeFilter(TypeDefinition definition) {
			Contract.Requires(definition != null);
			return definition.IsExternallyVisible();
		}

		protected virtual int TypeModelComparison(ITypeSimpleModel a, ITypeSimpleModel b){
			return DefaultSimpleModelComparison(a, b);
		}

		private ISimpleModel GetModelFromCrefCore(string cref) {
			Contract.Requires(!String.IsNullOrEmpty(cref));
			var reference = CrefOverlay.GetReference(cref);
			if (null == reference)
				return null;

			return ReferenceToModel(reference);
		}

		private ISimpleModel ReferenceToModel(MemberReference reference){
			Contract.Requires(null != reference);
			if (reference is TypeDefinition)
				return DefinitionToModel((TypeDefinition)reference);

			throw new NotSupportedException();
		}

		private ITypeSimpleModel DefinitionToModel(TypeDefinition definition){
			Contract.Requires(null != definition);
			return _types.Value.GetModel(definition);
		}

		private SimpleModelMembersCollection GetMembersCore(ITypeSimpleModel model){
			Contract.Requires(model != null);
			Contract.Ensures(Contract.Result<SimpleModelMembersCollection>() != null);
			return _membersCache.GetOrAdd(model, GenerateSimpleModelMembersCollection);
		}

		private SimpleModelMembersCollection GenerateSimpleModelMembersCollection(ITypeSimpleModel model) {
			Contract.Requires(model != null);
			Contract.Ensures(Contract.Result<SimpleModelMembersCollection>() != null);
			var definition = _types.Value.GetDefinition(model);
			Contract.Assume(definition != null);

			var nestedTypes = new List<TypeDefinition>();
			var nestedDelegates = new List<TypeDefinition>();
			if (definition.HasNestedTypes){
				foreach (var nestedType in definition.NestedTypes.Where(TypeFilter)){
					(nestedType.IsDelegateType() ? nestedDelegates : nestedTypes).Add(nestedType);
				}
			}

			var constructors = new List<MethodDefinition>();
			var methods = new List<MethodDefinition>();
			var operators = new List<MethodDefinition>();
			if (definition.HasMethods) {
				foreach (var methodDefinition in definition.Methods.Where(MethodFilter)) {
					var target =
						methodDefinition.IsConstructor
						? constructors
						: methodDefinition.IsOperatorOverload()
						? operators
						: methods;
					target.Add(methodDefinition);
				}
			}

			return new SimpleModelMembersCollection(
				new TypeDefinitionModelCollection(nestedTypes,d => _types.Value.GetModel(d),TypeModelComparison),
				new DefinitionModelCollection<TypeDefinition, IDelegateSimpleModel>(nestedDelegates, d => (IDelegateSimpleModel)(_types.Value.GetModel(d)), TypeModelComparison),
				new DefinitionModelCollection<MethodDefinition, IMethodSimpleModel>(constructors, d => new MethodSimpleModel(d, model), MethodModelComparison),
				new DefinitionModelCollection<MethodDefinition, IMethodSimpleModel>(methods, d => new MethodSimpleModel(d, model), MethodModelComparison),
				new DefinitionModelCollection<MethodDefinition, IMethodSimpleModel>(operators, d => new MethodSimpleModel(d, model), MethodModelComparison),
				new DefinitionModelCollection<PropertyDefinition, IPropertySimpleModel>(definition.Properties.Where(PropertyFilter), d => new PropertySimpleModel(d, model), PropertyModelComparison),
				new DefinitionModelCollection<FieldDefinition, IFieldSimpleModel>(definition.Fields.Where(FieldFilter), d => new FieldSimpleModel(d, model), FieldModelComparison),
				new DefinitionModelCollection<EventDefinition, IEventSimpleModel>(definition.Events.Where(EventFilter), d => new EventSimpleModel(d, model), EventModelComparison)
			);
		}

		// ------------ Public access

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public ISimpleModelRepository RootRepository { get; private set; }

		protected AssemblyDefinition Definition { get; private set; }

		public CrefOverlay CrefOverlay {
			get {
				Contract.Ensures(Contract.Result<CrefOverlay>() != null);
				return XmlDocOverlay.CrefOverlay;
			}
		}

		public virtual string ShortName {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return Definition.Name.Name;
			}
		}

		public virtual string FullName {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return Definition.FullName;
			}
		}

		public virtual string CRef {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return "A:" + FullName;
			}
		}

		public virtual string Title{
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return ShortName;
			}
		}

		public virtual string SubTitle{
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return "Assembly";
			}
		}

		public virtual string AssemblyFileName {
			get{
				return new FileInfo(Definition.MainModule.FullyQualifiedName).Name;
			}
		}

		public string NamespaceName {
			get { return ShortName; }
		}

		public IAssemblySimpleModel ContainingAssembly {
			get{
				Contract.Ensures(Contract.Result<IAssemblySimpleModel>() != null);
				return this;
			}
		}

		public IList<ITypeSimpleModel> RootTypes {
			get {
				Contract.Ensures(Contract.Result<IList<ITypeSimpleModel>>() != null);
				return _types.Value.SortedRootModels;
			}
		}

		public IList<ITypeSimpleModel> AllTypes {
			get {
				Contract.Ensures(Contract.Result<IList<ITypeSimpleModel>>() != null);
				return _types.Value.SortedModels;
			}
		}

		public ISimpleModel GetModelFromCref(string cref) {
			if (String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid CRef", "cref");
			return GetModelFromCrefCore(cref);
		}

		public ISimpleModelMembersCollection GetMembers(ITypeSimpleModel model) {
			if(null == model) throw new ArgumentNullException("model");
			Contract.EndContractBlock();
			return GetMembersCore(model);
		}

		public bool HasFlair {
			get { return false; }
		}

		public IList<IFlairTag> FlairTags {
			get { return new IFlairTag[0]; }
		}

		public bool HasSummary { get { throw new NotImplementedException(); } }
		public IComplexTextNode Summary { get { throw new NotImplementedException(); } }

		public bool HasRemarks { get { return Remarks.Count > 0; } }
		public IList<IComplexTextNode> Remarks { get { throw new NotImplementedException(); } }

		public bool HasExamples { get { return Examples.Count > 0; } }
		public IList<IComplexTextNode> Examples { get { throw new NotImplementedException(); } }

		public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }
		public IList<IComplexTextNode> SeeAlso { get { throw new NotImplementedException(); } }

		[ContractInvariantMethod]
		private void CodeContractInvariants() {
			Contract.Invariant(Definition != null);
			Contract.Invariant(RootRepository != null);
			Contract.Invariant(XmlDocOverlay != null);
		}

	}
}
