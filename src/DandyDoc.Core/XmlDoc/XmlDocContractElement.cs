using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    public class XmlDocContractElement : XmlDocRefElement
    {

        [Obsolete]
        private static bool IsMatch(Regex regex, params string[] testItems) {
            Contract.Requires(regex != null);
            Contract.Requires(testItems != null);
            return Array.Exists(testItems, regex.IsMatch);
        }

        [Obsolete]
        public static bool RequiresParameterNotEverNull(IEnumerable<XmlDocContractElement> contracts, string parameterName) {
            if(contracts == null) throw new ArgumentNullException("contracts");
            Contract.EndContractBlock();
            if (String.IsNullOrEmpty(parameterName))
                return false;

            var notEqualNullRegex = new Regex(WrapRegexCodeTestLine(BuildNotNullRegexText(parameterName)));
            var notNullOrEmptyRegex = new Regex(WrapRegexCodeTestLine(WrapNotRegex(BuildIsNullOrEmptyRegexText(parameterName))));
            var notNullOrWhiteSpaceRegex = new Regex(WrapRegexCodeTestLine(WrapNotRegex(BuildIsNullOrWhiteSpaceRegexText(parameterName))));
            return contracts
                .Select(c =>
                    new[] {
                        c.CSharp,
                        c.VisualBasic,
                        c.Node.InnerText
                    }
                    .Where(x => !String.IsNullOrEmpty(x))
                    .ToArray()
                )
                .Where(s => s.Length > 0)
                .Any(s => IsMatch(notEqualNullRegex, s) || IsMatch(notNullOrEmptyRegex, s) || IsMatch(notNullOrWhiteSpaceRegex, s));
        }

        [Obsolete]
        private static readonly Regex _resultNotNullCodeRegex = new Regex(
           WrapRegexCodeTestLine(BuildNotNullRegexText("result")), RegexOptions.IgnoreCase);

        [Obsolete]
        private static readonly Regex _resultNotIsNullOrEmptyRegex = new Regex(
            @"^\s*([!]|Not\s+)\s*" + BuildIsNullOrEmptyRegexText("result") + @"\s*$", RegexOptions.IgnoreCase);

        [Obsolete]
        private static readonly Regex _resultNotIsNullOrWhiteSpaceRegex = new Regex(
            @"^\s*([!]|Not\s+)\s*" + BuildIsNullOrWhiteSpaceRegexText("result") + @"\s*$", RegexOptions.IgnoreCase);

        [Obsolete]
        private static string BuildIsNullOrEmptyRegexText(string parameterName){
            return @"(:?String[.])IsNullOrEmpty\s*[(]\s*" + parameterName + @"\s*[)]";
        }

        [Obsolete]
        private static string BuildIsNullOrWhiteSpaceRegexText(string parameterName) {
            return @"(:?String[.])IsNullOrWhiteSpace\s*[(]\s*" + parameterName + @"\s*[)]";
        }

        [Obsolete]
        private static string BuildNotNullRegexText(string parameterName){
            return String.Concat(
                "(:?",
                parameterName,@"\s*(:?!=\s*null|<>\s*Nothing)",
                "|",
                @"(:?null\s*!=|Nothing\s*<>)\s*", parameterName,
                ")"
            );
        }

        [Obsolete]
        private static string WrapRegexCodeTestLine(string regexText){
            return String.Concat(@"^\s*", regexText, @"\s*$");
        }

        [Obsolete]
        private static string WrapNotRegex(string regexText) {
            return String.Concat(@"(:?[!]|Not\s+)\s*", regexText);
        }

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

        public XmlDocContractElement(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children) {
            Contract.Requires(element != null);
            Contract.Requires(children == null || Contract.ForAll(children, x => x != null));
        }

        public bool IsRequires { get { return "REQUIRES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        public bool IsNormalEnsures { get { return "ENSURES".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        public bool IsEnsuresOnThrow { get { return "ENSURESONTHROW".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        public bool IsAnyEnsures { get { return IsNormalEnsures || IsEnsuresOnThrow; } }

        public bool IsInvariant { get { return "INVARIANT".Equals(Name, StringComparison.OrdinalIgnoreCase); } }

        public string CSharp {
            get { return Element.GetAttribute("csharp"); }
        }

        public string VisualBasic {
            get { return Element.GetAttribute("vb"); }
        }

        public string ExceptionCRef {
            get { return Element.GetAttribute("exception"); }
        }

        public override string CRef {
            get {
                var result = base.CRef;
                if (String.IsNullOrEmpty(result))
                    result = ExceptionCRef;

                return result;
            }
        }

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

        public bool RequiresParameterNotEqualNull(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotEqualNull(CSharp, parameterName)
                || CodeIsNotEqualNull(VisualBasic, parameterName);
        }

        public bool RequiresParameterNotNullOrEmpty(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotNullOrEmpty(CSharp, parameterName)
                || CodeIsNotNullOrEmpty(VisualBasic, parameterName);
        }

        public bool RequiresParameterNotNullOrWhiteSpace(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("bad parameter name", "parameterName");
            Contract.EndContractBlock();
            return CodeIsNotNullOrWhiteSpace(CSharp, parameterName)
                || CodeIsNotNullOrWhiteSpace(VisualBasic, parameterName);
        }

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

        public bool EnsuresResultNotEqualNull {
            get {
                if (!IsNormalEnsures)
                    return false;
                return CodeIsNotEqualNull(CSharp, ResultParameterName)
                    || CodeIsNotEqualNull(VisualBasic, ResultParameterName);
            }
        }

        public bool EnsuresResultNotNullOrEmpty{
            get{
                if (!IsNormalEnsures)
                    return false;
                return CodeIsNotNullOrEmpty(CSharp, ResultParameterName)
                    || CodeIsNotNullOrEmpty(VisualBasic, ResultParameterName);
            }
        }

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
