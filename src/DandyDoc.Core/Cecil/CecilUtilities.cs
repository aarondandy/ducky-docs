using System;
using System.Diagnostics.Contracts;
using System.IO;
using Mono.Cecil;

namespace DandyDoc.Cecil
{
	public static class CecilUtilities
	{

		public static string GetFilePath(AssemblyDefinition assembly) {
			if(assembly == null) throw new ArgumentNullException("assembly");
			Contract.EndContractBlock();
			return new FileInfo(assembly.MainModule.FullyQualifiedName).FullName;
		}

	}
}
