using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DandyDoc.CodeDoc;
using System.Text;
using System.Web.Mvc.Html;
using DandyDoc.XmlDoc;

namespace DandyDoc.Web.Mvc4.Helpers
{
    public static class DandyDocHelpers
    {

        public static MvcHtmlString CRefActionLink(this HtmlHelper helper, MvcHtmlString innerHtml, string cRef){
            var linkBuilder = new TagBuilder("a");
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            linkBuilder.MergeAttribute("href", urlHelper.Action("Api", new { cRef }));
            if(innerHtml != null)
                linkBuilder.InnerHtml = innerHtml.ToString();
            return new MvcHtmlString(linkBuilder.ToString());
        }

        public static MvcHtmlString CodeDocEntityTable(this HtmlHelper helper, IEnumerable<ICodeDocEntity> entities, object tableTagAttributes = null) {
            var builder = new TagBuilder("table");

            if(tableTagAttributes != null)
                builder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var tableInnerHtmlBuilder = new StringBuilder("<thead><tr><th><i class=\"icon-info-sign\"></i></th><th>Name</th><th>Description</th></tr></thead><tbody>");
            foreach(var entity in entities){
                tableInnerHtmlBuilder.Append("<tr><td>");
                // TODO: flair
                tableInnerHtmlBuilder.Append("</td><td>");

                tableInnerHtmlBuilder.Append(helper.CRefActionLink(new MvcHtmlString(entity.ShortName), entity.CRef.FullCRef));

                tableInnerHtmlBuilder.Append("</td><td>");
                if (entity.HasSummary)
                    tableInnerHtmlBuilder.Append(helper.XmlDocContainedHtml(entity.Summary));

                tableInnerHtmlBuilder.Append("</td></tr>");
            }
            tableInnerHtmlBuilder.Append("</tbody>");
            builder.InnerHtml = tableInnerHtmlBuilder.ToString();

            return new MvcHtmlString(builder.ToString());
        }

        [Obsolete("This should not exist, instead a SummaryContent property for example would be better.")]
        public static MvcHtmlString XmlDocContainedHtml(this HtmlHelper helper, XmlDocNode node){
            if (node.HasChildren)
                return helper.XmlDocHtml(node.Children);
            return MvcHtmlString.Empty;
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, IEnumerable<XmlDocNode> nodes){
            var htmlBuilder = new StringBuilder();
            foreach (var node in nodes)
                htmlBuilder.Append(helper.XmlDocHtml(node));
            return new MvcHtmlString(htmlBuilder.ToString());
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocNode node){
            if (node is XmlDocTextNode)
                return new MvcHtmlString(node.Node.OuterXml);

            if (node is XmlDocNameElement)
                return helper.XmlDocHtml((XmlDocNameElement)node);

            if (node is XmlDocCodeElement)
                return helper.XmlDocHtml((XmlDocCodeElement)node);

            if (node is XmlDocRefElement)
                return helper.XmlDocHtml((XmlDocRefElement)node);

            if (node is XmlDocDefinitionList)
                return helper.XmlDocHtml((XmlDocDefinitionList)node);

            return node.HasChildren
                ? helper.XmlDocHtml(node.Children)
                : MvcHtmlString.Empty;
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocNameElement element) {
            if("PARAMREF".Equals(element.Name)){
                return element.HasChildren
                    ? helper.XmlDocHtml(element.Children)
                    : new MvcHtmlString("<code>" + (element.TargetName ?? "parameter") + "</code>");
            }

            return element.HasChildren
                ? helper.XmlDocHtml(element.Children)
                : new MvcHtmlString(element.TargetName);
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocCodeElement element){
            if (!element.HasChildren)
                return MvcHtmlString.Empty;

            var codeTag = element.IsInline
                ? new TagBuilder("code")
                : new TagBuilder("pre");

            // TODO: need to do something with the language

            codeTag.InnerHtml = helper.XmlDocHtml(element.Children).ToString();
            return new MvcHtmlString(codeTag.ToString());
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocRefElement element){
            if(!String.IsNullOrWhiteSpace(element.CRef)){
                if (element.HasChildren) {
                    return helper.CRefActionLink(
                        helper.XmlDocHtml(element.Children),
                        element.CRef);
                }
                // TODO: try to use the repository to get a short name
                return helper.CRefActionLink(
                    new MvcHtmlString(element.CRef),
                    element.CRef);
            }
            if (!String.IsNullOrWhiteSpace(element.HRef)) {
                var hrefTagBuilder = new TagBuilder("a");
                hrefTagBuilder.MergeAttribute("href", element.HRef);
                hrefTagBuilder.InnerHtml = element.HasChildren
                    ? helper.XmlDocHtml(element.Children).ToString()
                    : HttpUtility.HtmlEncode(element.HRef);
                return new MvcHtmlString(hrefTagBuilder.ToString());
            }
            if (!String.IsNullOrWhiteSpace(element.LangWord)) {
                var href = String.Format(
                    "http://www.bing.com/search?q={0}+keyword+%2Bmsdn",
                    element.LangWord);
                var hrefTagBuilder = new TagBuilder("a");
                hrefTagBuilder.MergeAttribute("href", href);
                hrefTagBuilder.InnerHtml = element.HasChildren
                    ? helper.XmlDocHtml(element.Children).ToString()
                    : HttpUtility.HtmlEncode(element.LangWord);
                return new MvcHtmlString(hrefTagBuilder.ToString());
            }

            return element.HasChildren
                ? helper.XmlDocHtml(element.Children)
                : MvcHtmlString.Empty;
        }

        public static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocDefinitionList element) {
            if (!element.HasItems)
                return MvcHtmlString.Empty;
            if ("NUMBER".Equals(element.ListType, StringComparison.OrdinalIgnoreCase))
                return helper.XmlDocHtmlAsList(element, "ol");
            if (String.IsNullOrWhiteSpace(element.ListType) || "BULLET".Equals(element.ListType, StringComparison.OrdinalIgnoreCase))
                return helper.XmlDocHtmlAsList(element, "ul");
            return helper.XmlDocHtmlAsTable(element);

        }

        private static MvcHtmlString XmlDocHtmlAsTable(this HtmlHelper helper, XmlDocDefinitionList element) {
            var tableTag = new TagBuilder("table");
            tableTag.MergeAttribute("class", "table table-bordered table-condensed");
            var tableGutsBuilder = new StringBuilder();
            foreach (var row in element.Items) {
                if(!row.HasTermContents && !row.HasDescriptionContents)
                    continue;

                var cellTag = row.IsHeader ? "th" : "td";
                var cellStartTag = String.Concat('<', cellTag, '>');
                var cellEndTag = String.Concat("</", cellTag, '>');
                var rowStart = row.IsHeader ? "<thead><tr>" : "<tr>";
                var rowEnd = row.IsHeader ? "</tr></thead>" : "</tr>";

                tableGutsBuilder.Append(rowStart + cellStartTag);
                if (row.HasTermContents)
                    tableGutsBuilder.Append(helper.XmlDocHtml(row.TermContents));
                tableGutsBuilder.Append(cellEndTag + cellStartTag);
                if (row.HasDescriptionContents)
                    tableGutsBuilder.Append(helper.XmlDocHtml(row.DescriptionContents));
                tableGutsBuilder.Append(cellEndTag + rowEnd);
            }
            tableTag.InnerHtml = tableGutsBuilder.ToString();
            return new MvcHtmlString(tableTag.ToString());
        }

        private static MvcHtmlString XmlDocHtmlAsList(this HtmlHelper helper, XmlDocDefinitionList element, string listType) {
            var listTag = new TagBuilder(listType);
            var listItemsHtmlBuilder = new StringBuilder();
            foreach (var item in element.Items) {
                listItemsHtmlBuilder.Append("<li><dl>");
                if (item.HasTermContents) {
                    listItemsHtmlBuilder.Append("<dt>");
                    listItemsHtmlBuilder.Append(helper.XmlDocHtml(item.TermContents));
                    listItemsHtmlBuilder.Append("</dt>");
                }
                if (item.HasDescriptionContents) {
                    listItemsHtmlBuilder.Append("<dd>");
                    listItemsHtmlBuilder.Append(helper.XmlDocHtml(item.DescriptionContents));
                    listItemsHtmlBuilder.Append("</dd>");
                }
                listItemsHtmlBuilder.Append("</dl></li>");
            }
            listTag.InnerHtml = listItemsHtmlBuilder.ToString();
            return new MvcHtmlString(listTag.ToString());
        }


    }
}