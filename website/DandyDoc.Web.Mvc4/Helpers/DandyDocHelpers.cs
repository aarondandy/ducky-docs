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
using DandyDoc.ExternalVisibility;
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

        public static MvcHtmlString ActionLinkFull(this HtmlHelper helper, ICodeDocMember member) {
            var cRef = member.CRef;
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
                // simplify the CRef for an assembly
                var text = member.ShortName;
                var cRefText = "A:" + member.ShortName;
                if (member is CodeDocSimpleAssembly) {
                    var assemblyFileName = ((CodeDocSimpleAssembly)member).AssemblyFileName;
                    if (!String.IsNullOrEmpty(assemblyFileName)) {
                        text += " (" + assemblyFileName + ')';
                        cRefText = "A:" + assemblyFileName;
                    }
                }
                return helper.CRefActionLink(text, cRefText);
            }
            return helper.CRefActionLink(member.FullName, cRef.FullCRef);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper helper, ICodeDocMember member) {
            var cRef = member.CRef;
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase)) {
                // simplify the CRef for an assembly
                return helper.CRefActionLink(member.ShortName, "A:" + member.ShortName);
            }
            return helper.CRefActionLink(member.ShortName, cRef.FullCRef);
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

        public static MvcHtmlString ActionLinkList(this HtmlHelper helper, IEnumerable<ICodeDocMember> entities, object ulTagAttributes = null, object liTagAttributes = null) {
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

        public static MvcHtmlString CodeDocEntityTable(this HtmlHelper helper, IEnumerable<ICodeDocMember> entities, object tableTagAttributes = null) {
            var builder = new TagBuilder("table");

            if(tableTagAttributes == null)
                builder.MergeAttribute("class", "table table-bordered");
            else
                builder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var tableInnerHtmlBuilder = new StringBuilder("<thead><tr><th><i class=\"icon-info-sign\"></i></th><th>Name</th><th>Description</th></tr></thead><tbody>");
            foreach(var entity in entities){
                tableInnerHtmlBuilder.Append("<tr><td>");
                tableInnerHtmlBuilder.Append(helper.FlairIconList(entity));
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

            if (node is XmlDocElement) {
                var element = (XmlDocElement)node;
                var nodeName = element.Name;
                if (String.Equals("PARA", nodeName))
                    return new MvcHtmlString("<p>" + helper.XmlDocHtml(element.Children) + "</p>");
                return new MvcHtmlString(element.Node.OuterXml); // just use it in the raw
            }

            return helper.XmlDocHtml(node.Children);
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

                var repository = helper.ViewBag.CodeDocEntityRepository as ICodeDocMemberRepository;
                if (repository != null) {
                    var targetModel = repository.GetSimpleMember(new CRefIdentifier(element.CRef));
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

        public static MvcHtmlString CodeDocParameterDetails(this HtmlHelper helper, CodeDocParameter parameter) {
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

        public static MvcHtmlString CodeDocParameters(this HtmlHelper helper, IEnumerable<CodeDocParameter> parameters) {
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

        public static MvcHtmlString CodeDocParameters(this HtmlHelper helper, IEnumerable<CodeDocGenericParameter> parameters) {
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

        public static MvcHtmlString CodeDocExceptions(this HtmlHelper helper, IEnumerable<CodeDocException> exceptions, object tableTagAttributes = null) {
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

        public static MvcHtmlString CodeDocAccessor(this HtmlHelper helper, CodeDocMethod accessor) {
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

        public static bool CodeDocAccessorIsWorthDisplaying(CodeDocMethod accessor) {
            return GetFlair(accessor).Any()
                || accessor.HasExceptions
                || accessor.HasNormalTerminationEnsures
                || accessor.HasRequires;
        }

        public static MvcHtmlString FlairIconList(this HtmlHelper helper, ICodeDocMember member){
            var flair = GetFlair(member).ToList();
            if (flair.Count == 0)
                return MvcHtmlString.Empty;
            return helper.FlairIconList(flair);
        }

        public static MvcHtmlString FlairIconList(this HtmlHelper helper, IEnumerable<FlairItem> flairItems) {
            var builder = new StringBuilder();
            foreach (var flairItem in flairItems) {
                builder.Append(helper.FlairIcon(flairItem));
            }
            return new MvcHtmlString(builder.ToString());
        }

        private static readonly Dictionary<string, string> SimpleIconClasses = new Dictionary<string, string> {
            {"protected", "icon-eye-close"},
            {"public","icon-eye-open"},
            {"hidden","icon-ban-circle"},
            {"pure","icon-leaf"},
            {"static","icon-globe"},
            {"extension","icon-resize-full"},
            {"no-nulls","icon-ok-circle"},
            {"null-result","icon-question-sign"},
            {"null-ok","icon-ok-sign"},
            {"flags","icon-flag"},
            {"operator","icon-plus"},
            {"sealed","icon-lock"},
            {"virtual","icon-circle-arrow-down"},
            {"abstract","icon-edit"},
            {"obsolete","icon-warning-sign"},
            {"instant","icon-tasks"},
            {"value-type","icon-th-large"}
        };

        public static MvcHtmlString FlairIcon(this HtmlHelper helper, FlairItem item) {
            string cssClass;
            if(SimpleIconClasses.TryGetValue(item.IconId, out cssClass))
                return new MvcHtmlString("<i class=\"" +cssClass+ " flair-icon\" title=\"" + item.IconId + "\"></i>");

            switch (item.IconId) {
            case "get": return new MvcHtmlString("<code class=\"flair-icon\" title=\"getter\">get</code>");
            case "proget": return new MvcHtmlString("<code class=\"flair-icon\" title=\"protected getter\"><i class=\"icon-eye-close\"></i>get</code>");
            case "set": return new MvcHtmlString("<code class=\"flair-icon\" title=\"setter\">set</code>");
            case "proset": return new MvcHtmlString("<code class=\"flair-icon\" title=\"protected setter\"><i class=\"icon-eye-close\"></i>set</code>");
            case "indexer": return new MvcHtmlString("<code class=\"flair-icon\" title=\"indexer\">[]</code>");
            case "constant": return new MvcHtmlString("<code class=\"flair-icon\" title=\"constant\">42</code>");
            case "readonly": return new MvcHtmlString("<code class=\"flair-icon\" title=\"readonly\">get</code>");
            default: return new MvcHtmlString(item.IconId);
            }
        }

        public static MvcHtmlString FlairTable(this HtmlHelper helper, ICodeDocMember member){
            var flair = GetFlair(member).ToList();
            if (flair.Count == 0)
                return MvcHtmlString.Empty;
            return helper.FlairTable(flair);
        }

        public static MvcHtmlString FlairTable(this HtmlHelper helper, IEnumerable<FlairItem> items){
            var tagbuilder = new TagBuilder("table");
            tagbuilder.MergeAttribute("class", "table table-bordered table-condensed");
            var rowBuilder = new StringBuilder();
            foreach (var item in items.OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)) {
                rowBuilder.Append("<tr><th>");
                rowBuilder.Append(helper.FlairIcon(item));
                rowBuilder.Append("</th><td>");
                rowBuilder.Append(HttpUtility.HtmlEncode(item.Category));
                rowBuilder.Append("</td><td>");
                rowBuilder.Append(item.Description);
                rowBuilder.Append("</td></tr>");
            }
            tagbuilder.InnerHtml = rowBuilder.ToString();
            return new MvcHtmlString(tagbuilder.ToString());
        }

        private static bool HasReferenceParamsOrReturnAndAllAreNullrestricted(ICodeDocInvokable invokable) {
            Contract.Requires(invokable != null);
            IEnumerable<CodeDocParameter> parameters = invokable.Parameters;
            if (invokable.HasReturn)
                parameters = parameters.Concat(new[] { invokable.Return });

            int count = 0;
            foreach (var parameter in parameters) {
                if (parameter.IsReferenceType.GetValueOrDefault()) {
                    if (!(parameter.NullRestricted.GetValueOrDefault()))
                        return false;
                    count++;
                }
            }
            return count > 0;
        }

        public class FlairItem
        {
            public string IconId;
            public string Category;
            public MvcHtmlString Description;

            public FlairItem(string iconId, string category, MvcHtmlString description) {
                IconId = iconId;
                Category = category;
                Description = description;
            }

            public FlairItem(string iconId, string category, string description)
                : this(iconId, category, new MvcHtmlString(description)) { }

        }

        private static readonly FlairItem DefaultStaticTag = new FlairItem("static", "Static", "Accessible relative to a type rather than an object instance.");
        private static readonly FlairItem DefaultObsoleteTag = new FlairItem("obsolete", "Warning", "<strong>This is deprecated.</strong>");
        private static readonly FlairItem DefaultPublicTag = new FlairItem("public", "Visibility", "Externally visible.");
        private static readonly FlairItem DefaultProtectedTag = new FlairItem("protected", "Visibility", "Externally visible only through inheritance.");
        private static readonly FlairItem DefaultHiddenTag = new FlairItem("hidden", "Visibility", "Not externally visible.");
        private static readonly FlairItem DefaultCanReturnNullTag = new FlairItem("null-result", "Null Values", "May return null.");
        private static readonly FlairItem DefaultNotNullTag = new FlairItem("no-nulls", "Null Values", "Does not return or accept null values for reference types.");
        private static readonly FlairItem DefaultPureTag = new FlairItem("pure", "Purity", "Does not have side effects.");
        private static readonly FlairItem DefaultOperatorTag = new FlairItem("operator", "Operator", "Invoked through a language operator.");
        private static readonly FlairItem DefaultFlagsTag = new FlairItem("flags", "Enumeration", "Bitwise combination is allowed.");
        private static readonly FlairItem DefaultSealedTag = new FlairItem("sealed", "Inheritance", "This is sealed, preventing inheritance.");
        private static readonly FlairItem DefaultValueTypeTag = new FlairItem("value-type", "Type", "This type is a value type.");
        private static readonly FlairItem DefaultExtensionMethodTag = new FlairItem("extension", "Extension", "This method is an extension method.");
        private static readonly FlairItem DefaultAbstractTag = new FlairItem("abstract", "Inheritance", "This is abstract and <strong>must</strong> be implemented by inheriting types.");
        private static readonly FlairItem DefaultVirtualTag = new FlairItem("virtual", "Inheritance", "This is virtual and can be overridden by inheriting types.");
        private static readonly FlairItem DefaultOverrideTag = new FlairItem("override", "Inheritance", "This overrides functionality from its parent.");
        private static readonly FlairItem DefaultConstantTag = new FlairItem("constant", "Value", "This field is a constant.");
        private static readonly FlairItem DefaultReadonlyTag = new FlairItem("readonly", "Value", "This field is only assignable during instantiation.");
        private static readonly FlairItem DefaultIndexerOperatorTag = new FlairItem("indexer", "Operator", "This property is invoked through a language index operator.");
        private static readonly FlairItem DefaultGetTag = new FlairItem("get", "Property", "Value can be read externally.");
        private static readonly FlairItem DefaultProGetTag = new FlairItem("proget", "Property", "Value can be read through inheritance.");
        private static readonly FlairItem DefaultSetTag = new FlairItem("set", "Property", "Value can be assigned externally.");
        private static readonly FlairItem DefaultProSetTag = new FlairItem("proset", "Property", "Value can be assigned through inheritance.");

        private static IEnumerable<FlairItem> GetFlair(ICodeDocMember member){
            Contract.Requires(member != null);

            var invokable = member as ICodeDocInvokable;
            var property = member as CodeDocProperty;
            var field = member as CodeDocField;
            var type = member as CodeDocType;

            if (property == null && !(member is CodeDocSimpleNamespace || member is CodeDocSimpleAssembly)) {
                switch (member.ExternalVisibility) {
                    case ExternalVisibilityKind.Hidden:
                        yield return DefaultHiddenTag;
                        break;
                    case ExternalVisibilityKind.Protected:
                        yield return DefaultProtectedTag;
                        break;
                    case ExternalVisibilityKind.Public:
                        //yield return DefaultPublicTag;
                        break;
                }
            }

            if (member.IsStatic) {
                if (invokable == null || !invokable.IsOperatorOverload)
                    yield return DefaultStaticTag;
            }

            if (member.IsObsolete)
                yield return DefaultObsoleteTag;

            if (type != null) {
                if (invokable == null) {
                    if (type.IsFlagsEnum)
                        yield return DefaultFlagsTag;

                    if (type.IsValueType)
                        yield return DefaultValueTypeTag;
                    else if (type.IsSealed)
                        yield return DefaultSealedTag;
                }
            }

            if (invokable != null) {
                if (HasReferenceParamsOrReturnAndAllAreNullrestricted(invokable))
                    yield return DefaultNotNullTag;
                if (invokable.IsPure)
                    yield return DefaultPureTag;
                if (invokable.IsExtensionMethod)
                    yield return DefaultExtensionMethodTag;
                if (invokable.IsOperatorOverload)
                    yield return DefaultOperatorTag;

                if (type == null) {
                    if (invokable.IsSealed)
                        yield return DefaultSealedTag;
                    else if (invokable.IsAbstract)
                        yield return DefaultAbstractTag;
                    else if (invokable.IsVirtual)
                        yield return DefaultVirtualTag;
                }
            }

            if (field != null) {
                if (field.IsLiteral)
                    yield return DefaultConstantTag;
                if (field.IsInitOnly)
                    yield return DefaultReadonlyTag;
            }

            if (property != null) {
                if (property.HasParameters && "Item".Equals(property.ShortName))
                    yield return DefaultIndexerOperatorTag;

                if (property.HasGetter) {
                    switch (property.Getter.ExternalVisibility) {
                        case ExternalVisibilityKind.Public:
                            yield return DefaultGetTag;
                            break;
                        case ExternalVisibilityKind.Protected:
                            yield return DefaultProGetTag;
                            break;
                    }
                }
                if (property.HasSetter) {
                    switch (property.Setter.ExternalVisibility) {
                        case ExternalVisibilityKind.Public:
                            yield return DefaultSetTag;
                            break;
                        case ExternalVisibilityKind.Protected:
                            yield return DefaultProSetTag;
                            break;
                    }
                }

            }

        }

        private static IEnumerable<FlairItem> GetFlair(CodeDocParameter parameter){
            Contract.Requires(parameter != null);
            if(parameter.NullRestricted.GetValueOrDefault())
                throw new NotImplementedException();

            if (parameter.IsOut)
                throw new NotImplementedException();
            else if(parameter.IsByRef)
                throw new NotImplementedException();

            // TODO: instant handle?
            yield break;
        }

        private static IEnumerable<FlairItem> GetFlair(CodeDocGenericParameter parameter) {
            Contract.Requires(parameter != null);
            if(parameter.IsCovariant)
                throw new NotImplementedException();
            else if(parameter.IsContravariant)
                throw new NotImplementedException();
            yield break;
        } 

    }
}