using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using System.Text;
using DandyDoc.XmlDoc;

namespace DandyDoc.Web.Mvc4.Helpers
{
    public static class DandyDocHelpers
    {

        private static readonly Regex CodeNameSplitRegex = new Regex(@"(?<=[.,;\<\(\[])");
        
        public static string[] CodeNameParts(string title) {
            return CodeNameSplitRegex.Split(title);
        }

        public static MvcHtmlString BreakIntoCodeNamePartElements(this HtmlHelper helper, string name, string tagName = "span") {
            var htmlBuilder = new StringBuilder();
            var openTag = '<' + tagName + '>';
            var closeTag = "</" + tagName + '>';
            foreach (var part in CodeNameParts(name)) {
                htmlBuilder.Append(openTag);
                htmlBuilder.Append(HttpUtility.HtmlEncode(part));
                htmlBuilder.Append(closeTag);
            }
            return new MvcHtmlString(htmlBuilder.ToString());
        }

        public static MvcHtmlString ActionLinkFull(this HtmlHelper helper, ICodeDocEntity entity) {
            var cRef = entity.CRef;
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
                // simplify the CRef for an assembly
                var text = entity.ShortName;
                var cRefText = "A:" + entity.ShortName;
                if (entity is ICodeDocAssembly) {
                    var assemblyFileName = ((ICodeDocAssembly)entity).AssemblyFileName;
                    if (!String.IsNullOrEmpty(assemblyFileName)) {
                        text += '(' + assemblyFileName + ')';
                        cRefText = "A:" + assemblyFileName;
                    }
                }
                return helper.CRefActionLink(text, cRefText);
            }
            return helper.CRefActionLink(entity.FullName, cRef.FullCRef);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper helper, ICodeDocEntity entity) {
            var cRef = entity.CRef;
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
                // simplify the CRef for an assembly
                return helper.CRefActionLink(entity.ShortName, "A:" + entity.ShortName);
            }
            return helper.CRefActionLink(entity.ShortName, cRef.FullCRef);
        }

        public static MvcHtmlString CRefActionLink(this HtmlHelper helper, string linkText, string cRef) {
            return helper.CRefActionLink(new MvcHtmlString(HttpUtility.HtmlEncode(linkText)), cRef);
        }

        public static MvcHtmlString CRefActionLink(this HtmlHelper helper, MvcHtmlString innerHtml, string cRef) {
            if (String.IsNullOrWhiteSpace(cRef) || cRef[0] == '`')
                return new MvcHtmlString("<span>" + innerHtml + "</span>");
            var linkBuilder = new TagBuilder("a");
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            linkBuilder.MergeAttribute("href", urlHelper.Action("Api", "Docs", new { cRef }));
            if(innerHtml != null)
                linkBuilder.InnerHtml = innerHtml.ToString();
            return new MvcHtmlString(linkBuilder.ToString());
        }

        public static MvcHtmlString ActionLinkList(this HtmlHelper helper, IEnumerable<ICodeDocEntity> entities, object ulTagAttributes = null, object liTagAttributes = null) {
            var tagBuilder = new TagBuilder("ul");

            if (ulTagAttributes == null)
                tagBuilder.MergeAttribute("class", "unstyled");
            else
                tagBuilder.MergeAttributes(new RouteValueDictionary(ulTagAttributes));

            var liTagAttributesDictionary = liTagAttributes != null
                ? new RouteValueDictionary(liTagAttributes)
                : null;

            var innerHtmlBuilder = new StringBuilder();
            foreach (var entity in entities) {
                var liTagBuilder = new TagBuilder("li");
                if(liTagAttributesDictionary == null)
                    liTagBuilder.MergeAttribute("class","text-indent: -7px;");
                else
                    liTagBuilder.MergeAttributes(liTagAttributesDictionary);
                liTagBuilder.InnerHtml = helper.ActionLink(entity).ToString();
                innerHtmlBuilder.Append(liTagBuilder);
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();

            return new MvcHtmlString(tagBuilder.ToString());
        }

        public static MvcHtmlString CodeDocEntityTable(this HtmlHelper helper, IEnumerable<ICodeDocEntity> entities, object tableTagAttributes = null) {
            var builder = new TagBuilder("table");

            if(tableTagAttributes == null)
                builder.MergeAttribute("class", "table table-bordered");
            else
                builder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var tableInnerHtmlBuilder = new StringBuilder("<thead><tr><th><i class=\"icon-info-sign\"></i></th><th>Name</th><th>Description</th></tr></thead><tbody>");
            foreach(var entity in entities){
                tableInnerHtmlBuilder.Append("<tr><td>");
                // TODO: flair
                tableInnerHtmlBuilder.Append("</td><td>");
                tableInnerHtmlBuilder.Append(helper.ActionLink(entity));
                tableInnerHtmlBuilder.Append("</td><td>");
                if (entity.HasSummaryContents)
                    tableInnerHtmlBuilder.Append(helper.XmlDocHtml(entity.SummaryContents));

                tableInnerHtmlBuilder.Append("</td></tr>");
            }
            tableInnerHtmlBuilder.Append("</tbody>");
            builder.InnerHtml = tableInnerHtmlBuilder.ToString();

            return new MvcHtmlString(builder.ToString());
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

            // TODO: need to do something with the language? Maybe not if the google thing is used?

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

                var repository = helper.ViewBag.CodeDocEntityRepository as ICodeDocEntityRepository;
                if (repository != null) {
                    var targetModel = repository.GetSimpleEntity(new CRefIdentifier(element.CRef));
                    if (targetModel != null) {
                        return helper.ActionLink(targetModel);
                    }
                }

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

        public static MvcHtmlString CodeDocParameterDetails(this HtmlHelper helper, ICodeDocParameter parameter) {
            var htmlBuilder = new StringBuilder();
            if (parameter.HasSummaryContents) {
                htmlBuilder.Append("<div>");
                htmlBuilder.Append(helper.XmlDocHtml(parameter.SummaryContents));
                htmlBuilder.Append("</div>");
            }
            htmlBuilder.Append("<div>Type: ");
            htmlBuilder.Append(helper.ActionLink(parameter.ParameterType));
            htmlBuilder.Append("</div>");
            // TODO: Flair
            // TODO: Ref/Out params (taken care of by flair?)
            return new MvcHtmlString(htmlBuilder.ToString());
        }

        public static MvcHtmlString CodeDocParameters(this HtmlHelper helper, IEnumerable<ICodeDocParameter> parameters) {
            var tagBuilder = new TagBuilder("dl");
            var innerHtmlBuilder = new StringBuilder();
            foreach (var parameter in parameters) {
                innerHtmlBuilder.Append("<dt>");
                innerHtmlBuilder.Append(HttpUtility.HtmlEncode(parameter.Name));
                innerHtmlBuilder.Append("</dt><dd>");
                innerHtmlBuilder.Append(helper.CodeDocParameterDetails(parameter));
                innerHtmlBuilder.Append("</dd>");

            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new MvcHtmlString(tagBuilder.ToString());
        }

        public static MvcHtmlString CodeDocParameters(this HtmlHelper helper, IEnumerable<ICodeDocGenericParameter> parameters) {
            var tagBuilder = new TagBuilder("dl");
            var innerHtmlBuilder = new StringBuilder();
            foreach (var parameter in parameters) {
                innerHtmlBuilder.Append("<dt>");
                innerHtmlBuilder.Append(parameter.Name);
                innerHtmlBuilder.Append("</dt><dd>");

                if (parameter.HasSummaryContents) {
                    innerHtmlBuilder.Append("<p>");
                    innerHtmlBuilder.Append(helper.XmlDocHtml(parameter.SummaryContents));
                    innerHtmlBuilder.Append("</p>");
                }

                if (parameter.IsContravariant)
                    innerHtmlBuilder.Append("<div><i class=\"icon-random\"></i>Contravariant: Type is used for input and can be used with a more specific type.</div>");
                if (parameter.IsCovariant)
                    innerHtmlBuilder.Append("<div><i class=\"icon-random\"></i>Covariant: Type is used for output and can be used as a more general type.</div>");

                if (parameter.HasAnyConstraints) {
                    innerHtmlBuilder.Append("<div>Constraints:<ul>");

                    if (parameter.HasDefaultConstructorConstraint)
                        innerHtmlBuilder.Append("<li>Default Constructor</li>");

                    if (parameter.HasNotNullableValueTypeConstraint)
                        ;//innerHtmlBuilder.Append("<li>Value Type</li>");
                    else if (parameter.HasReferenceTypeConstraint)
                        innerHtmlBuilder.Append("<li>Reference Type</li>");

                    if (parameter.HasTypeConstraints) {
                        foreach (var typeConstraint in parameter.TypeConstraints) {
                            innerHtmlBuilder.Append("<li>");
                            innerHtmlBuilder.Append(helper.ActionLink(typeConstraint));
                            innerHtmlBuilder.Append("</li>");
                        }
                    }

                    innerHtmlBuilder.Append("</ul></div>");
                }

                innerHtmlBuilder.Append("</dd>");
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new MvcHtmlString(tagBuilder.ToString());
        }

        public static MvcHtmlString CodeDocExceptions(this HtmlHelper helper, IEnumerable<ICodeDocException> exceptions, object tableTagAttributes = null) {
            var tagBuilder = new TagBuilder("table");

            if (tableTagAttributes == null)
                tagBuilder.MergeAttribute("class", "table table-bordered table-condensed");
            else
                tagBuilder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var innerHtmlBuilder = new StringBuilder("<thead><tr><th>Exception</th><th>Condition</th></tr></thead><tbody>");
            foreach(var exception in exceptions){
                innerHtmlBuilder.Append("<tr><td>");
                innerHtmlBuilder.Append(helper.ActionLink(exception.ExceptionType));
                innerHtmlBuilder.Append("</td><td>");

                var conditions = exception.HasConditions ? exception.Conditions.Where(x => x.HasChildren).ToList() : new List<XmlDocNode>(0);
                var ensures = exception.HasEnsures ? exception.Ensures.Where(x => x.HasChildren).ToList() : new List<XmlDocNode>(0);

                if (conditions.Count == 1 && ensures.Count == 0) {
                    innerHtmlBuilder.Append(helper.XmlDocHtml(conditions[0].Children));
                }
                else{
                    innerHtmlBuilder.Append("<dl>");
                    if (conditions.Count > 0) {
                        innerHtmlBuilder.Append("<dt>Conditions</dt><dd>");
                        innerHtmlBuilder.Append(helper.XmlDocHtmlChildListOrSingle(conditions));
                        innerHtmlBuilder.Append("</dd>");
                    }
                    if (ensures.Count > 0) {
                        innerHtmlBuilder.Append("<dt>Ensures</dt><dd>");
                        innerHtmlBuilder.Append(helper.XmlDocHtmlChildListOrSingle(ensures));
                        innerHtmlBuilder.Append("</dd>");
                    }
                    innerHtmlBuilder.Append("</dl>");
                }
                innerHtmlBuilder.Append("</td></tr>");
            }
            innerHtmlBuilder.Append("</tbody>");
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();

            return new MvcHtmlString(tagBuilder.ToString());
        }

        private static MvcHtmlString XmlDocHtmlChildListOrSingle(this HtmlHelper helper, IList<XmlDocNode> nodes){
            Contract.Requires(nodes != null);
            if (nodes.Count == 0)
                return MvcHtmlString.Empty;
            if (nodes.Count == 1)
                return helper.XmlDocHtml(nodes[0].Children);
            var tagBuilder = new TagBuilder("ul");
            var innerHtmlBuilder = new StringBuilder();
            foreach(var node in nodes){
                innerHtmlBuilder.Append("<li>");
                innerHtmlBuilder.Append(helper.XmlDocHtml(node.Children));
                innerHtmlBuilder.Append("</li>");
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new MvcHtmlString(tagBuilder.ToString());
        }

        public static MvcHtmlString CodeDocSimpleCodeContractTable(this HtmlHelper helper, IEnumerable<XmlDocContractElement> contracts, object tableTagAttributes = null) {
            var tagBuilder = new TagBuilder("table");

            if (tableTagAttributes == null)
                tagBuilder.MergeAttribute("class", "table table-bordered table-condensed");
            else
                tagBuilder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var innerHtmlBuilder = new StringBuilder("<thead><tr><th>Description</th><th>Code</th></tr></thead><tbody>");
            foreach (var contract in contracts) {
                innerHtmlBuilder.Append("<tr><td>");
                innerHtmlBuilder.Append(helper.XmlDocHtml(contract.Children));
                innerHtmlBuilder.Append("</td><td>");
                var code = contract.CSharp ?? contract.VisualBasic;
                if(!String.IsNullOrWhiteSpace(code)){
                    innerHtmlBuilder.Append("<code>");
                    innerHtmlBuilder.Append(HttpUtility.HtmlEncode(code));
                    innerHtmlBuilder.Append("</code>");
                }
                innerHtmlBuilder.Append("</td></tr>");
            }
            innerHtmlBuilder.Append("</tbody>");
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new MvcHtmlString(tagBuilder.ToString());
        }

        public static MvcHtmlString CodeDocAccessor(this HtmlHelper helper, ICodeDocMethod accessor) {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append(helper.FlairTable(accessor));

            if (accessor.HasExceptions) {
                htmlBuilder.Append("<section><h3>Exceptions</h3>");
                htmlBuilder.Append(helper.CodeDocExceptions(accessor.Exceptions));
                htmlBuilder.Append("</section>");
            }
            if (accessor.HasRequires) {
                htmlBuilder.Append("<section><h3>Requires</h3>");
                htmlBuilder.Append(helper.CodeDocSimpleCodeContractTable(accessor.Requires));
                htmlBuilder.Append("</section>");
            }
            if (accessor.HasNormalTerminationEnsures) {
                htmlBuilder.Append("<section><h3>Ensures on Successful Execution</h3>");
                htmlBuilder.Append(helper.CodeDocSimpleCodeContractTable(accessor.NormalTerminationEnsures));
                htmlBuilder.Append("</section>");
            }
            return new MvcHtmlString(htmlBuilder.ToString());
        }

        public static bool CodeDocAccessorIsWorthDisplaying(ICodeDocMethod accessor) {
            return GetFlair(accessor).Any()
                || accessor.HasExceptions
                || accessor.HasNormalTerminationEnsures
                || accessor.HasRequires;
        }

        public static MvcHtmlString FlairIconList(this HtmlHelper helper, ICodeDocEntity entity){
            var flair = GetFlair(entity).ToList();
            if (flair.Count == 0)
                return MvcHtmlString.Empty;
            throw new NotImplementedException();
        }

        public static MvcHtmlString FlairTable(this HtmlHelper helper, ICodeDocEntity entity){
            var flair = GetFlair(entity).ToList();
            if (flair.Count == 0)
                return MvcHtmlString.Empty;
            throw new NotImplementedException();
        }

        private class FlairItem
        {
            MvcHtmlString Icon;
            MvcHtmlString Description;
        }

        private static IEnumerable<FlairItem> GetFlair(ICodeDocEntity entity){
            throw new NotImplementedException();
        }

        private static IEnumerable<FlairItem> GetFlair(ICodeDocParameter parameter){
            /*
            if (Parameter.HasAttributeMatchingName("CanBeNullAttribute")){
					tags.Add(DefaultCanBeNullTag);
				}
				else{
					var name = Parameter.Name;
					Contract.Assume(!String.IsNullOrEmpty(name));
					if (Parent.RequiresParameterNotNullOrEmpty(name))
						tags.Add(DefaultParamNotNullAndNotEmptyTag);
					else if (Parent.RequiresParameterNotNull(name))
						tags.Add(DefaultParamNotNullTag);
				}

				if (Parameter.HasAttributeMatchingName("InstantHandleAttribute"))
					tags.Add(DefaultInstantHandleTag);
            */
            throw new NotImplementedException();
        }

    }
}