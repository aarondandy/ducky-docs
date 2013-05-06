using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    public class XmlDocContractElement : XmlDocRefElement
    {

        private static readonly Regex _resultNotNullCodeRegex = new Regex(
           WrapRegexCodeTestLine(BuildNotNullRegexText("result")), RegexOptions.IgnoreCase);
        private static readonly Regex _resultNotIsNullOrEmptyRegex = new Regex(
            @"^\s*([!]|Not\s+)\s*" + BuildIsNullOrEmptyRegexText("result") + @"\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex _resultNotIsNullOrWhiteSpaceRegex = new Regex(
            @"^\s*([!]|Not\s+)\s*" + BuildIsNullOrWhiteSpaceRegexText("result") + @"\s*$", RegexOptions.IgnoreCase);

        private static string BuildIsNullOrEmptyRegexText(string parameterName){
            return @"(:?String[.])IsNullOrEmpty\s*[(]\s*" + parameterName + @"\s*[)]";
        }

        private static string BuildIsNullOrWhiteSpaceRegexText(string parameterName) {
            return @"(:?String[.])IsNullOrWhiteSpace\s*[(]\s*" + parameterName + @"\s*[)]";
        }

        private static string BuildNotNullRegexText(string parameterName){
            return String.Concat(
                "(:?",
                parameterName,@"\s*(:?!=\s*null|<>\s*Nothing)",
                "|",
                @"(:?null\s*!=|Nothing\s*<>)\s*", parameterName,
                ")"
            );
        }

        private static string WrapRegexCodeTestLine(string regexText){
            return String.Concat(@"^\s*", regexText, @"\s*$");
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

        public bool IsParameterNotNull(string parameterName){
            if (!IsRequires)
                return false;

            var regex = new Regex(WrapRegexCodeTestLine(BuildNotNullRegexText(parameterName)), RegexOptions.IgnoreCase);

            var code = CSharp;
            if (!String.IsNullOrEmpty(code) && regex.IsMatch(code))
                return true;
            code = VisualBasic;
            if (!String.IsNullOrEmpty(code) && regex.IsMatch(code))
                return true;
            code = Node.InnerText;
            if (!String.IsNullOrEmpty(code) && regex.IsMatch(code))
                return true;

            return false;
        }

        public bool EnsuresResultNotNull {
            get {
                if (!IsNormalEnsures)
                    return false;
                var code = CSharp;
                if (!String.IsNullOrEmpty(code) && _resultNotNullCodeRegex.IsMatch(code))
                    return true;
                code = VisualBasic;
                if (!String.IsNullOrEmpty(code) && _resultNotNullCodeRegex.IsMatch(code))
                    return true;
                code = Node.InnerText;
                if (!String.IsNullOrEmpty(code) && _resultNotNullCodeRegex.IsMatch(code))
                    return true;
                return false;
            }
        }

        public bool EnsuresResultNotNullOrEmpty{
            get{
                if (!IsNormalEnsures)
                    return false;
                var code = CSharp;
                if (!String.IsNullOrEmpty(code) && _resultNotIsNullOrEmptyRegex.IsMatch(code))
                    return true;
                code = VisualBasic;
                if(!String.IsNullOrEmpty(code) && _resultNotIsNullOrEmptyRegex.IsMatch(code))
                    return true;
                code = Node.InnerText;
                if (!String.IsNullOrEmpty(code) && _resultNotIsNullOrEmptyRegex.IsMatch(code))
                    return true;
                return false;
            }
        }

        public bool EnsuresResultNotNullOrWhiteSpace {
            get {
                if (!IsNormalEnsures)
                    return false;
                var code = CSharp;
                if (!String.IsNullOrEmpty(code) && _resultNotIsNullOrWhiteSpaceRegex.IsMatch(code))
                    return true;
                code = VisualBasic;
                if (!String.IsNullOrEmpty(code) && _resultNotIsNullOrWhiteSpaceRegex.IsMatch(code))
                    return true;
                code = Node.InnerText;
                if (!String.IsNullOrEmpty(code) && _resultNotIsNullOrWhiteSpaceRegex.IsMatch(code))
                    return true;
                return false;
            }
        }


    }
}
