using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Xml;

namespace DandyDoc.XmlDoc
{

    /// <summary>
    /// An XML doc code contract element.
    /// </summary>
    /// <seealso href="http://research.microsoft.com/en-us/projects/contracts/"/>
    public class XmlDocContractElement : XmlDocRefElement
    {

        private const string ResultParameterName = "result";

        private static readonly Regex ExtractParameterNotEqualNullParamRegex = new Regex(
            @"^\s*(:?(?<paramLeft>\S+)\s*(:?!=\s*null|<>\s*Nothing)|(:?null\s*!=|Nothing\s*<>)\s*(?<paramRight>\S+))\s*$",
            RegexOptions.IgnoreCase);

        private static bool CodeIsNotEqualNull(string code, string parameterName) {
            Contract.Requires(!String.IsNullOrEmpty(parameterName));
            if (String.IsNullOrEmpty(code))
                return false;
            var match = ExtractParameterNotEqualNullParamRegex.Match(code);
            return match.Success
                && (
                    String.Equals(match.Groups["paramLeft"].Value, parameterName, StringComparison.OrdinalIgnoreCase)
                    || String.Equals(match.Groups["paramRight"].Value, parameterName, StringComparison.OrdinalIgnoreCase)
                );
        }

        private static readonly Regex ExtractParameterNotIsNullOrEmptyRegex = new Regex(
            @"^\s*(:?[!]|Not\s+)\s*(:?String[.])?IsNullOrEmpty\s*[(]\s*(?<param>\S+)\s*[)]\s*$", RegexOptions.IgnoreCase);

        private static bool CodeIsNotNullOrEmpty(string code, string parameterName) {
            Contract.Requires(!String.IsNullOrEmpty(parameterName));
            if (String.IsNullOrEmpty(code))
                return false;
            var match = ExtractParameterNotIsNullOrEmptyRegex.Match(code);
            return match.Success
                && String.Equals(match.Groups["param"].Value, parameterName, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly Regex ExtractParameterNotIsNullOrWhiteSpaceRegex = new Regex(
            @"^\s*(:?[!]|Not\s+)\s*(:?String[.])?IsNullOrWhiteSpace\s*[(]\s*(?<param>\S+)\s*[)]\s*$", RegexOptions.IgnoreCase);

        private static bool CodeIsNotNullOrWhiteSpace(string code, string parameterName) {
            Contract.Requires(!String.IsNullOrEmpty(parameterName));
            if (String.IsNullOrEmpty(code))
                return false;
            var match = ExtractParameterNotIsNullOrWhiteSpaceRegex.Match(code);
            return match.Success
                && String.Equals(match.Groups["param"].Value, parameterName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates an XML doc code contract element.
        /// </summary>
        /// <param name="element">The raw XML element to wrap.</param>
        /// <param name="children">The child XML doc nodes.</param>
        public XmlDocContractElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        /// <summary>
        /// Indicates that this is a requires contract element.
        /// </summary>
        public bool IsRequires { get { return "REQUIRES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        /// <summary>
        /// Indicates that this is an ensures contract element for normal termination.
        /// </summary>
        public bool IsNormalEnsures { get { return "ENSURES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        /// <summary>
        /// Indicates that this is an ensures contract element for exceptional termination.
        /// </summary>
        public bool IsEnsuresOnThrow { get { return "ENSURESONTHROW".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        /// <summary>
        /// Indicates that this is an ensures contract element of any kind.
        /// </summary>
        public bool IsAnyEnsures { get { return IsNormalEnsures || IsEnsuresOnThrow; } }

        /// <summary>
        /// Indicates that this is an invariant contract element.
        /// </summary>
        public bool IsInvariant { get { return "INVARIANT".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        /// <summary>
        /// The C# code for the contract.
        /// </summary>
        public string CSharp {
            get { return Element.GetAttribute("csharp"); }
        }

        /// <summary>
        /// The VB code for the contract.
        /// </summary>
        public string VisualBasic {
            get { return Element.GetAttribute("vb"); }
        }

        /// <summary>
        /// The code reference (cref) of an exception associated with this contract.
        /// </summary>
        public string ExceptionCRef {
            get { return Element.GetAttribute("exception"); }
        }

        /// <summary>
        /// The context sensitive code reference (cref) associated with this contract.
        /// </summary>
        public override string CRef {
            get {
                var result = base.CRef;
                if (String.IsNullOrEmpty(result))
                    result = ExceptionCRef;

                return result;
            }
        }

        /// <summary>
        /// Determines if this contract is a requirement that a given parameter is not null due to various possible forms of null checking.
        /// </summary>
        /// <param name="parameterName">The parameter name to test for.</param>
        /// <returns><c>true</c> when the parameter can not be null.</returns>
        public bool RequiresParameterNotEverNull(string parameterName) {
            if(String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return IsRequires
                && (
                    RequiresParameterNotEqualNull(parameterName)
                    || RequiresParameterNotNullOrEmpty(parameterName)
                    || RequiresParameterNotNullOrWhiteSpace(parameterName)
                );
        }

        /// <summary>
        /// Determines if this contract is a requirement that a given parameter is not exactly null.
        /// </summary>
        /// <param name="parameterName">The parameter name to test for.</param>
        /// <returns><c>true</c> when the parameter can not be equal to null.</returns>
        public bool RequiresParameterNotEqualNull(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotEqualNull(CSharp, parameterName)
                || CodeIsNotEqualNull(VisualBasic, parameterName);
        }

        /// <summary>
        /// Determines if this contract is a requirement that a given parameter is not null or empty.
        /// </summary>
        /// <param name="parameterName">The parameter name to test for.</param>
        /// <returns><c>true</c> when the parameter can not be null or empty.</returns>
        /// <remarks>
        /// This check may test positive for contracts other than !String.IsNullOrEmpty.
        /// </remarks>
        public bool RequiresParameterNotNullOrEmpty(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotNullOrEmpty(CSharp, parameterName)
                || CodeIsNotNullOrEmpty(VisualBasic, parameterName);
        }

        /// <summary>
        /// Determines if this contract is a requirement that a given parameter is not null or white space.
        /// </summary>
        /// <param name="parameterName">The parameter name to test for.</param>
        /// <returns><c>true</c> when the parameter can not be null or white space.</returns>
        public bool RequiresParameterNotNullOrWhiteSpace(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotNullOrWhiteSpace(CSharp, parameterName)
                || CodeIsNotNullOrWhiteSpace(VisualBasic, parameterName);
        }

        /// <summary>
        /// Determines if this contract ensures the result is not null due to various possible forms of null checking.
        /// </summary>
        public bool EnsuresResultNotEverNull {
            get {
                return IsNormalEnsures
                    && (
                        EnsuresResultNotEqualNull
                        || EnsuresResultNotNullOrEmpty
                        || EnsuresResultNotNullOrWhiteSpace
                    );
            }
        }

        /// <summary>
        /// Determines if this contract ensures the result is not equal to null.
        /// </summary>
        public bool EnsuresResultNotEqualNull {
            get {
                if (!IsNormalEnsures)
                    return false;
                return CodeIsNotEqualNull(CSharp, ResultParameterName)
                    || CodeIsNotEqualNull(VisualBasic, ResultParameterName);
            }
        }

        /// <summary>
        /// Determines if this contract ensures the result is not null or empty.
        /// </summary>
        /// <remarks>
        /// This check may test positive for contracts other than !String.IsNullOrEmpty.
        /// </remarks>
        public bool EnsuresResultNotNullOrEmpty{
            get{
                if (!IsNormalEnsures)
                    return false;
                return CodeIsNotNullOrEmpty(CSharp, ResultParameterName)
                    || CodeIsNotNullOrEmpty(VisualBasic, ResultParameterName);
            }
        }

        /// <summary>
        /// Determins if this contract ensures the result is not null or white space.
        /// </summary>
        public bool EnsuresResultNotNullOrWhiteSpace {
            get {
                if (!IsNormalEnsures)
                    return false;
                return CodeIsNotNullOrWhiteSpace(CSharp, ResultParameterName)
                    || CodeIsNotNullOrWhiteSpace(VisualBasic, ResultParameterName);
            }
        }


    }
}
