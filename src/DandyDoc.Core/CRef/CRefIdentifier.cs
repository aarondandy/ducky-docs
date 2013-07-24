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
        private static readonly Regex CRefRegex;

        /// <summary>
        /// An invalid code reference identifier.
        /// </summary>
        public static CRefIdentifier Invalid { get; private set; }

        static CRefIdentifier() {
            CRefRegex = new Regex(
                @"((?<targetType>\w)[:])?(?<coreName>[^():]+)([(](?<params>.*)[)])?([~](?<returnType>.+))?",
                RegexOptions.Compiled);
            Invalid = new CRefIdentifier("!:");
        }

        /// <summary>
        /// Attempts to parse a code reference from a URI.
        /// </summary>
        /// <param name="uri">The URI that encodes a code reference.</param>
        /// <param name="cRef">The resulting code reference if generation is possible.</param>
        /// <returns><c>true</c> if a code reference was extracted from the URI.</returns>
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
            ReturnTypePart = String.Empty;

            if ("N:".Equals(cRef)) {
                Contract.Assume(cRef.Length == 2);
                CoreName = cRef.Substring(2);
                TargetType = "N";
            }
            else if (cRef.StartsWith("A:", StringComparison.InvariantCultureIgnoreCase)) {
                Contract.Assume(cRef.Length == 2);
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
                        if (paramsGroup.Success)
                            ParamPart = paramsGroup.Value;

                        var returnTypeGroup = match.Groups["returnType"];
                        if (returnTypeGroup.Success)
                            ReturnTypePart = returnTypeGroup.Value;
                    }
                }
            }
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(!String.IsNullOrEmpty(FullCRef));
            Contract.Invariant(Invalid != null);
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

        /// <summary>
        /// The return type part of the code reference.
        /// </summary>
        /// <remarks>
        /// Return types in code references are only expected with conversion operators.
        /// </remarks>
        public string ReturnTypePart { get; private set; }

        internal static List<string> ExtractParams(string paramPartText, char splitChar = ',') {
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

        /// <summary>
        /// Gets a URI representation of this code reference.
        /// </summary>
        /// <returns>A URI for the given code reference.</returns>
        public Uri ToUri() {
            Contract.Ensures(Contract.Result<Uri>() != null);
            return new Uri("cref:" + Uri.EscapeDataString(FullCRef), UriKind.Absolute);
        }

        /// <summary>
        /// Creates a new code reference with the same core name but with the given target type.
        /// </summary>
        /// <param name="targetType">The desired target type for the derived code reference.</param>
        /// <returns>A code reference based on this code reference with the given target type.</returns>
        public CRefIdentifier WithTargetType(string targetType){
            if(String.IsNullOrWhiteSpace(targetType))
                targetType = "!";

            var primaryCRefPart = FullCRef;
            var firstColonIndex = primaryCRefPart.IndexOf(':');
            if (firstColonIndex >= 0)
                primaryCRefPart = primaryCRefPart.Substring(firstColonIndex + 1);
            return new CRefIdentifier(String.Concat(targetType, ':', primaryCRefPart));

        }
    }
}
