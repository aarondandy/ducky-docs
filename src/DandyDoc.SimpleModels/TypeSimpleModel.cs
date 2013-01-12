using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class TypeSimpleModel : ITypeSimpleModel
	{

		private static readonly DisplayNameOverlay RegularTypeDisplayNameOverlay = new DisplayNameOverlay{
			ShowTypeNameForMembers = false
		};

		private static readonly DisplayNameOverlay NestedTypeDisplayNameOverlay = new DisplayNameOverlay{
			ShowTypeNameForMembers = true
		};

		private static readonly DisplayNameOverlay FullTypeDisplayNameOverlay = new DisplayNameOverlay {
			ShowTypeNameForMembers = true,
			IncludeNamespaceForTypes = true
		};

		public TypeSimpleModel(TypeDefinition definition, IAssemblySimpleModel assemblyModel){
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == assemblyModel) throw new ArgumentNullException("assemblyModel");
			Contract.EndContractBlock();
			Definition = definition;
			ContainingAssembly = assemblyModel;
		}

		protected TypeDefinition Definition { get; private set; }

		public IAssemblySimpleModel ContainingAssembly { get; private set; }

		public virtual string DisplayName {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				var nameGenerator = Definition.IsNested ? NestedTypeDisplayNameOverlay : RegularTypeDisplayNameOverlay;
				return nameGenerator.GetDisplayName(Definition);
			}
		}

		public virtual string FullName {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return FullTypeDisplayNameOverlay.GetDisplayName(Definition);
			}
		}

		public virtual string CRef {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return ContainingAssembly.CrefOverlay.GetCref(Definition);
			}
		}

		public virtual string Title {
			get{
				Contract.Ensures(Contract.Result<string>() != null);
				return DisplayName;
			}
		}

		public virtual string NamespaceName{ get { return Definition.Namespace; } }

		public virtual string SubTitle {
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

		public ISimpleModelRepository RootRepository {
			get{
				Contract.Ensures(Contract.Result<ISimpleModelRepository>() != null);
				return ContainingAssembly.RootRepository;
			}
		}

		public IList<ITypeSimpleModel> NestedTypes { get { throw new NotImplementedException(); } }

		public IList<IDelegateSimpleModel> NestedDelegates { get { throw new NotImplementedException(); } }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Definition != null);
			Contract.Invariant(ContainingAssembly != null);
		}

		
	}
}
