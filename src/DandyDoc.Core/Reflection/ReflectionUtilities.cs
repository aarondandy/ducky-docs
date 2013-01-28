using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DandyDoc.Reflection
{
	public static class ReflectionUtilities
	{

		public static string GetFilePath(Assembly assembly) {
			if (assembly == null) throw new ArgumentNullException("assembly");
			Contract.EndContractBlock();

			var codeBase = assembly.CodeBase;
			Uri uri;
			if (Uri.TryCreate(codeBase, UriKind.Absolute, out uri) && "FILE".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
				return uri.AbsolutePath;

			return assembly.Location;
		}

	}
}
