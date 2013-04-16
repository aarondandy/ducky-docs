using System;
using System.Collections.Generic;
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

        public static MvcHtmlString CRefActionLink(this HtmlHelper helper, string text, string cRef){
            return helper.ActionLink(text, "Api", new { cRef });
        }

        public static MvcHtmlString CRefActionLink(this HtmlHelper helper, MvcHtmlString innerHtml, string cRef){
            var linkBuilder = new TagBuilder("a");
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            linkBuilder.MergeAttribute("href", urlHelper.Action("Api", new { cRef }).ToString());
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

                tableInnerHtmlBuilder.Append(helper.CRefActionLink(entity.ShortName, entity.CRef.FullCRef));

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

            throw new NotImplementedException();
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

        private static MvcHtmlString XmlDocHtml(this HtmlHelper helper, XmlDocRefElement element){
            if(!String.IsNullOrWhiteSpace(element.CRef)){
                if (element.HasChildren) {
                    return helper.CRefActionLink(
                        helper.XmlDocHtml(element.Children),
                        element.CRef);
                }
                else{
                    // TODO: try to use the repository to get a short name
                    return helper.CRefActionLink(
                        new MvcHtmlString(element.CRef),
                        element.CRef);
                }
            }
            throw new NotImplementedException();
        }

    }
}