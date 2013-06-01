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

        public static bool TryParse(Uri uri, out CRefIdentifier cRef) {
            if (uri != null) {
                var scheme = uri.Scheme;
                if (String.IsNullOrWhiteSpace(scheme) || "CREF".Equals(scheme, StringComparison.OrdinalIgnoreCase)) {
                    var decodedCRef = Uri.UnescapeDataString(uri.PathAndQuery);
                    if (!String.IsNullOrEmpty(decodedCRef)) {
                        cRef = new CRefIdentifier(decodedCRef);
                        return true;
                    }
                }
            }
            cRef = null;
            return false;
        }

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
            ParamPart = String.Empty;

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
                            ParamPart = paramsGroup.Value;
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
        public string ParamPart { get; private set; }

        private static List<string> ExtractParams(string paramPartText, char splitChar = ',') {
            Contract.Ensures(Contract.Result<List<string>>() != null);
            if (String.IsNullOrEmpty(paramPartText))
                return new List<string>(0);

            var results = new List<String>();
            int depth = 0;
            int partStartIndex = 0;
            for (int i = 0; i < paramPartText.Length; i++) {
                var c = paramPartText[i];
                if (c == splitChar) {
                    if (depth == 0) {
                        results.Add(paramPartText.Substring(partStartIndex, i - partStartIndex));
                        partStartIndex = i + 1;
                    }
                }
                else {
                    switch (c) {
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
            }

            if (partStartIndex <= paramPartText.Length)
                results.Add(paramPartText.Substring(partStartIndex));

            return results;
        }

        /// <summary>
        /// Generates a list of parsed parameter types.
        /// </summary>
        public List<string> ParamPartTypes {
            get {
                Contract.Ensures(Contract.Result<List<string>>() != null);
                return ExtractParams(ParamPart);
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

        public Uri ToUri() {
            Contract.Ensures(Contract.Result<Uri>() != null);
            return new Uri("cref:" + Uri.EscapeDataString(FullCRef), UriKind.Absolute);
        }

        [Obsolete]
        public CRefIdentifier GetGenericDefinitionCRef() {
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            var coreNameParts = ExtractParams(CoreName,'.');
            var hasParamText = !String.IsNullOrWhiteSpace(ParamPart);
            if (coreNameParts.Count > 0) {
                var mayBeMethod = String.Equals("M",TargetType, StringComparison.OrdinalIgnoreCase)
                    || (
                        !String.Equals("T", TargetType, StringComparison.OrdinalIgnoreCase)
                        && hasParamText
                    );
                var lastIndex = coreNameParts.Count - 1;
                coreNameParts[lastIndex] = NamePartToGenericCardinality(coreNameParts[lastIndex], tickCount: mayBeMethod ? 2 : 1);

                for (int i = coreNameParts.Count - 1; i >= 0; i--) {
                    coreNameParts[i] = NamePartToGenericCardinality(coreNameParts[i]);
                }
            }

            var result = String.Join(".", coreNameParts);
            if (!String.IsNullOrWhiteSpace(TargetType))
                result = String.Concat(TargetType, ':', result);

            if (hasParamText) {
                var paramParts = ParamPartTypes.ConvertAll(t => NamePartToGenericCardinality(t));
                result = String.Concat(result, '(', String.Join(",", paramParts), ')');
            }

            return new CRefIdentifier(String.IsNullOrEmpty(result) ? "!:" : result);
        }

        private string NamePartToGenericCardinality(string part, int tickCount = 1) {
            var genericParamListOpenAt = part.IndexOfAny(new[] {'{', '<', '(', '['});
            var firstParamPartChar = genericParamListOpenAt + 1;
            if (genericParamListOpenAt < 0 || firstParamPartChar >= part.Length)
                return part; // if an open is not found, don't mess with it
            var genericParamListCloseAt = part.LastIndexOfAny(new[] {'}', '>', ')', ']'});
            if (genericParamListCloseAt != part.Length - 1)
                return part; // must be the last character

            var correctedParts = ExtractParams(part.Substring(firstParamPartChar));
            var tickText = "`";
            for (int tickIndex = 1; tickIndex < tickCount; tickIndex++)
                tickText += '`';
            return String.Concat(part.Substring(0, genericParamListOpenAt), tickText, correctedParts.Count);
        }

    }
}
