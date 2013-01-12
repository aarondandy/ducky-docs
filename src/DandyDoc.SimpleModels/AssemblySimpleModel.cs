using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.Overlays.ExternalVisibility;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class AssemblySimpleModel : IAssemblySimpleModel, ISimpleModelAssemblyRepository
	{

		private readonly object _repositoryMutex = new object();
		private readonly Lazy<ReadOnlyCollection<ITypeSimpleModel>> _types;

		public AssemblySimpleModel(AssemblyDefinition assemblyDefinition, ISimpleModelRepository repository) {
			if (null == assemblyDefinition) throw new ArgumentNullException("assemblyDefinition");
			if (null == repository) throw new ArgumentNullException("repository");
			Contract.EndContractBlock();
			Definition = assemblyDefinition;
			RootRepository = repository;
			_types = new Lazy<ReadOnlyCollection<ITypeSimpleModel>>(GenerateTypeViewModels, true);
		}

		private ReadOnlyCollection<ITypeSimpleModel> GenerateTypeViewModels() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<ITypeSimpleModel>>() != null);
			lock (_repositoryMutex) {
				var definitions = Definition.Modules.SelectMany(x => x.Types.Where(TypeFilter));
				return Array.AsReadOnly(definitions.Select(CreateTypeSimpleModelInstance).ToArray());
			}
		}

		protected virtual ITypeSimpleModel CreateTypeSimpleModelInstance(TypeDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<ITypeSimpleModel>() != null);
			if(definition.IsDelegateType())
				return new DelegateSimpleModel();
			return new TypeSimpleModel();
		}

		protected virtual bool TypeFilter(TypeDefinition definition) {
			Contract.Requires(definition != null);
			return definition.IsExternallyVisible();
		}

		// ------------ Public access

		public IAssemblySimpleModel Assembly {
			get { return this; }
		}

		public ISimpleModelRepository RootRepository { get; private set; }

		protected AssemblyDefinition Definition { get; private set; }

		public virtual string DisplayName {
			get { return Definition.Name.Name; }
		}

		public virtual string FullName {
			get { return Definition.FullName; }
		}

		public virtual string CRef {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				return "A:" + FullName;
			}
		}

		public virtual string AssemblyFileName {
			get { return new FileInfo(Definition.MainModule.FullyQualifiedName).Name; }
		}

		public IList<ITypeSimpleModel> Types {
			get {
				throw new NotImplementedException();
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariants() {
			Contract.Invariant(Definition != null);
			Contract.Invariant(RootRepository != null);
		}

	}
}
