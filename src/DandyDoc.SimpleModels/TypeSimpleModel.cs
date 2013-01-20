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
						baseChain.Add(new ReferenceSimpleMemberPointer(currentReference, displayName));
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
					directInterfaces.AddRange(definition.Interfaces.Select(x => new ReferenceSimpleMemberPointer(x, getName(x))));
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
		private readonly Lazy<InheritanceData> _inheritanceData;
		private readonly Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>> _genericParameters;

		public TypeSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel)
			: base(definition, assemblyModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(assemblyModel != null);
			_members = new Lazy<ISimpleModelMembersCollection>(() => ContainingAssembly.GetMembers(this), true);
			_inheritanceData = new Lazy<InheritanceData>(() => new InheritanceData(Definition, x => NestedTypeDisplayNameOverlay.GetDisplayName(x)), true);
			_genericParameters = new Lazy<ReadOnlyCollection<IGenericParameterSimpleModel>>(CreateGenericParameters, true);
		}


		public override ISimpleModel DeclaringModel { get { return DeclaringTypeModel; } }

		public ITypeSimpleModel DeclaringTypeModel { get {
			return null; // TODO: figure out how to get this set
		} }

		private ReadOnlyCollection<IGenericParameterSimpleModel> CreateGenericParameters(){
			var results = new List<IGenericParameterSimpleModel>();
			if (Definition.HasGenericParameters){
				var xmlDocs = TypeXmlDocs;
				foreach (var genericParameterDefinition in Definition.GenericParameters){
					Contract.Assume(!String.IsNullOrEmpty(genericParameterDefinition.Name));
					IComplexTextNode summary = null;
					if (null != xmlDocs){
						var summaryParsedXml = xmlDocs.DocsForTypeparam(genericParameterDefinition.Name);
						if (null != summaryParsedXml && summaryParsedXml.Children.Count > 0){
							summary = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(summaryParsedXml.Children);
						}
					}

					var constraints = new List<IGenericParameterConstraint>();
					if(genericParameterDefinition.HasNotNullableValueTypeConstraint)
						constraints.Add(new ValueTypeGenericConstraint());
					else if(genericParameterDefinition.HasReferenceTypeConstraint)
						constraints.Add(new ReferenceTypeGenericConstraint());

					var constraintReferenceTypes = (IEnumerable<TypeReference>)genericParameterDefinition.Constraints;
					if(genericParameterDefinition.HasNotNullableValueTypeConstraint)
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


		protected ISimpleModelMembersCollection Members{
			get{
				Contract.Ensures(Contract.Result<ISimpleModelMembersCollection>() != null);
				return _members.Value;
			}
		}

		protected TypeDefinitionXmlDoc TypeXmlDocs { get { return DefinitionXmlDocs as TypeDefinitionXmlDoc; } }

		public override string Title {
			get{
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<String>()));
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

		public bool IsEnum { get { return Definition.IsEnum; } }

		public IList<ITypeSimpleModel> NestedTypes { get { return Members.NestedTypes; } }

		public IList<IDelegateSimpleModel> NestedDelegates { get { return Members.NestedDelegates; } }

		public IList<IMethodSimpleModel> Constructors { get { return Members.Constructors; } }

		public IList<IMethodSimpleModel> Methods { get { return Members.Methods; } }

		public IList<IMethodSimpleModel> Operators { get { return Members.Operators; } }

		public IList<IPropertySimpleModel> Properties { get { return Members.Properties; } }

		public IList<IFieldSimpleModel> Fields { get { return Members.Fields; } }

		public IList<IEventSimpleModel> Events { get { return Members.Events; } }

		public bool HasBaseChain { get { return BaseChain.Count > 0; } }

		public IList<ISimpleMemberPointerModel> BaseChain { get { return _inheritanceData.Value.BaseChain; } }

		public bool HasDirectInterfaces { get { return DirectInterfaces.Count > 0; } }

		public IList<ISimpleMemberPointerModel> DirectInterfaces { get { return _inheritanceData.Value.DirectImplementedInterfaces; } }

		public override IList<IFlairTag> FlairTags{
			get {
				var tags = new List<IFlairTag>();
				tags.AddRange(base.FlairTags);

				if (Definition.IsEnum && Definition.HasFlagsAttribute())
					tags.Add(DefaultFlagsFlair);

				if (!Definition.IsValueType && Definition.IsSealed && !Definition.IsDelegateType())
					tags.Add(DefaultSealedFlair);

				return tags;
			}
		}

		public bool HasGenericParameters { get { return GenericParameters.Count > 0; } }

		public IList<IGenericParameterSimpleModel> GenericParameters { get { return _genericParameters.Value; } }

	}
}
