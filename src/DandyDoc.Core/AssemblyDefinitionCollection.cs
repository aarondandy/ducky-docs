using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc
{
	[Obsolete]
	public class AssemblyDefinitionCollection : Collection<AssemblyDefinition>
	{

		private class ImmediateAssemblyResolver : IAssemblyResolver
		{

			private readonly IAssemblyResolver _core;
			private readonly ReaderParameters _immediateParams;

			public ImmediateAssemblyResolver(){
				_core = new DefaultAssemblyResolver();
				_immediateParams = new ReaderParameters(ReadingMode.Immediate);
				_immediateParams.AssemblyResolver = this;
			}

			public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters){
				return _core.Resolve(fullName, parameters);
			}

			public AssemblyDefinition Resolve(string fullName) {
				return _core.Resolve(fullName, _immediateParams);
			}

			public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
				return _core.Resolve(name, parameters);
			}

			public AssemblyDefinition Resolve(AssemblyNameReference name) {
				return _core.Resolve(name, _immediateParams);
			}

		}

		public static AssemblyDefinition LoadAssemblyDefinition(string filePath){
			Contract.Requires(!String.IsNullOrEmpty(filePath));
			Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);
			return LoadAssemblyDefinition(filePath, false);
		}

		public static AssemblyDefinition LoadAssemblyDefinition(string filePath, bool immediate) {
			if (String.IsNullOrEmpty(filePath)) throw new ArgumentException("Invalid file path.", "filePath");
			Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);

			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
				throw new FileNotFoundException("The given file was not found.", fileInfo.FullName);

			AssemblyDefinition assemblyDefinition;
			if (immediate){
				var readerParams = new ReaderParameters(ReadingMode.Immediate);
				readerParams.AssemblyResolver = new ImmediateAssemblyResolver();
				assemblyDefinition = AssemblyDefinition.ReadAssembly(fileInfo.FullName, readerParams);
			}
			else{
				assemblyDefinition = AssemblyDefinition.ReadAssembly(fileInfo.FullName);
			}

			if (null == assemblyDefinition)
				throw new ArgumentException("Failed to load the given assembly from '" + fileInfo.FullName + '\'', "filePath");
			return assemblyDefinition;
		}

		public AssemblyDefinitionCollection() { }

		public AssemblyDefinitionCollection(bool immediate, params string[] filePaths)
			: this(Array.ConvertAll(filePaths, p => LoadAssemblyDefinition(p, immediate))) { Contract.Requires(null != filePaths); }

		public AssemblyDefinitionCollection(params string[] filePaths)
			: this(Array.ConvertAll(filePaths, LoadAssemblyDefinition))
		{ Contract.Requires(null != filePaths); }

		public AssemblyDefinitionCollection(bool immediate, IEnumerable<string> filePaths)
			: this(filePaths.Select(p => LoadAssemblyDefinition(p, immediate))) { Contract.Requires(null != filePaths); }

		public AssemblyDefinitionCollection(IEnumerable<string> filePaths)
			: this(filePaths.Select(LoadAssemblyDefinition))
		{ Contract.Requires(null != filePaths); }

		public AssemblyDefinitionCollection(IEnumerable<AssemblyDefinition> definitions)
		{
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.EndContractBlock();
			AddRange(definitions);
		}

		public void AddRange(IEnumerable<AssemblyDefinition> definitions) {
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.EndContractBlock();
			foreach (var definition in definitions) {
				if(null == definition) throw new ArgumentException("Enumerable contains a null element.","definitions");
				Add(definition);
			}
		}

		protected override void InsertItem(int index, AssemblyDefinition item) {
			if(null == item) throw new ArgumentNullException("item");
			Contract.EndContractBlock();
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, AssemblyDefinition item) {
			if(null == item) throw new ArgumentNullException("item");
			Contract.EndContractBlock();
			base.SetItem(index, item);
		}

		public bool ContainsDefinition(MemberReference reference){
			if (null == reference)
				return false;
			var module = reference.Module;
			if (null == module)
				return false;
			var assembly = module.Assembly;
			return Contains(assembly);
		}

	}
}
