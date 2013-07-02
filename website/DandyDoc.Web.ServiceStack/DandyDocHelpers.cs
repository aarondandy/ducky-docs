using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;
using ServiceStack.Html;
using ServiceStack.Razor;

namespace DandyDoc.Web.ServiceStack
{
    public static class DandyDocHelpers
    {

        private static readonly HtmlString EmptyHtmlString = new HtmlString(String.Empty);

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

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, IEnumerable<XmlDocNode> nodes) where T:class {
            var htmlBuilder = new StringBuilder();
            foreach (var node in nodes)
                htmlBuilder.Append(viewPage.XmlDocHtml(node));
            return new HtmlString(htmlBuilder.ToString());
        }

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, XmlDocNode node) where T:class {
            if (node is XmlDocTextNode)
                return new HtmlString(node.Node.OuterXml);
            if (node is XmlDocNameElement)
                return viewPage.XmlDocHtml((XmlDocNameElement)node);
            if (node is XmlDocCodeElement)
                return viewPage.XmlDocHtml((XmlDocCodeElement)node);
            if (node is XmlDocRefElement)
                return viewPage.XmlDocHtml((XmlDocRefElement)node);
            if (node is XmlDocDefinitionList)
                return viewPage.XmlDocHtml((XmlDocDefinitionList)node);
            if (node is XmlDocElement) {
                var element = (XmlDocElement)node;
                var nodeName = element.Name;
                if (String.Equals("PARA", nodeName, StringComparison.OrdinalIgnoreCase))
                    return new HtmlString("<p>" + viewPage.XmlDocHtml(element.Children) + "</p>");
                return new HtmlString(element.Node.OuterXml); // just use it in the raw
            }
            return viewPage.XmlDocHtml(node.Children);
        }

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, XmlDocNameElement element) where T:class {
            var targetName = element.TargetName;
            if (String.IsNullOrWhiteSpace(targetName))
                targetName = "parameter";
            if ("PARAMREF".Equals(element.Name)) {
                return element.HasChildren
                    ? viewPage.XmlDocHtml(element.Children)
                    : new HtmlString("<code>" + targetName + "</code>");
            }

            return element.HasChildren
                ? viewPage.XmlDocHtml(element.Children)
                : new HtmlString(targetName);
        }

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, XmlDocCodeElement element) where T:class {
            if (!element.HasChildren)
                return EmptyHtmlString;
            var tagName = element.IsInline ? "code" : "pre";
            var codeTag = new TagBuilder(tagName);

            // TODO: need to do something with the language? Maybe not if the google code format thing is used?

            codeTag.InnerHtml = viewPage.XmlDocHtml(element.Children).ToString();
            return new HtmlString(codeTag.ToString());
        }

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, XmlDocRefElement element) where T:class {
            var linkText = element.HasChildren
                ? viewPage.XmlDocHtml(element.Children)
                : null;
            if (!String.IsNullOrWhiteSpace(element.CRef))
                return viewPage.ActionLink(new CRefIdentifier(element.CRef), linkText);

            var href = "#";
            if (!String.IsNullOrWhiteSpace(element.HRef)) {
                href = element.HRef;
                if (linkText == null)
                    linkText = new HtmlString(HttpUtility.HtmlEncode(element.HRef));
            }
            else if (!String.IsNullOrWhiteSpace(element.LangWord)) {
                href = String.Format(
                    "http://www.bing.com/search?q={0}+keyword+%2Bmsdn",
                    element.LangWord);
                if (linkText == null)
                    linkText = new HtmlString(HttpUtility.HtmlEncode(element.LangWord));
            }
            else if (linkText == null) {
                return EmptyHtmlString;
            }

            var hrefTagBuilder = new TagBuilder("a");
            hrefTagBuilder.MergeAttribute("href", href);
            hrefTagBuilder.InnerHtml = linkText.ToString();
            return new HtmlString(hrefTagBuilder.ToString());
        }

        public static HtmlString XmlDocHtml<T>(this ViewPage<T> viewPage, XmlDocDefinitionList element) where T:class {
            if (!element.HasItems)
                return EmptyHtmlString;
            if ("NUMBER".Equals(element.ListType, StringComparison.OrdinalIgnoreCase))
                return viewPage.XmlDocHtmlAsList(element, "ol");
            if (String.IsNullOrWhiteSpace(element.ListType) || "BULLET".Equals(element.ListType, StringComparison.OrdinalIgnoreCase))
                return viewPage.XmlDocHtmlAsList(element, "ul");
            return viewPage.XmlDocHtmlAsTable(element);
        }

        private static HtmlString XmlDocHtmlAsTable<T>(this ViewPage<T> viewPage, XmlDocDefinitionList element) where T:class {
            var tableTag = new TagBuilder("table");
            tableTag.MergeAttribute("class", "table table-bordered table-condensed");
            var tableGutsBuilder = new StringBuilder();
            foreach (var row in element.Items) {
                if (!row.HasTermContents && !row.HasDescriptionContents)
                    continue;

                var cellTag = row.IsHeader ? "th" : "td";
                var cellStartTag = String.Concat('<', cellTag, '>');
                var cellEndTag = String.Concat("</", cellTag, '>');
                var rowStart = row.IsHeader ? "<thead><tr>" : "<tr>";
                var rowEnd = row.IsHeader ? "</tr></thead>" : "</tr>";

                tableGutsBuilder.Append(rowStart + cellStartTag);
                if (row.HasTermContents)
                    tableGutsBuilder.Append(viewPage.XmlDocHtml(row.TermContents));
                tableGutsBuilder.Append(cellEndTag + cellStartTag);
                if (row.HasDescriptionContents)
                    tableGutsBuilder.Append(viewPage.XmlDocHtml(row.DescriptionContents));
                tableGutsBuilder.Append(cellEndTag + rowEnd);
            }
            tableTag.InnerHtml = tableGutsBuilder.ToString();
            return new HtmlString(tableTag.ToString());
        }

        private static HtmlString XmlDocHtmlAsList<T>(this ViewPage<T> viewPage, XmlDocDefinitionList element, string listType) where T:class {
            var listTag = new TagBuilder(listType);
            var listItemsHtmlBuilder = new StringBuilder();
            foreach (var item in element.Items) {
                listItemsHtmlBuilder.Append("<li><dl>");
                if (item.HasTermContents) {
                    listItemsHtmlBuilder.Append("<dt>");
                    listItemsHtmlBuilder.Append(viewPage.XmlDocHtml(item.TermContents));
                    listItemsHtmlBuilder.Append("</dt>");
                }
                if (item.HasDescriptionContents) {
                    listItemsHtmlBuilder.Append("<dd>");
                    listItemsHtmlBuilder.Append(viewPage.XmlDocHtml(item.DescriptionContents));
                    listItemsHtmlBuilder.Append("</dd>");
                }
                listItemsHtmlBuilder.Append("</dl></li>");
            }
            listTag.InnerHtml = listItemsHtmlBuilder.ToString();
            return new HtmlString(listTag.ToString());
        }

        public static HtmlString CodeDocParameterDetails<T>(this ViewPage<T> viewPage, CodeDocParameter parameter) where T:class {
            var htmlBuilder = new StringBuilder();
            if (parameter.HasSummaryContents) {
                htmlBuilder.Append("<div>");
                htmlBuilder.Append(viewPage.XmlDocHtml(parameter.SummaryContents));
                htmlBuilder.Append("</div>");
            }
            htmlBuilder.Append("<div>Type: ");
            htmlBuilder.Append(viewPage.ActionLink(parameter.ParameterType));
            htmlBuilder.Append("</div>");
            // TODO: Flair
            // TODO: Ref/Out params (taken care of by flair?)
            return new HtmlString(htmlBuilder.ToString());
        }

        public static HtmlString CodeDocParameters<T>(this ViewPage<T> viewPage, IEnumerable<CodeDocParameter> parameters) where T:class {
            var tagBuilder = new TagBuilder("dl");
            var innerHtmlBuilder = new StringBuilder();
            foreach (var parameter in parameters) {
                innerHtmlBuilder.Append("<dt>");
                innerHtmlBuilder.Append(HttpUtility.HtmlEncode(parameter.Name));
                innerHtmlBuilder.Append("</dt><dd>");
                innerHtmlBuilder.Append(viewPage.CodeDocParameterDetails(parameter));
                innerHtmlBuilder.Append("</dd>");

            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new HtmlString(tagBuilder.ToString());
        }

        public static HtmlString CodeDocParameters<T>(this ViewPage<T> viewPage, IEnumerable<CodeDocGenericParameter> parameters) where T:class {
            var tagBuilder = new TagBuilder("dl");
            var innerHtmlBuilder = new StringBuilder();
            foreach (var parameter in parameters) {
                innerHtmlBuilder.Append("<dt>");
                innerHtmlBuilder.Append(parameter.Name);
                innerHtmlBuilder.Append("</dt><dd>");

                if (parameter.HasSummaryContents) {
                    innerHtmlBuilder.Append("<p>");
                    innerHtmlBuilder.Append(viewPage.XmlDocHtml(parameter.SummaryContents));
                    innerHtmlBuilder.Append("</p>");
                }

                if (parameter.IsContravariant.GetValueOrDefault())
                    innerHtmlBuilder.Append("<div><i class=\"icon-random\"></i>Contravariant: Type is used for input and can be used with a more specific type.</div>");
                if (parameter.IsCovariant.GetValueOrDefault())
                    innerHtmlBuilder.Append("<div><i class=\"icon-random\"></i>Covariant: Type is used for output and can be used as a more general type.</div>");

                if (parameter.HasAnyConstraints) {
                    innerHtmlBuilder.Append("<div>Constraints:<ul>");

                    if (parameter.HasDefaultConstructorConstraint.GetValueOrDefault())
                        innerHtmlBuilder.Append("<li>Default Constructor</li>");

                    if (parameter.HasNotNullableValueTypeConstraint.GetValueOrDefault())
                        ;//innerHtmlBuilder.Append("<li>Value Type</li>");
                    else if (parameter.HasReferenceTypeConstraint.GetValueOrDefault())
                        innerHtmlBuilder.Append("<li>Reference Type</li>");

                    if (parameter.HasTypeConstraints) {
                        foreach (var typeConstraint in parameter.TypeConstraints) {
                            innerHtmlBuilder.Append("<li>");
                            innerHtmlBuilder.Append(viewPage.ActionLink(typeConstraint));
                            innerHtmlBuilder.Append("</li>");
                        }
                    }

                    innerHtmlBuilder.Append("</ul></div>");
                }

                innerHtmlBuilder.Append("</dd>");
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new HtmlString(tagBuilder.ToString());
        }

        public static HtmlString CodeDocExceptions<T>(this ViewPage<T> viewPage, IEnumerable<CodeDocException> exceptions, object tableTagAttributes = null) where T:class {
            var tagBuilder = new TagBuilder("table");

            if (tableTagAttributes == null)
                tagBuilder.MergeAttribute("class", "table table-bordered table-condensed");
            else
                tagBuilder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var innerHtmlBuilder = new StringBuilder("<thead><tr><th>Exception</th><th>Condition</th></tr></thead><tbody>");
            foreach (var exception in exceptions) {
                innerHtmlBuilder.Append("<tr><td>");
                innerHtmlBuilder.Append(viewPage.ActionLink(exception.ExceptionType));
                innerHtmlBuilder.Append("</td><td>");

                var conditions = exception.HasConditions ? exception.Conditions.Where(x => x.HasChildren).ToList() : new List<XmlDocNode>(0);
                var ensures = exception.HasEnsures ? exception.Ensures.Where(x => x.HasChildren).ToList() : new List<XmlDocNode>(0);

                if (conditions.Count == 1 && ensures.Count == 0) {
                    innerHtmlBuilder.Append(viewPage.XmlDocHtml(conditions[0].Children));
                }
                else {
                    innerHtmlBuilder.Append("<dl>");
                    if (conditions.Count > 0) {
                        innerHtmlBuilder.Append("<dt>Conditions</dt><dd>");
                        innerHtmlBuilder.Append(viewPage.XmlDocHtmlChildListOrSingle(conditions));
                        innerHtmlBuilder.Append("</dd>");
                    }
                    if (ensures.Count > 0) {
                        innerHtmlBuilder.Append("<dt>Ensures</dt><dd>");
                        innerHtmlBuilder.Append(viewPage.XmlDocHtmlChildListOrSingle(ensures));
                        innerHtmlBuilder.Append("</dd>");
                    }
                    innerHtmlBuilder.Append("</dl>");
                }
                innerHtmlBuilder.Append("</td></tr>");
            }
            innerHtmlBuilder.Append("</tbody>");
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();

            return new HtmlString(tagBuilder.ToString());
        }

        private static HtmlString XmlDocHtmlChildListOrSingle<T>(this ViewPage<T> viewPage, IList<XmlDocNode> nodes) where T:class {
            Contract.Requires(nodes != null);
            if (nodes.Count == 0)
                return EmptyHtmlString;
            if (nodes.Count == 1)
                return viewPage.XmlDocHtml(nodes[0].Children);
            var tagBuilder = new TagBuilder("ul");
            var innerHtmlBuilder = new StringBuilder();
            foreach (var node in nodes) {
                innerHtmlBuilder.Append("<li>");
                innerHtmlBuilder.Append(viewPage.XmlDocHtml(node.Children));
                innerHtmlBuilder.Append("</li>");
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new HtmlString(tagBuilder.ToString());
        }

        public static HtmlString CodeDocSimpleCodeContractTable<T>(this ViewPage<T> viewPage, IEnumerable<XmlDocContractElement> contracts, object tableTagAttributes = null) where T:class {
            var tagBuilder = new TagBuilder("table");

            if (tableTagAttributes == null)
                tagBuilder.MergeAttribute("class", "table table-bordered table-condensed");
            else
                tagBuilder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var innerHtmlBuilder = new StringBuilder("<thead><tr><th>Description</th><th>Code</th></tr></thead><tbody>");
            foreach (var contract in contracts) {
                innerHtmlBuilder.Append("<tr><td>");
                innerHtmlBuilder.Append(viewPage.XmlDocHtml(contract.Children));
                innerHtmlBuilder.Append("</td><td>");
                var code = contract.CSharp ?? contract.VisualBasic;
                if (!String.IsNullOrWhiteSpace(code)) {
                    innerHtmlBuilder.Append("<code>");
                    innerHtmlBuilder.Append(HttpUtility.HtmlEncode(code));
                    innerHtmlBuilder.Append("</code>");
                }
                innerHtmlBuilder.Append("</td></tr>");
            }
            innerHtmlBuilder.Append("</tbody>");
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();
            return new HtmlString(tagBuilder.ToString());
        }

        public static HtmlString CodeDocAccessor<T>(this ViewPage<T> viewPage, CodeDocMethod accessor) where T :class {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append(FlairTable(accessor));

            if (accessor.HasExceptions) {
                htmlBuilder.Append("<section><h3>Exceptions</h3>");
                htmlBuilder.Append(viewPage.CodeDocExceptions(accessor.Exceptions));
                htmlBuilder.Append("</section>");
            }
            if (accessor.HasRequires) {
                htmlBuilder.Append("<section><h3>Requires</h3>");
                htmlBuilder.Append(viewPage.CodeDocSimpleCodeContractTable(accessor.Requires));
                htmlBuilder.Append("</section>");
            }
            if (accessor.HasNormalTerminationEnsures) {
                htmlBuilder.Append("<section><h3>Ensures on Successful Execution</h3>");
                htmlBuilder.Append(viewPage.CodeDocSimpleCodeContractTable(accessor.NormalTerminationEnsures));
                htmlBuilder.Append("</section>");
            }
            return new HtmlString(htmlBuilder.ToString());
        }

        public static bool CodeDocAccessorIsWorthDisplaying(CodeDocMethod accessor) {
            return GetFlair(accessor).Any()
                || accessor.HasExceptions
                || accessor.HasNormalTerminationEnsures
                || accessor.HasRequires;
        }

        #region Flair

        public class FlairItem
        {
            public string IconId;
            public string Category;
            public HtmlString Description;

            public FlairItem(string iconId, string category, HtmlString description) {
                IconId = iconId;
                Category = category;
                Description = description;
            }

            public FlairItem(string iconId, string category, string description)
                : this(iconId, category, new HtmlString(description)) { }

        }

        private static readonly FlairItem DefaultStaticTag = new FlairItem("static", "Static", "Accessible relative to a type rather than an object instance.");
        private static readonly FlairItem DefaultObsoleteTag = new FlairItem("obsolete", "Warning", "<strong>Deprecated.</strong>");
        private static readonly FlairItem DefaultPublicTag = new FlairItem("public", "Visibility", "Externally visible.");
        private static readonly FlairItem DefaultProtectedTag = new FlairItem("protected", "Visibility", "Externally visible only through inheritance.");
        private static readonly FlairItem DefaultHiddenTag = new FlairItem("hidden", "Visibility", "Not externally visible.");
        private static readonly FlairItem DefaultCanReturnNullTag = new FlairItem("null-result", "Null Values", "May return null.");
        private static readonly FlairItem DefaultNotNullTag = new FlairItem("no-nulls", "Null Values", "Does not return or accept null values for reference types.");
        private static readonly FlairItem DefaultPureTag = new FlairItem("pure", "Purity", "Does not have side effects.");
        private static readonly FlairItem DefaultOperatorTag = new FlairItem("operator", "Operator", "Invoked through a language operator.");
        private static readonly FlairItem DefaultFlagsTag = new FlairItem("flags", "Enumeration", "Bitwise combination is allowed.");
        private static readonly FlairItem DefaultEnumTypeTag = new FlairItem("enum", "Type", "An enumeration type.");
        private static readonly FlairItem DefaultSealedTag = new FlairItem("sealed", "Inheritance", "Member is sealed, preventing inheritance.");
        private static readonly FlairItem DefaultValueTypeTag = new FlairItem("value-type", "Type", "Type is a value type.");
        private static readonly FlairItem DefaultExtensionMethodTag = new FlairItem("extension", "Extension", "Method is an extension method.");
        private static readonly FlairItem DefaultAbstractTag = new FlairItem("abstract", "Inheritance", "Member is abstract and <strong>must</strong> be implemented by inheriting types.");
        private static readonly FlairItem DefaultVirtualTag = new FlairItem("virtual", "Inheritance", "Member is virtual and can be overridden by inheriting types.");
        private static readonly FlairItem DefaultOverrideTag = new FlairItem("override", "Inheritance", "Member overrides functionality from its parent.");
        private static readonly FlairItem DefaultConstantTag = new FlairItem("constant", "Value", "Field contains a constant compile-time value.");
        private static readonly FlairItem DefaultReadonlyTag = new FlairItem("readonly", "Value", "Member is only assignable during instantiation.");
        private static readonly FlairItem DefaultIndexerOperatorTag = new FlairItem("indexer", "Operator", "This property is invoked through a language index operator.");
        private static readonly FlairItem DefaultGetTag = new FlairItem("get", "Property", "Value can be read externally.");
        private static readonly FlairItem DefaultProGetTag = new FlairItem("proget", "Property", "Value can be read through inheritance.");
        private static readonly FlairItem DefaultSetTag = new FlairItem("set", "Property", "Value can be assigned externally.");
        private static readonly FlairItem DefaultProSetTag = new FlairItem("proset", "Property", "Value can be assigned through inheritance.");

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
            {"value-type","icon-th-large"},
            {"enum","icon-th-list"}
        };

        public static HtmlString FlairIcon(FlairItem item) {
            string cssClass;
            if (SimpleIconClasses.TryGetValue(item.IconId, out cssClass))
                return new HtmlString("<i class=\"" + cssClass + " flair-icon\" title=\"" + item.IconId + "\"></i>");

            switch (item.IconId) {
            case "get": return new HtmlString("<code class=\"flair-icon\" title=\"getter\">get</code>");
            case "proget": return new HtmlString("<code class=\"flair-icon\" title=\"protected getter\"><i class=\"icon-eye-close\"></i>get</code>");
            case "set": return new HtmlString("<code class=\"flair-icon\" title=\"setter\">set</code>");
            case "proset": return new HtmlString("<code class=\"flair-icon\" title=\"protected setter\"><i class=\"icon-eye-close\"></i>set</code>");
            case "indexer": return new HtmlString("<code class=\"flair-icon\" title=\"indexer\">[]</code>");
            case "constant": return new HtmlString("<code class=\"flair-icon\" title=\"constant\">42</code>");
            case "readonly": return new HtmlString("<code class=\"flair-icon\" title=\"readonly\">get</code>");
            default: return new HtmlString("<code>" + item.IconId + "</code>");
            }
        }

        public static HtmlString FlairIconList(ICodeDocMember member) {
            var flair = GetFlair(member).ToList();
            if (flair.Count == 0)
                return EmptyHtmlString;
            return FlairIconList(flair);
        }

        public static HtmlString FlairIconList(IEnumerable<FlairItem> flairItems) {
            var builder = new StringBuilder();
            foreach (var flairItem in flairItems) {
                builder.Append(FlairIcon(flairItem));
            }
            return new HtmlString(builder.ToString());
        }

        public static HtmlString FlairTable(ICodeDocMember member) {
            var flair = GetFlair(member).ToList();
            if (flair.Count == 0)
                return EmptyHtmlString;
            return FlairTable(flair);
        }

        public static HtmlString FlairTable(IEnumerable<FlairItem> items) {
            var tagbuilder = new TagBuilder("table");
            tagbuilder.MergeAttribute("class", "table table-bordered table-condensed");
            var rowBuilder = new StringBuilder();
            foreach (var item in items.OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)) {
                rowBuilder.Append("<tr><th>");
                rowBuilder.Append(FlairIcon(item));
                rowBuilder.Append("</th><td>");
                rowBuilder.Append(HttpUtility.HtmlEncode(item.Category));
                rowBuilder.Append("</td><td>");
                rowBuilder.Append(item.Description);
                rowBuilder.Append("</td></tr>");
            }
            tagbuilder.InnerHtml = rowBuilder.ToString();
            return new HtmlString(tagbuilder.ToString());
        }

        private static IEnumerable<FlairItem> GetFlair(ICodeDocMember member) {
            Contract.Requires(member != null);

            var content = member as CodeDocMemberContentBase;
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

            if (content != null) {
                if (content.IsStatic.GetValueOrDefault()) {
                    if (invokable == null || !invokable.IsOperatorOverload.GetValueOrDefault())
                        yield return DefaultStaticTag;
                }

                if (content.IsObsolete.GetValueOrDefault())
                    yield return DefaultObsoleteTag;

            }
            if (type != null) {
                if (invokable == null) {
                    if (type.IsFlagsEnum.GetValueOrDefault())
                        yield return DefaultFlagsTag;

                    if (type.IsEnum.GetValueOrDefault())
                        yield return DefaultEnumTypeTag;
                    else if (type.IsValueType.GetValueOrDefault())
                        yield return DefaultValueTypeTag;
                    else if (type.IsSealed.GetValueOrDefault() && !type.IsStatic.GetValueOrDefault())
                        yield return DefaultSealedTag;
                }
            }

            if (invokable != null) {
                if (HasReferenceParamsOrReturnAndAllAreNullRestricted(invokable))
                    yield return DefaultNotNullTag;
                if (invokable.IsPure.GetValueOrDefault())
                    yield return DefaultPureTag;
                if (invokable.IsExtensionMethod.GetValueOrDefault())
                    yield return DefaultExtensionMethodTag;
                if (invokable.IsOperatorOverload.GetValueOrDefault())
                    yield return DefaultOperatorTag;

                if (type == null) {
                    if (invokable.IsSealed.GetValueOrDefault())
                        yield return DefaultSealedTag;
                    else if (invokable.IsAbstract.GetValueOrDefault())
                        yield return DefaultAbstractTag;
                    else if (invokable.IsVirtual.GetValueOrDefault())
                        yield return DefaultVirtualTag;
                }
            }

            if (field != null) {
                if (field.IsLiteral.GetValueOrDefault())
                    yield return DefaultConstantTag;
                if (field.IsInitOnly.GetValueOrDefault())
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

        private static IEnumerable<FlairItem> GetFlair(CodeDocParameter parameter) {
            Contract.Requires(parameter != null);
            if (parameter.NullRestricted.GetValueOrDefault())
                throw new NotImplementedException();

            if (parameter.IsOut.GetValueOrDefault())
                throw new NotImplementedException();
            else if (parameter.IsByRef.GetValueOrDefault())
                throw new NotImplementedException();

            // TODO: instant handle?
            yield break;
        }

        private static IEnumerable<FlairItem> GetFlair(CodeDocGenericParameter parameter) {
            Contract.Requires(parameter != null);
            if (parameter.IsCovariant.GetValueOrDefault())
                throw new NotImplementedException();
            else if (parameter.IsContravariant.GetValueOrDefault())
                throw new NotImplementedException();
            yield break;
        }

        #endregion

        private static bool HasReferenceParamsOrReturnAndAllAreNullRestricted(ICodeDocInvokable invokable) {
            Contract.Requires(invokable != null);

            if (!invokable.HasParameters)
                return false;

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

        public static HtmlString CodeDocEntityTable<T>(this ViewPage<T> viewPage, IEnumerable<ICodeDocMember> entities, object tableTagAttributes = null) where T :class {
            var builder = new TagBuilder("table");

            if (tableTagAttributes == null)
                builder.MergeAttribute("class", "table table-bordered");
            else
                builder.MergeAttributes(new RouteValueDictionary(tableTagAttributes));

            var tableInnerHtmlBuilder = new StringBuilder("<thead><tr><th><i class=\"icon-info-sign\"></i></th><th>Name</th><th>Description</th></tr></thead><tbody>");
            foreach (var entity in entities.OrderBy(x => x.Title)) {
                tableInnerHtmlBuilder.Append("<tr><td>");
                tableInnerHtmlBuilder.Append(FlairIconList(entity));
                tableInnerHtmlBuilder.Append("</td><td>");
                tableInnerHtmlBuilder.Append(viewPage.ActionLink(entity));
                tableInnerHtmlBuilder.Append("</td><td>");
                if (entity.HasSummaryContents)
                    tableInnerHtmlBuilder.Append(viewPage.XmlDocHtml(entity.SummaryContents));

                tableInnerHtmlBuilder.Append("</td></tr>");
            }
            tableInnerHtmlBuilder.Append("</tbody>");
            builder.InnerHtml = tableInnerHtmlBuilder.ToString();

            return new HtmlString(builder.ToString());
        }

        #region Member Linking

        private static string MemberUri<T>(this ViewPageBase<T> viewPage, ICodeDocMember member) where T:class {
            Contract.Requires(member != null);

            var uri = member.Uri;
            if (uri != null) {
                if ("HTTP".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase) | "HTTPS".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                    return uri.ToString();

                // while it is less efficient the Uri may contain a more accurate linking CRef than the CRef itself due to byref and arrays
                CRefIdentifier cRef;
                if (CRefIdentifier.TryParse(uri, out cRef))
                    return viewPage.LocalMemberUri(cRef);
            }

            return viewPage.LocalMemberUri(member.CRef);
        }

        private static string LocalMemberUri<T>(this ViewPageBase<T> viewPage, CRefIdentifier cRef) where T : class {
            var cRefUri = "/Docs/Api?cRef=" + Uri.EscapeDataString(cRef.FullCRef);
            return viewPage.CorrectRelativeUri(cRefUri);
        }

        public static string CorrectRelativeUri<T>(this ViewPageBase<T> viewPage, string appUri) where T : class {
            return viewPage.GetRelativeUri(viewPage.FixAbsoluteUri(appUri));
        }

        public static string GetRelativeUri<T>(this ViewPageBase<T> viewPage, string appUri) where T:class {
            var requestPath = viewPage.Request.PathInfo;
            var relative = VirtualPathUtility.MakeRelative(requestPath, appUri);
            return relative;
        }

        public static string FixAbsoluteUri<T>(this ViewPageBase<T> viewPage, string appUri) where T : class {
            if (appUri == "/")
                return "/index.cshtml";
            if ("/Docs/".Equals(appUri, StringComparison.OrdinalIgnoreCase) || "/Docs".Equals(appUri, StringComparison.OrdinalIgnoreCase))
                return "/Docs/index.cshtml";
            return appUri;
        }

        public static HtmlString LinkTextFull(ICodeDocMember member) {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<HtmlString>() != null);
            var rawText = member.FullName;
            if (member is CodeDocSimpleAssembly) {
                var assemblyFileName = ((CodeDocSimpleAssembly)member).AssemblyFileName;
                if (!String.IsNullOrEmpty(assemblyFileName)) {
                    rawText += " (" + assemblyFileName + ')';
                }
            }
            return new HtmlString(HttpUtility.HtmlEncode(rawText));
        }

        public static HtmlString LinkTextShort(ICodeDocMember member) {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<HtmlString>() != null);
            return new HtmlString(HttpUtility.HtmlEncode(member.ShortName));
        }

        public static HtmlString ActionLinkFull<T>(this ViewPage<T> viewPage, ICodeDocMember member) where T:class {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<HtmlString>() != null);
            return viewPage.ActionLink(member, LinkTextFull(member));
        }

        public static HtmlString ActionLink(this ViewPage viewPage, ICodeDocMember member) {
            return ActionLink<dynamic>(viewPage, member);
        }

        public static HtmlString ActionLink<T>(this ViewPageBase<T> viewPage, ICodeDocMember member) where T:class {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<HtmlString>() != null);
            return viewPage.ActionLink(member, LinkTextShort(member));
        }

        public static HtmlString ActionLink<T>(this ViewPageBase<T> viewPage, ICodeDocMember member, HtmlString linkText) where T :class {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<HtmlString>() != null);

            var uri = viewPage.MemberUri(member);
            if (linkText == null || String.IsNullOrEmpty(linkText.ToString()))
                linkText = LinkTextShort(member);

            var linkBuilder = new TagBuilder("a");
            linkBuilder.MergeAttribute("href", uri);
            linkBuilder.InnerHtml = linkText.ToString();
            return new HtmlString(linkBuilder.ToString());
        }

        public static HtmlString ActionLink<T>(this ViewPage<T> viewPage, CRefIdentifier cRef, HtmlString linkText) where T:class {
            Contract.Requires(viewPage != null);
            Contract.Requires(cRef != null);

            var repositories = viewPage.AppHost.TryResolve<CodeDocRepositories>();

            if (repositories != null) {
                var member = repositories.GetModel(cRef);
                if (member != null) {
                    return viewPage.ActionLink(member, linkText);
                }
            }

            var href = viewPage.LocalMemberUri(cRef);
            if (linkText == null || String.IsNullOrEmpty(linkText.ToString()))
                linkText = new HtmlString(HttpUtility.HtmlEncode(cRef.FullCRef));

            var linkBuilder = new TagBuilder("a");
            linkBuilder.MergeAttribute("href", href);
            linkBuilder.InnerHtml = linkText.ToString();
            return new HtmlString(linkBuilder.ToString());
        }

        public static HtmlString ActionLinkList<T>(this ViewPage<T> viewPage, IEnumerable<ICodeDocMember> entities, object ulTagAttributes = null, object liTagAttributes = null) where T:class {
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
                if (liTagAttributesDictionary == null)
                    liTagBuilder.MergeAttribute("class", "text-indent: -7px;");
                else
                    liTagBuilder.MergeAttributes(liTagAttributesDictionary);
                liTagBuilder.InnerHtml = viewPage.ActionLink(entity).ToString();
                innerHtmlBuilder.Append(liTagBuilder);
            }
            tagBuilder.InnerHtml = innerHtmlBuilder.ToString();

            return new HtmlString(tagBuilder.ToString());
        }

        #endregion

    }
}