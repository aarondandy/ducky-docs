using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace DandyDoc
{
	public class AssemblyDefinitionCollection : Collection<AssemblyDefinition>
	{

		// TODO: need some stuff to syncronize access to the assembly definitions as I don't think they are thread safe

		public static AssemblyDefinition LoadAssemblyDefinition(string filePath) {
			if (String.IsNullOrEmpty(filePath)) throw new ArgumentException("Invalid file path.", "filePath");
			Contract.Ensures(Contract.Result<AssemblyDefinition>() != null);

			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
				throw new FileNotFoundException("The given file was not found.", fileInfo.FullName);

			var assemblyDefinition = AssemblyDefinition.ReadAssembly(fileInfo.FullName);
			if (null == assemblyDefinition)
				throw new ArgumentException("Failed to load the given assembly from '" + fileInfo.FullName + '\'', "filePath");
			return assemblyDefinition;
		}

		public AssemblyDefinitionCollection() { }

		public AssemblyDefinitionCollection(params string[] filePaths)
			: this(Array.ConvertAll(filePaths, LoadAssemblyDefinition))
		{ Contract.Requires(null != filePaths); }

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
				if(null == definitions) throw new ArgumentException("Enumerable contains a null element.","definitions");
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

	}
}
