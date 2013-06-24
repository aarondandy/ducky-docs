using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ServiceStack.Html;

namespace DandyDoc.Web.ServiceStack
{
    public static class DandyDocHelpers
    {

        private static readonly Regex CodeNameSplitRegex = new Regex(@"(?<=[.,;\<\(\[])");

        public static string[] CodeNameParts(string title) {
            return CodeNameSplitRegex.Split(title);
        }

        public static HtmlString BreakIntoCodeNamePartElements(this HtmlHelper helper, string name, string tagName = "span") {
            var htmlBuilder = new StringBuilder();
            var openTag = '<' + tagName + '>';
            var closeTag = "</" + tagName + '>';
            foreach (var part in CodeNameParts(name)) {
                htmlBuilder.Append(openTag);
                htmlBuilder.Append(HttpUtility.HtmlEncode(part));
                htmlBuilder.Append(closeTag);
            }
            return new HtmlString(htmlBuilder.ToString());
        }

    }
}