using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DandyDoc.Core.Utility
{
	internal static class NameUtilities
	{

		public static string ParentDotSeperatedName(string name) {
			if (String.IsNullOrEmpty(name))
				return null;

			int dotIndex = name.LastIndexOf('.');
			if (dotIndex <= 0)
				return name;

			return name.Substring(0, dotIndex);
		}

		public static string GetLastNamePart(string name) {
			if (String.IsNullOrEmpty(name))
				return null;

			int dotIndex = name.LastIndexOf('.');
			if (dotIndex <= 0)
				return name;

			return name.Substring(dotIndex+1);
		}

	}
}
