using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using DandyDoc.CodeDoc;
using System.Text;
using System.Web.Mvc.Html;

namespace DandyDoc.Web.Mvc4.Helpers
{
    public static class DandyDocHelpers
    {

        public static MvcHtmlString CodeDocEntityTable(this HtmlHelper helper, IEnumerable<ICodeDocEntity> entities, object tableTagAttributes = null) {
            var builder = new TagBuilder("table");

            if(tableTagAttributes != null)
                builder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var rowHtmlBuilder = new StringBuilder();
            foreach(var entity in entities){
                rowHtmlBuilder.Append("<tr><td>");
                // TODO: flair
                rowHtmlBuilder.Append("</td><td>");
                rowHtmlBuilder.Append(helper.ActionLink(entity.ShortName, "Api", new { cRef=entity.CRef.FullCRef}));
                rowHtmlBuilder.Append("</td><td>");
                // TODO: summry
                rowHtmlBuilder.Append("</td></tr>");
            }

            builder.InnerHtml = String.Concat(
                "<thead><tr><th><i class=\"icon-info-sign\"></i></th><th>Name</th><th>Description</th></tr></thead>",
                "<tbody>",
                rowHtmlBuilder.ToString(),
                "</tbody>"
                );

            return new MvcHtmlString(builder.ToString());
        }

    }
}