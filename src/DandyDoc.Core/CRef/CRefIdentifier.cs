using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace DandyDoc.CRef
{
    /// <summary>
    /// A code reference (cref) identifier with parsed segments.
    /// </summary>
    public sealed class CRefIdentifier : IEquatable<CRefIdentifier>
    {

        /// <summary>
        /// The core parser regular expression.
        /// </summary>
        private static readonly Regex CRefRegex = new Regex(
            @"((?<targetType>\w)[:])?(?<coreName>[^():]+)([(](?<params>.*)[)])?",
            RegexOptions.Compiled);

        /// <summary>
        /// Creates a new code reference identifier by parsing it from a given string.
        /// </summary>
        /// <param name="cRef">The code reference string to parse.</param>
        public CRefIdentifier(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
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
                var match = CRefRegex.Match(cRef);
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

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(!String.IsNullOrEmpty(FullCRef));
        }

        /// <summary>
        /// The explicit target type prefix of the code reference.
        /// </summary>
        /// <remarks>
        /// The target type is a string that should contain only a single letter and will exclude the
        /// leading ':' separator character. In some instances involving resolution errors during XML doc
        /// compilation the target  type may be a string containing the '!' character.
        /// </remarks>
        public string TargetType { get; private set; }

        /// <summary>
        /// Determines if this code reference contains an explicit target type.
        /// </summary>
        public bool HasTargetType {
            [Pure] get {
                return !String.IsNullOrEmpty(TargetType);
            }
        }

        /// <summary>
        /// The core referenced name as parsed.
        /// </summary>
        public string CoreName { get; private set; }

        /// <summary>
        /// The parsed parameters if they exist.
        /// </summary>
        public string ParamParts { get; private set; }

        /// <summary>
        /// Generates a list of parsed parameter types.
        /// </summary>
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

        /// <summary>
        /// The full code reference as was parsed.
        /// </summary>
        public string FullCRef { get; private set; }

        /// <inheritdoc/>
        public override string ToString() {
            return FullCRef;
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return FullCRef.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            return Equals(obj as CRefIdentifier);
        }

        /// <inheritdoc/>
        public bool Equals(CRefIdentifier obj) {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.FullCRef == FullCRef;
        }

    }
}
