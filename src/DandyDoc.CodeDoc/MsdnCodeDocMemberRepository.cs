using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Text;
using System.Xml;
using DandyDoc.CRef;
using DandyDoc.CodeDoc.MtpsServiceReference;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class MsdnCodeDocMemberRepository : ICodeDocMemberRepository
    {

        private class MtpsIdentifier
        {

            internal const string AssetIdPrefix = "AssetId:";

            public static string AppendAssetIdPrefixIfRequired(string assetId) {
                Contract.Requires(!String.IsNullOrEmpty(assetId));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return assetId.StartsWith(AssetIdPrefix) ? assetId : String.Concat(AssetIdPrefix, assetId);
            }

            public static string RemoveAssetIdPrefixIfFound(string assetId) {
                Contract.Requires(!String.IsNullOrEmpty(assetId));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return assetId.StartsWith(AssetIdPrefix) ? assetId.Substring(AssetIdPrefix.Length) : assetId;
            }

            public MtpsIdentifier(string assetId, string version, string locale) {
                Contract.Requires(!String.IsNullOrEmpty(assetId));
                Contract.Requires(!String.IsNullOrEmpty(version));
                Contract.Requires(!String.IsNullOrEmpty(locale));
                AssetId = assetId;
                Version = version;
                Locale = locale;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariant() {
                Contract.Invariant(!String.IsNullOrEmpty(AssetId));
                Contract.Invariant(!String.IsNullOrEmpty(Version));
                Contract.Invariant(!String.IsNullOrEmpty(Locale));
            }

            public string AssetId { get; private set; }

            public string Version { get; private set; }

            public string Locale { get; private set; }

        }

        private class MtpsNodeCore
        {

            private static readonly string[] NamespaceEnding = new[] { " Namespace" };
            private static readonly string[] TypeEndings = new[] { " Class", " Interface", " Structure", "Enumeration", " Delegate" };
            private static readonly string[] GroupEndings = new[] { "Namespaces", "Fields", "Properties", "Methods", "Members", "Constructors", "Events", "Overload" };

            public MtpsNodeCore(MtpsIdentifier subTreeId, MtpsIdentifier targetId, string title, MtpsNodeCore parent, bool phantom = false) {
                SubTreeId = subTreeId;
                TargetId = targetId;
                Title = title;
                Parent = parent;
                IsPhantom = phantom;
            }

            public MtpsIdentifier SubTreeId { get; private set; }
            public MtpsIdentifier TargetId { get; private set; }
            public string Title { get; private set; }
            public MtpsNodeCore Parent { get; private set; }
            public bool IsPhantom { get; private set; }

            public bool IsNamespace {
                get { return GetNamespaceName() != null; }
            }

            public string GetNamespaceName() {
                if (null != TargetId) {
                    var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
                    if (assetPrefixRemoved.StartsWith("N:")) {
                        return assetPrefixRemoved.Substring(2);
                    }
                }
                if (null != Title) {
                    foreach (var namespaceEnding in NamespaceEnding) {
                        if (Title.EndsWith(namespaceEnding)) {
                            return Title.Substring(0, Title.Length - namespaceEnding.Length);
                        }
                    }
                }
                return null;
            }

            public bool IsNodeGroup {
                get {
                    if (null != TargetId) {
                        var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
                        foreach (var groupEnding in GroupEndings) {
                            if (assetPrefixRemoved.StartsWith(groupEnding + ':')) {
                                return true;
                            }
                        }
                    }
                    if (null != Title) {
                        foreach (var groupEnding in GroupEndings) {
                            if (Title.EndsWith(' ' + groupEnding)) {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }

            public string GetNamespaceGroupName() {
                const string namespacesEnding = " Namespaces";
                if (null != Title) {
                    if (Title.EndsWith(namespacesEnding))
                        return Title.Substring(0, Title.Length - namespacesEnding.Length);
                }
                return null;
            }

            public string GetOverloadGroupName() {
                const string overloadPrefix = "Overload:";
                if (null != TargetId) {
                    var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
                    if (assetPrefixRemoved.StartsWith(overloadPrefix)) {
                        return assetPrefixRemoved.Substring(overloadPrefix.Length);
                    }
                }
                return null;
            }

            public bool IsTypeOrMember {
                get {
                    var name = ExtractFullTypeOrMemberName();
                    if (!String.IsNullOrEmpty(name))
                        return true;
                    name = ExtractPartName();
                    if (!String.IsNullOrEmpty(name))
                        return true;
                    return false;
                }
            }

            private string ExtractPartName() {
                if (null != Title) {
                    foreach (var typeEnding in TypeEndings) {
                        if (Title.EndsWith(typeEnding)) {
                            return Title.Substring(0, Title.Length - typeEnding.Length);
                        }
                    }
                }
                return null;
            }

            private string ExtractFullTypeOrMemberName() {
                if (null != TargetId) {
                    var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
                    if (assetPrefixRemoved.Length > 2 && assetPrefixRemoved[1] == ':') {
                        return assetPrefixRemoved.Substring(2);
                    }
                }
                return null;
            }

            public string GetFullName() {
                if (IsTypeOrMember) {
                    var name = ExtractFullTypeOrMemberName();
                    if (!String.IsNullOrEmpty(name))
                        return name;
                    name = ExtractPartName();
                    if (null != Parent && Parent.IsNamespace) {
                        var ns = Parent.GetNamespaceName();
                        if (!String.IsNullOrEmpty(ns)) {
                            name = String.Concat(ns, '.', name);
                        }
                    }
                    return name;
                }
                if (IsNamespace) {
                    return GetNamespaceName();
                }
                var nsGroupName = GetNamespaceGroupName();
                if (!String.IsNullOrEmpty(nsGroupName))
                    return nsGroupName;
                var overloadGroupName = GetOverloadGroupName();
                if (!String.IsNullOrEmpty(overloadGroupName))
                    return overloadGroupName;
                return null;
            }

            public string GetCrefName() {
                const string assetIdPrefix = "AssetId:";
                if (null == TargetId)
                    return null;

                var result = TargetId.AssetId;
                if (result.StartsWith(assetIdPrefix))
                    result = result.Substring(assetIdPrefix.Length);

                if (result.Length > 2 && result[1] == ':') {
                    result = result.Substring(2);
                    return result;
                }
                return null;
            }

            public override string ToString() {
                return Title;
            }
        }

        private class MtpsNavigationNode : MtpsNodeCore
        {

            private static readonly ReadOnlyCollection<MtpsNodeCore> EmptyChildrenCollection
                = new ReadOnlyCollection<MtpsNodeCore>(new MtpsNodeCore[0]);

            public MtpsNavigationNode(
                MtpsIdentifier subTreeId,
                MtpsIdentifier targetId,
                MtpsNodeCore parent = null,
                bool phantom = false,
                Guid? guid = null,
                string contentId = null,
                string alias = null,
                string title = null,
                IList<MtpsNodeCore> childLinks = null
            )
                : base(subTreeId, targetId, title, parent, phantom) {
                Guid = guid;
                ContentId = contentId;
                Alias = alias;
                ChildLinks = null == childLinks ? EmptyChildrenCollection : new ReadOnlyCollection<MtpsNodeCore>(childLinks);
            }



            public Guid? Guid { get; private set; }
            public string ContentId { get; private set; }
            public string Alias { get; private set; }
            public ReadOnlyCollection<MtpsNodeCore> ChildLinks { get; private set; }

        }

        public static string DefaultRootAssetId{
            get{
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "AssetId:2c606a4d-a51a-bdd7-020c-73f9081c4e33";
            }
        }

        public static string DefaultVersion{
            get{
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "VS.110";
            }
        }

        public static string DefaultLocale{
            get{
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "en-us";
            }
        }

        public static string DefaultServiceUrl{
            get{
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "http://services.msdn.microsoft.com/ContentServices/ContentService.asmx";
            }
        }

        public MsdnCodeDocMemberRepository(string rootAssetId = null, string version = null, string locale = null, string serviceUrl = null) {
            RootAssetId = String.IsNullOrEmpty(rootAssetId) ? DefaultRootAssetId : rootAssetId;
            Version = String.IsNullOrEmpty(version) ? DefaultVersion : version;
            Locale = String.IsNullOrEmpty(locale) ? DefaultLocale : locale;
            ServiceUrl = String.IsNullOrEmpty(serviceUrl) ? DefaultServiceUrl : serviceUrl;
            _appId = new appId();
            _clientGenerator = new Lazy<ContentServicePortTypeClient>(
                () => new ContentServicePortTypeClient(
                    new BasicHttpBinding{
                        MaxReceivedMessageSize = 1024 * 1024
                    },
                    new EndpointAddress(ServiceUrl)
                ),
                true
            );
        }

        private void CodeContractInvariants(){
            Contract.Invariant(!String.IsNullOrEmpty(RootAssetId));
            Contract.Invariant(!String.IsNullOrEmpty(Version));
            Contract.Invariant(!String.IsNullOrEmpty(Locale));
            Contract.Invariant(!String.IsNullOrEmpty(ServiceUrl));
            Contract.Invariant(_appId != null);
            Contract.Invariant(_clientGenerator != null);
        }

        private readonly Lazy<ContentServicePortTypeClient> _clientGenerator;

        private ContentServicePortTypeClient Client{
            get{
                Contract.Ensures(Contract.Result<ContentServicePortTypeClient>() != null);
                return _clientGenerator.Value;
            }
        }

        private ObjectCache _cache;
        public ObjectCache Cache{
            get{
                Contract.Ensures(Contract.Result<ObjectCache>() != null);
                return _cache ?? MemoryCache.Default;
            }
            set{
                if(value == null) throw new ArgumentNullException("value");
                Contract.EndContractBlock();
                _cache = value;
            }
        }

        private readonly appId _appId;

        public string RootAssetId { get; private set; }

        public string Version { get; private set; }

        public string Locale { get; private set; }

        public string ServiceUrl { get; private set; }

        public ICodeDocMember GetMemberModel(string cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            Contract.Requires(!String.IsNullOrEmpty(cRef));
            return GetMemberModel(new CRefIdentifier(cRef), searchContext, detailLevel);
        }

        public ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var tocResults = SearchToc(cRef.FullCRef);
            var bestTocResult = tocResults.FirstOrDefault();
            stopwatch.Stop();
            Debug.WriteLine("Search for {0} returned in {1}", cRef, stopwatch.Elapsed);

            if (bestTocResult == null)
                return null;

            Uri uri;
            if (!Uri.TryCreate(GetUrl(bestTocResult), UriKind.Absolute, out uri))
                uri = cRef.ToUri();

            CodeDocSimpleMember model = null;

            if (detailLevel != CodeDocMemberDetailLevel.Minimum) {
                var contentXml = GetContent(bestTocResult.ContentId);
                if (contentXml != null) {

                    bool? isSealed = null;
                    bool? isValueType = null;
                    bool? isReferenceType = null;
                    bool? isEnum = null;
                    bool? isVirtual = null;
                    bool? isAbstract = null;
                    bool? isMethod = null;
                    ExternalVisibilityKind? visibility = null;

                    var titleDiv = contentXml.GetElementsByTagName("div")
                        .OfType<XmlElement>()
                        .FirstOrDefault(x => "TITLE".Equals(x.GetAttribute("class"), StringComparison.OrdinalIgnoreCase));

                    if (titleDiv != null) {
                        var innerTitleText = titleDiv.InnerText.Trim();
                        if (innerTitleText.EndsWith("Method", StringComparison.OrdinalIgnoreCase)) {
                            isMethod = true;
                        }
                    }

                    var syntaxElement = contentXml.GetElementsByTagName("mtps:CollapsibleArea")
                        .OfType<XmlElement>()
                        .FirstOrDefault(x => "SYNTAX".Equals(x.GetAttribute("Title"), StringComparison.OrdinalIgnoreCase));
                    var codeSnippets = syntaxElement.GetElementsByTagName("mtps:CodeSnippet").OfType<XmlElement>().ToList();

                    foreach (var snippet in codeSnippets) {
                        var inputSpans = snippet.GetElementsByTagName("span")
                            .OfType<XmlElement>()
                            .Where(e => e.GetAttribute("class") == "input")
                            .ToList();
                        if (snippet.GetAttribute("Language") == "CSharp") {
                            foreach (var inputSpan in inputSpans) {
                                var innerText = inputSpan.InnerText;
                                if ("PUBLIC".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    visibility = ExternalVisibilityKind.Public;
                                else if ("PROTECTED".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    visibility = ExternalVisibilityKind.Protected;
                                else if ("SEALED".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isSealed = true;
                                else if ("CLASS".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isReferenceType = true;
                                else if ("STRUCT".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isValueType = true;
                                else if ("ENUM".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isEnum = true;
                                else if ("VIRTUAL".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isVirtual = true;
                                else if ("ABSTRACT".Equals(innerText, StringComparison.OrdinalIgnoreCase))
                                    isAbstract = true;
                            }
                        }
                    }

                    if (model == null) {
                        if (isReferenceType ?? isValueType ?? isEnum ?? false) {
                            var typeModel = new CodeDocType(cRef);
                            typeModel.IsSealed = isSealed;
                            typeModel.IsValueType = isValueType;
                            typeModel.IsEnum = isEnum;
                            model = typeModel;
                        }
                        else if (isMethod ?? false) {
                            var methodModel = new CodeDocMethod(cRef);
                            methodModel.IsSealed = isSealed;
                            methodModel.IsVirtual = isVirtual;
                            methodModel.IsAbstract = isAbstract;
                            model = methodModel;
                        }
                        else {
                            model = new CodeDocSimpleMember(cRef);
                        }
                        model.ExternalVisibility = visibility ?? ExternalVisibilityKind.Hidden;
                    }

                    XmlNode summaryElement = contentXml.GetElementsByTagName("div")
                        .OfType<XmlElement>()
                        .FirstOrDefault(x => String.Equals(x.GetAttribute("class"), "summary", StringComparison.OrdinalIgnoreCase));
                    if (summaryElement != null) {
                        if (summaryElement.ChildNodes.Count == 1 && summaryElement.ChildNodes[0].Name == "p") {
                            // unwrap the lone p tag.
                            summaryElement = summaryElement.ChildNodes[0];
                        }
                        var summaryXmlDoc = XmlDocParser.Default.Parse(summaryElement);
                        model.SummaryContents = summaryXmlDoc.Children;
                    }

                }

            }

            if (model == null)
                model = new CodeDocSimpleMember(cRef);

            if(model.ExternalVisibility == ExternalVisibilityKind.Hidden)
                model.ExternalVisibility = ExternalVisibilityKind.Public;

            model.Uri = uri;
            model.FullName = bestTocResult.GetFullName();

            int lastTitleSpaceIndex = bestTocResult.Title.LastIndexOf(' ');
            if (lastTitleSpaceIndex >= 0) {
                model.Title = bestTocResult.Title.Substring(0, lastTitleSpaceIndex);
                model.SubTitle = bestTocResult.Title.Substring(lastTitleSpaceIndex + 1);
            }
            else {
                model.Title = bestTocResult.Title;
                model.SubTitle = String.Empty;
            }
            model.ShortName = model.Title;

            if (String.IsNullOrEmpty(model.NamespaceName)) {
                MtpsNodeCore namespaceNode = bestTocResult;
                while (namespaceNode != null && !namespaceNode.IsNamespace) {
                    namespaceNode = namespaceNode.Parent;
                }
                if (namespaceNode != null) {
                    model.NamespaceName = namespaceNode.GetNamespaceName();
                    model.Namespace = GetNamespaceByName(model.NamespaceName);
                }
            }

            return model;
        }

        public IList<CodeDocSimpleAssembly> Assemblies {
            get{
                // NOTE: I am not even sure how to get assemblies from MTPS
                return new CodeDocSimpleAssembly[0];
            }
        }

        public IList<CodeDocSimpleNamespace> Namespaces {
            get{
                // TODO: a list of namespaces may be useful in the future, but would be costly at startup
                return new CodeDocSimpleNamespace[0];
            }
        }

        private CodeDocSimpleNamespace GetNamespaceByName(string namespaceName){
            // TODO: in the future, this should locate the namespace from a cached list
            return new CodeDocSimpleNamespace(new CRefIdentifier("N:" + namespaceName));
        }

        private string GetBestLocale(MtpsNodeCore node) {
            Contract.Requires(null != node);
            if (null != node.TargetId) {
                if (!String.IsNullOrEmpty(node.TargetId.Locale))
                    return node.TargetId.Locale;
            }
            if (null != node.SubTreeId) {
                if (!String.IsNullOrEmpty(node.SubTreeId.Locale))
                    return node.SubTreeId.Locale;
            }
            return Locale;
        }

        private string GetUrl(MtpsNavigationNode node) {
            Contract.Requires(node != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var locale = GetBestLocale(node);
            var id = String.IsNullOrEmpty(node.Alias) ? node.ContentId : node.Alias;
            return String.Format("http://msdn.microsoft.com/{0}/library/{1}.aspx", locale ?? Locale, id);
        }

        private string CreateGetContentRequestKey(getContentRequest request){
            return String.Format(
                "MTPS_GetContent({0},{1},{2},[{3}])",
                request.contentIdentifier,
                request.locale,
                request.version,
                String.Join(",",(request.requestedDocuments ?? Enumerable.Empty<requestedDocument>()).Select(d => String.Concat(d.type,d.selector)))
            );
        }

        private getContentResponse CachedGetContentRequest(appId appId, getContentRequest request) {
            var requestKey = CreateGetContentRequestKey(request);
            var rawResult = Cache[requestKey] as getContentResponse;
            if (rawResult == null) {
                rawResult = Client.GetContent(appId, request);
                if (rawResult != null)
                    Cache[requestKey] = rawResult;
            }
            return rawResult;
        }

        private XmlElement GetTocXmlElementRequest(string assetId, string version, string locale) {
            Contract.Requires(!String.IsNullOrEmpty(assetId));
            var request = new getContentRequest {
                contentIdentifier = MtpsIdentifier.AppendAssetIdPrefixIfRequired(assetId),
                locale = locale,
                version = version,
                requestedDocuments = new[]{
                    new requestedDocument{
                        type = documentTypes.primary,
                        selector = "Mtps.Toc"
                    }
                }
            };

            var rawResult = CachedGetContentRequest(_appId, request);

            if (rawResult == null)
                return null;

            var root = rawResult.primaryDocuments.SingleOrDefault();
            if (null == root)
                return null;

            return root.Any;
        }

        private XmlElement GetTocXmlElement(string assetId, string version, string locale) {
            Contract.Requires(!String.IsNullOrEmpty(assetId));
            if (String.IsNullOrEmpty(version))
                version = Version;
            if (String.IsNullOrEmpty(locale))
                locale = Locale;
            return GetTocXmlElementRequest(assetId, version, locale);
        }

        private getContentResponse GetContentResponse(string assetId, string version, string locale) {
            Contract.Requires(!String.IsNullOrEmpty(assetId));
            if (String.IsNullOrEmpty(version))
                version = Version;
            if (String.IsNullOrEmpty(locale))
                locale = Locale;

            //return Client.GetContent(_appId, new getContentRequest { contentIdentifier = assetId, locale = locale, version = version });
            return CachedGetContentRequest(_appId, new getContentRequest {contentIdentifier = assetId, locale = locale, version = version});
        }

        private getContentResponse GetContentResponse(MtpsIdentifier identifier) {
            Contract.Requires(null != identifier);
            return GetContentResponse(identifier.AssetId, identifier.Version, identifier.Locale);
        }

        private MtpsNavigationNode ToNavigationNode(MtpsNodeCore parent, XmlElement xmlElement) {
            Contract.Requires(null != xmlElement);
            Contract.Ensures(Contract.Result<MtpsNavigationNode>() != null);

            var subTreeAssetId = xmlElement.Attributes["toc:SubTree"].Value;
            Contract.Assume(!String.IsNullOrEmpty(subTreeAssetId));
            var subTreeVersion = xmlElement.Attributes["toc:SubTreeVersion"].Value;
            Contract.Assume(!String.IsNullOrEmpty(subTreeVersion));
            var subTreeLocale = xmlElement.Attributes["toc:SubTreeLocale"].Value;
            Contract.Assume(!String.IsNullOrEmpty(subTreeLocale));
            var subTreeId = new MtpsIdentifier(subTreeAssetId, subTreeVersion, subTreeLocale);

            var title = xmlElement.GetAttribute("toc:Title");
            var targetAssetId = xmlElement.GetAttribute("toc:Target");
            var targetLocale = xmlElement.GetAttribute("toc:TargetLocale");
            var targetVersion = xmlElement.GetAttribute("toc:TargetVersion");
            bool isPhantom;
            Boolean.TryParse(xmlElement.GetAttribute("toc:IsPhantom"), out isPhantom);
            ;
            var children = new List<MtpsNodeCore>();
            foreach (var childElement in xmlElement.ChildNodes.Cast<XmlElement>()) {
                MtpsIdentifier childSubTree = null;
                var childSubTreeId = childElement.GetAttribute("toc:SubTree");
                if (!String.IsNullOrEmpty(childSubTreeId)) {
                    var version = childElement.GetAttribute("toc:SubTreeVersion");
                    Contract.Assume(!String.IsNullOrEmpty(version));
                    var locale = childElement.GetAttribute("toc:SubTreeLocale");
                    Contract.Assume(!String.IsNullOrEmpty(locale));
                    childSubTree = new MtpsIdentifier(childSubTreeId, version, locale);
                }

                MtpsIdentifier childTarget = null;
                var childTargetId = childElement.GetAttribute("toc:Target");
                if (!String.IsNullOrEmpty(childTargetId)) {
                    var version = childElement.GetAttribute("toc:TargetVersion");
                    Contract.Assume(!String.IsNullOrEmpty(version));
                    var locale = childElement.GetAttribute("toc:TargetLocale");
                    Contract.Assume(!String.IsNullOrEmpty(locale));
                    childTarget = new MtpsIdentifier(childTargetId, version, locale);
                }

                if (null != childSubTree || null != childTarget) {
                    var childTitle = childElement.GetAttribute("toc:Title");
                    var childLink = new MtpsNodeCore(childSubTree, childTarget, childTitle, parent);
                    children.Add(childLink);
                }
            }

            MtpsIdentifier targetId = null;
            Guid? guid = null;
            string contentId = null;
            string alias = null;
            if (!String.IsNullOrEmpty(targetAssetId)) {
                Contract.Assume(!String.IsNullOrEmpty(targetVersion));
                Contract.Assume(!String.IsNullOrEmpty(targetLocale));
                targetId = new MtpsIdentifier(targetAssetId, targetVersion, targetLocale);
                var contentResponse = GetContentResponse(targetId);
                contentId = contentResponse.contentId;
                var guidText = contentResponse.contentGuid;
                Guid guidValue;
                if (!String.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out guidValue)) {
                    guid = guidValue;
                }
                alias = contentResponse.contentAlias;
            }

            return new MtpsNavigationNode(
                subTreeId: subTreeId,
                targetId: targetId,
                parent: parent,
                phantom: isPhantom,
                guid: guid,
                contentId: contentId,
                alias: alias,
                title: title,
                childLinks: children
            );
        }

        private MtpsNavigationNode ToNavigationNode(MtpsNodeCore parent, MtpsNodeCore node) {
            if (null != node.SubTreeId) {
                var xmlElement = GetTocXmlElement(node.SubTreeId.AssetId, node.SubTreeId.Version, node.SubTreeId.Locale);
                if (null != xmlElement)
                    return ToNavigationNode(parent, xmlElement);
            }

            Guid? guid = null;
            string contentId = null;
            string alias = null;
            if (null != node.TargetId) {
                Contract.Assume(!String.IsNullOrEmpty(node.TargetId.AssetId));
                Contract.Assume(!String.IsNullOrEmpty(node.TargetId.Version));
                Contract.Assume(!String.IsNullOrEmpty(node.TargetId.Locale));
                var contentResponse = GetContentResponse(node.TargetId);
                contentId = contentResponse.contentId;
                var guidText = contentResponse.contentGuid;
                Guid guidValue;
                if (!String.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out guidValue)) {
                    guid = guidValue;
                }
                alias = contentResponse.contentAlias;
            }

            return new MtpsNavigationNode(
                subTreeId: node.SubTreeId,
                targetId: node.TargetId,
                parent: parent,
                phantom: node.IsPhantom,
                guid: guid,
                contentId: contentId,
                alias: alias,
                title: node.Title,
                childLinks: null
            );
        }

        private MtpsNavigationNode GetRootNode() {
            var xmlElement = GetTocXmlElement(RootAssetId, Version, Locale);
            return null == xmlElement ? null : ToNavigationNode(null, xmlElement);
        }

        private IEnumerable<MtpsNavigationNode> SearchChild(string searchName, MtpsNavigationNode parent, MtpsNodeCore childLink) {
            Contract.Requires(!String.IsNullOrEmpty(searchName));
            Contract.Requires(parent != null);
            Contract.Requires(childLink != null);
            Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
            var tocNode = ToNavigationNode(parent, childLink);
            return SearchToc(searchName, tocNode);
        }

        private IEnumerable<MtpsNavigationNode> SearchWithinGroup(string searchName, MtpsNavigationNode parent, MtpsNodeCore childLink) {
            Contract.Requires(!String.IsNullOrEmpty(searchName));
            Contract.Requires(parent != null);
            Contract.Requires(childLink != null);
            Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
            if (null == childLink.SubTreeId)
                return Enumerable.Empty<MtpsNavigationNode>();
            var xmlElement = GetTocXmlElement(childLink.SubTreeId.AssetId, childLink.SubTreeId.Version, childLink.SubTreeId.Locale);
            if (null == xmlElement)
                return Enumerable.Empty<MtpsNavigationNode>();
            var tocNode = ToNavigationNode(parent, xmlElement);
            return SearchChildren(searchName, parent, tocNode.ChildLinks);
        }

        private IEnumerable<MtpsNavigationNode> SearchChildren(string searchName, MtpsNavigationNode node, IEnumerable<MtpsNodeCore> children) {
            Contract.Requires(!String.IsNullOrEmpty(searchName));
            Contract.Requires(node != null);
            Contract.Requires(children != null);
            Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);


            // NOTE: while it would be easy to make this parallel it could result in a performance loss
            // The search collections use enumerable to allow for early search termination when a result matches.
            // Execution in parallel may cause service requests to be invoked even after the correct nodes have been located.
            return children
                .Where(c => !c.IsPhantom)
                .SelectMany(childNode => {
                    if (childNode.IsNamespace || childNode.IsTypeOrMember) {
                        var fullName = childNode.GetFullName();
                        if (!String.IsNullOrEmpty(fullName) && searchName.StartsWith(fullName)) {
                            return SearchChild(searchName, node, childNode);
                        }
                    }
                    else if (childNode.IsNodeGroup) {
                        var groupReferenceName = childNode.GetFullName();
                        if (String.IsNullOrEmpty(groupReferenceName) || searchName.StartsWith(groupReferenceName)){
                            return SearchWithinGroup(searchName, node, childNode);
                        }
                    }
                    return Enumerable.Empty<MtpsNavigationNode>();
                });
        }

        private IEnumerable<MtpsNavigationNode> SearchToc(string searchName, MtpsNavigationNode node) {
            Contract.Requires(!String.IsNullOrEmpty(searchName));
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);

            var results = SearchChildren(searchName, node, node.ChildLinks);
            var nodeFullName = node.GetFullName();
            if (searchName.Equals(nodeFullName)) {
                results = new[] { node }.Concat(results);
            }
            return results;
        }

        private IEnumerable<MtpsNavigationNode> SearchToc(string memberName) {
            if (String.IsNullOrEmpty(memberName)) throw new ArgumentException("memberName");
            Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
            var root = GetRootNode();
            if (null == root)
                throw new InvalidOperationException("The root asset is not valid.");

            if (memberName.Length > 2 && memberName[1] == ':')
                memberName = memberName.Substring(2);
            if (memberName.EndsWith("&") || memberName.EndsWith("@"))
                memberName = memberName.Substring(0, memberName.Length - 1);
            if(memberName.EndsWith("]")){
                var lastOpenSquareBraceIndex = memberName.LastIndexOf('[');
                if(lastOpenSquareBraceIndex >= 0)
                    memberName = memberName.Substring(0,lastOpenSquareBraceIndex);
            }
            Contract.Assume(!String.IsNullOrEmpty(memberName));
            return SearchToc(memberName, root);
        }

        private XmlElement GetContent(string assetId) {
            var request = new getContentRequest {
                contentIdentifier = assetId,
                locale = Locale,
                version = Version,
                requestedDocuments = new [] {
                    new requestedDocument {
                        selector = "Mtps.Xhtml",
                        type = documentTypes.primary
                    }
                }
            };

            var rawResult = CachedGetContentRequest(_appId, request);

            var bestNode = rawResult.primaryDocuments.FirstOrDefault(x => x.primaryFormat == "Mtps.Xhtml");
            if (bestNode == null)
                return null;

            return bestNode.Any;
        }

    }
}
