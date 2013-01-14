using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class TypeSimpleModel : DefinitionSimpleModelBase<TypeDefinition>, ITypeSimpleModel
	{

		private class InheritanceData
		{

			public InheritanceData(TypeDefinition definition, Func<TypeReference,string> getName){
				Contract.Requires(definition != null);
				Contract.Requires(getName != null);

				var baseChain = new List<ISimpleMemberPointerModel>();
				if (!definition.IsInterface){
					var currentReference = definition.BaseType;
					while (null != currentReference){
						var displayName = getName(currentReference);
						Contract.Assume(!String.IsNullOrEmpty(displayName));
						baseChain.Add(new ReferenceSimpleMemberPointer(displayName, currentReference));
						var currentDefinition = currentReference.Resolve();
						if (null == currentDefinition)
							break;
						currentReference = currentDefinition.BaseType;
					}
				}
				baseChain.Reverse();
				BaseChain = new ReadOnlyCollection<ISimpleMemberPointerModel>(baseChain);

				var directInterfaces = new List<ISimpleMemberPointerModel>();
				if (definition.HasInterfaces) {
					Contract.Assume(null != definition.Interfaces);
					directInterfaces.AddRange(definition.Interfaces.Select(x => new ReferenceSimpleMemberPointer(getName(x), x)));
				}
				DirectImplementedInterfaces = new ReadOnlyCollection<ISimpleMemberPointerModel>(directInterfaces);
			}

			public ReadOnlyCollection<ISimpleMemberPointerModel> BaseChain { get; private set; }
			public ReadOnlyCollection<ISimpleMemberPointerModel> DirectImplementedInterfaces { get; private set; }

			[ContractInvariantMethod]
			private void CodeContractInvariant(){
				Contract.Invariant(BaseChain != null);
				Contract.Invariant(DirectImplementedInterfaces != null);
			}
		}

		protected static readonly IFlairTag DefaultFlagsFlair = new SimpleFlairTag("flags", "Enumeration", "Bitwise combination is allowed.");
		protected static readonly IFlairTag DefaultSealedFlair = new SimpleFlairTag("sealed", "Inheritance", "This type is sealed, preventing inheritance.");

		private readonly Lazy<ISimpleModelMembersCollection> _members;
		private readonly Lazy<ReadOnlyCollection<IFlairTag>> _flair;
		private readonly Lazy<InheritanceData> _inheritanceData;

		public TypeSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel)
			: base(definition, assemblyModel)
		{
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == assemblyModel) throw new ArgumentNullException("assemblyModel");
			Contract.EndContractBlock();
			_members = new Lazy<ISimpleModelMembersCollection>(() => ContainingAssembly.GetMembers(this), true);
			_flair = new Lazy<ReadOnlyCollection<IFlairTag>>(CreateFlairTags, true);
			_inheritanceData = new Lazy<InheritanceData>(() => new InheritanceData(Definition, x => NestedTypeDisplayNameOverlay.GetDisplayName(x)), true);
		}

		private ReadOnlyCollection<IFlairTag> CreateFlairTags(){
			Contract.Ensures(Contract.Result<ReadOnlyCollection<IFlairTag>>() != null);
			var results = new List<IFlairTag>();
			results.AddRange(base.FlairTags);

			if(Definition.IsEnum && Definition.HasFlagsAttribute())
				results.Add(DefaultFlagsFlair);

			if(!Definition.IsValueType && Definition.IsSealed && !Definition.IsDelegateType())
				results.Add(DefaultSealedFlair);

			return new ReadOnlyCollection<IFlairTag>(results);
		}

		protected ISimpleModelMembersCollection Members{
			get{
				Contract.Ensures(Contract.Result<ISimpleModelMembersCollection>() != null);
				return _members.Value;
			}
		}

		protected TypeDefinitionXmlDoc TypeXmlDocs { get { return DefinitionXmlDocs as TypeDefinitionXmlDoc; } }

		public override string Title {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				var nameGenerator = Definition.IsNested ? NestedTypeDisplayNameOverlay : RegularTypeDisplayNameOverlay;
				return nameGenerator.GetDisplayName(Definition);
			}
		}

		public override string NamespaceName {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				var definition = Definition;
				while (definition.DeclaringType != null){
					definition = definition.DeclaringType;
				}
				return definition.Namespace ?? String.Empty;
			}
		}

		public override string SubTitle {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
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

		public IList<ITypeSimpleModel> NestedTypes { get { return Members.Types; } }

		public IList<IDelegateSimpleModel> NestedDelegates { get { return Members.Delegates; } }

		public bool HasBaseChain { get { return BaseChain.Count > 0; } }

		public IList<ISimpleMemberPointerModel> BaseChain { get { return _inheritanceData.Value.BaseChain; } }

		public bool HasDirectInterfaces { get { return DirectInterfaces.Count > 0; } }

		public IList<ISimpleMemberPointerModel> DirectInterfaces { get { return _inheritanceData.Value.DirectImplementedInterfaces; } }

		public override IList<IFlairTag> FlairTags {
			get { return _flair.Value; }
		}

		

	}
}
