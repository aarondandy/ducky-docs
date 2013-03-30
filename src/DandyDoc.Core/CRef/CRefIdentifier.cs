using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace DandyDoc.CRef
{
	public class CRefIdentifier : IEquatable<CRefIdentifier>
	{

		protected static readonly Regex CrefRegex = new Regex(
			@"((?<targetType>\w)[:])?(?<coreName>[^():]+)([(](?<params>.*)[)])?",
			RegexOptions.Compiled);

		public CRefIdentifier(string cRef) {
			if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
			Contract.EndContractBlock();

			FullCRef = cRef;

			TargetType = String.Empty;
			CoreName = String.Empty;
			ParamParts = String.Empty;

			if ("N:".Equals(cRef)) {
				TargetType = "N";
			}
			else if (cRef.StartsWith("A:", StringComparison.InvariantCultureIgnoreCase)) {
				CoreName = cRef.Substring(2);
				TargetType = "A";
			}
			else {
				var match = CrefRegex.Match(cRef);
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
		}

		public string TargetType { get; private set; }

		public bool HasTargetType {
			[Pure] get {
				return !String.IsNullOrEmpty(TargetType);
			}
		}

		public bool IsTargetingType {
			[Pure] get {
				return "T".Equals(TargetType, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public string CoreName { get; private set; }

		public string ParamParts { get; private set; }

		public IList<string> ParamPartTypes {
			get {
				if (String.IsNullOrEmpty(ParamParts))
					return null;

				var results = new List<String>();
				int depth = 0;
				int partStartIndex = 0;
				for (int i = 0; i < ParamParts.Length; i++) {
					var c = ParamParts[i];
					switch (c) {
					case ',':
						if (depth == 0) {
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

				return results;
			}
		}

		public string FullCRef { get; private set; }

        public override string ToString() {
            return FullCRef;
        }

        public override int GetHashCode() {
            return FullCRef.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as CRefIdentifier);
        }

        public bool Equals(CRefIdentifier obj) {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.FullCRef == FullCRef;
        }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(!String.IsNullOrEmpty(FullCRef));
		}

	}
}
