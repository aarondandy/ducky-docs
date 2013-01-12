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

			protected TDefinition GetDefinition(TModel key){
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

		private readonly Lazy<TypeDefinitionModelCollection> _types;
		private readonly ConcurrentDictionary<ITypeSimpleModel, ISimpleModelMembersCollection> _membersCache;

		public AssemblySimpleModel(AssemblyDefinition assemblyDefinition, ISimpleModelRepository repository) {
			if (null == assemblyDefinition) throw new ArgumentNullException("assemblyDefinition");
			if (null == repository) throw new ArgumentNullException("repository");
			Contract.EndContractBlock();
			Definition = assemblyDefinition;
			RootRepository = repository;
			XmlDocOverlay = new XmlDocOverlay(new CrefOverlay(new AssemblyDefinitionCollection(new[]{assemblyDefinition})));
			_types = new Lazy<TypeDefinitionModelCollection>(GenerateTypeViewModels, true);
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

		protected virtual bool TypeFilter(TypeDefinition definition) {
			Contract.Requires(definition != null);
			return definition.IsExternallyVisible();
		}

		protected virtual int TypeModelComparison(ITypeSimpleModel a, ITypeSimpleModel b){
			if (a == null)
				return b == null ? 0 : -1;
			if (b == null)
				return 1;
			return StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName);
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

		public virtual string DisplayName {
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
				return DisplayName;
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
			get { return DisplayName; }
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
			throw new NotImplementedException();
		}

		[ContractInvariantMethod]
		private void CodeContractInvariants() {
			Contract.Invariant(Definition != null);
			Contract.Invariant(RootRepository != null);
			Contract.Invariant(XmlDocOverlay != null);
		}

	}
}
