using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

				var results = new List<String>();
				int depth = 0;
				int partStartIndex = 0;
				for (int i = 0; i < ParamParts.Length; i++){
					var c = ParamParts[i];
					switch (c){
						case ',':
							if (depth == 0){
								results.Add(ParamParts.Substring(partStartIndex, i - partStartIndex));
								partStartIndex = i + 1;
							}
							break;
						case '[':
						case '(':
						case '<':
						case '{':
							depth++;
							break;
						case ']':
						case ')':
						case '>':
						case '}':
							depth--;
							break;
					}
				}

				if (partStartIndex < ParamParts.Length)
					results.Add(ParamParts.Substring(partStartIndex));

				return results.ToArray();
			}
		}

	}
}
