using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DandyDoc.Core.Overlays.Cref
{
	public class ParsedCref
	{

		public static ParsedCref Parse(string cref) {
			return new ParsedCref(cref);
		}

		private static readonly Regex CrefRegex = new Regex(
			@"((?<targetType>\w)[:])?(?<coreName>[^()]+)([(](?<params>.*)[)])?",
			RegexOptions.Compiled);

		public ParsedCref(string cref) {
			Cref = cref;

			var match = CrefRegex.Match(cref);
			if (match.Success) {
				var coreGroup = match.Groups["coreName"];
				if (coreGroup.Success) {
					CoreName = coreGroup.Value;
					var targetTypeGroup = match.Groups["targetType"];
					if (targetTypeGroup.Success)
						TargetType = targetTypeGroup.Value;
					var paramsGroup = match.Groups["params"];
					if (paramsGroup.Success) {
						ParamParts = paramsGroup.Value;
					}
				}
			}
		}

		public string Cref { get; private set; }

		public string TargetType { get; private set; }

		public string CoreName { get; private set; }

		public string[] CoreNameParts {
			get {
				if(String.IsNullOrEmpty(CoreName))
					return new string[0];
				return CoreName.Split('.');
			}
		}

		public string ParamParts { get; private set; }

		public string[] ParamPartTypes {
			get {
				if (String.IsNullOrEmpty(ParamParts))
					return new string[0];
				return ParamParts.Split(',');
			}
		}

	}
}
