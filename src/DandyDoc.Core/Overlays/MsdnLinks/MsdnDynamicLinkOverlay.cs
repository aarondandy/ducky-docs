using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.MsdnContentService;
using DandyDoc.Utility;

namespace DandyDoc.Overlays.MsdnLinks
{
	public class MsdnDynamicLinkOverlay : IMsdnLinkOverlay
	{

		public static string DefaultRootAssetId { get { return "AssetId:2c606a4d-a51a-bdd7-020c-73f9081c4e33"; } }

		public static string DefaultVersion { get { return "VS.110"; } }

		public static string DefaultLocale { get { return "en-us"; } }

		public MsdnDynamicLinkOverlay()
			: this(DefaultRootAssetId, DefaultVersion, DefaultLocale) { }

		public MsdnDynamicLinkOverlay(string rootAssetId, string version, string locale){
			if(String.IsNullOrEmpty(rootAssetId)) throw new ArgumentException("A valid root asset ID is required.", "rootAssetId");
			if(String.IsNullOrEmpty(version)) throw new ArgumentException("A valid version is required.", "version");
			if(String.IsNullOrEmpty(locale)) throw new ArgumentException("A valid locale is required.", "locale");
			Contract.EndContractBlock();
			RootAssetId = rootAssetId;
			Version = version;
			Locale = locale;
			_tocClient = new Lazy<ContentServicePortTypeClient>(
				() => new ContentServicePortTypeClient(
					new BasicHttpBinding{MaxReceivedMessageSize = 1024 * 1024},
					new EndpointAddress("http://services.msdn.microsoft.com/ContentServices/ContentService.asmx")
				),
				true
			);
			_myAppId = new appId();
			Cache = MemoryCache.Default;
		}

		private readonly Lazy<ContentServicePortTypeClient> _tocClient;

		private readonly appId _myAppId;

		internal ContentServicePortTypeClient TocClient { get { return _tocClient.Value; } }

		public string RootAssetId { get; private set; }

		public string Version { get; private set; }

		public string Locale { get; private set; }

		protected ObjectCache Cache { get; private set; }

		private string GetBestLocale(MtpsNodeCore node){
			Contract.Requires(null != node);
			if (null != node.TargetId){
				if (!String.IsNullOrEmpty(node.TargetId.Locale))
					return node.TargetId.Locale;
			}
			if (null != node.SubTreeId){
				if (!String.IsNullOrEmpty(node.SubTreeId.Locale))
					return node.SubTreeId.Locale;
			}
			return Locale;
		}

		public string GetUrl(MtpsNavigationNode node){
			if(null == node) throw new ArgumentNullException("node");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			string locale = GetBestLocale(node);
			string id = String.IsNullOrEmpty(node.Alias) ? node.ContentId : node.Alias;
			return String.Format("http://msdn.microsoft.com/{0}/library/{1}.aspx", locale ?? Locale, id);
		}

		public IEnumerable<MtpsNavigationNode> Search(string memberName){
			if(String.IsNullOrEmpty(memberName)) throw new ArgumentException("memberName");
			Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
			var root = GetRootNode();
			if (null == root)
				throw new InvalidOperationException("The root asset is not valid.");
			return Search(memberName, root);
		}

		private XmlElement GetTocXmlElement(string assetId, string version, string locale){
			Contract.Requires(!String.IsNullOrEmpty(assetId));
			if (String.IsNullOrEmpty(version))
				version = Version;
			if (String.IsNullOrEmpty(locale))
				locale = Locale;
			if (null != Cache){
				var cacheKey = String.Format("MsdnDynamicLinkOverlay.GetTocXmlElement({0},{1},{2})", assetId, version, locale);
				if (Cache.Contains(cacheKey))
					return Cache[cacheKey] as XmlElement;

				var result = GetTocXmlElementRequest(assetId, version, locale);
				Cache[cacheKey] = result;
				return result;
			}
			return GetTocXmlElementRequest(assetId, version, locale);
		}

		private XmlElement GetTocXmlElementRequest(string assetId, string version, string locale) {
			Contract.Requires(!String.IsNullOrEmpty(assetId));
			var request = new getContentRequest {
				contentIdentifier = MtpsIdentifier.AppendAssetIdPrefixIfRequired(assetId),
				locale = locale,
				version = version,
				requestedDocuments = new[] { new requestedDocument { type = documentTypes.primary, selector = "Mtps.Toc" } }
			};
			var root = TocClient.GetContent(_myAppId, request).primaryDocuments.SingleOrDefault();
			if (null == root)
				return null;
			return root.Any;
		}

		private getContentResponse GetContentResponse(MtpsIdentifier identifier){
			Contract.Requires(null != identifier);
			return GetContentResponse(identifier.AssetId, identifier.Version, identifier.Locale);
		}

		private getContentResponse GetContentResponse(string assetId, string version, string locale){
			Contract.Requires(!String.IsNullOrEmpty(assetId));
			if (String.IsNullOrEmpty(version))
				version = Version;
			if(String.IsNullOrEmpty(locale))
				locale = Locale;

			if (null != Cache) {
				var cacheKey = String.Format("MsdnDynamicLinkOverlay.GetContentResponse({0},{1},{2})", assetId, version, locale);
				if (Cache.Contains(cacheKey))
					return Cache[cacheKey] as getContentResponse;

				var result = TocClient.GetContent(_myAppId, new getContentRequest { contentIdentifier = assetId, locale = locale, version = version });
				Cache[cacheKey] = result;
				return result;
			}
			return TocClient.GetContent(_myAppId, new getContentRequest { contentIdentifier = assetId, locale = locale, version = version });
		}

		private MtpsNavigationNode ToNavigationNode(MtpsNodeCore parent, MtpsNodeCore node){
			if (null != node.SubTreeId){
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
				guid: guid,
				contentId: contentId,
				alias: alias,
				title: node.Title,
				childLinks: null
			);
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

			var title = xmlElement.Attributes.GetValueOrDefault("toc:Title");
			var targetAssetId = xmlElement.Attributes.GetValueOrDefault("toc:Target");
			var targetLocale = xmlElement.Attributes.GetValueOrDefault("toc:TargetLocale");
			var targetVersion = xmlElement.Attributes.GetValueOrDefault("toc:TargetVersion");

			var children = new List<MtpsNodeCore>();
			foreach (var childElement in xmlElement.ChildNodes.Cast<XmlElement>()){
				MtpsIdentifier childSubTree = null;
				var childSubTreeId = childElement.Attributes.GetValueOrDefault("toc:SubTree");
				if (!String.IsNullOrEmpty(childSubTreeId)){
					var version = childElement.Attributes.GetValueOrDefault("toc:SubTreeVersion");
					Contract.Assume(!String.IsNullOrEmpty(version));
					var locale = childElement.Attributes.GetValueOrDefault("toc:SubTreeLocale");
					Contract.Assume(!String.IsNullOrEmpty(locale));
					childSubTree = new MtpsIdentifier(childSubTreeId, version, locale);
				}

				MtpsIdentifier childTarget = null;
				var childTargetId = childElement.Attributes.GetValueOrDefault("toc:Target");
				if (!String.IsNullOrEmpty(childTargetId)){
					var version = childElement.Attributes.GetValueOrDefault("toc:TargetVersion");
					Contract.Assume(!String.IsNullOrEmpty(version));
					var locale = childElement.Attributes.GetValueOrDefault("toc:TargetLocale");
					Contract.Assume(!String.IsNullOrEmpty(locale));
					childTarget = new MtpsIdentifier(childTargetId, version, locale);
				}

				if (null != childSubTree || null != childTarget){
					var childTitle = childElement.Attributes.GetValueOrDefault("toc:Title");
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
				if (!String.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out guidValue)){
					guid = guidValue;
				}
				alias = contentResponse.contentAlias;
			}

			return new MtpsNavigationNode(
				subTreeId: subTreeId,
				targetId: targetId,
				parent: parent,
				guid: guid,
				contentId: contentId,
				alias: alias,
				title: title,
				childLinks: children
			);
		}

		private MtpsNavigationNode GetRootNode(){
			var xmlElement = GetTocXmlElement(RootAssetId, Version, Locale);
			return null == xmlElement ? null : ToNavigationNode(null, xmlElement);
		}

		private IEnumerable<MtpsNavigationNode> Search(string searchName, MtpsNavigationNode node){
			Contract.Requires(!String.IsNullOrEmpty(searchName));
			Contract.Requires(node != null);
			Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);

			var results = new List<MtpsNavigationNode>(0);
			var nodeFullName = node.GetFullName();
			if (searchName.Equals(nodeFullName))
				results.Add(node);

			results.AddRange(SearchChildren(searchName, node, node.ChildLinks));
			return results;
		}

		private IEnumerable<MtpsNavigationNode> SearchChildren(string searchName, MtpsNavigationNode node, IEnumerable<MtpsNodeCore> children){
			Contract.Requires(!String.IsNullOrEmpty(searchName));
			Contract.Requires(node != null);
			Contract.Requires(children != null);
			Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);

			var results = new List<MtpsNavigationNode>(0);
			foreach (var childNode in children) {
				if (childNode.IsNamespace || childNode.IsTypeOrMember) {
					var fullName = childNode.GetFullName();
					if (!String.IsNullOrEmpty(fullName) && searchName.StartsWith(fullName)) {
						results.AddRange(SearchChild(searchName, node, childNode));
					}
				}
				else if (childNode.IsNodeGroup){
					var groupReferenceName = childNode.GetFullName();
					if (!String.IsNullOrEmpty(groupReferenceName)) {
						if (!searchName.StartsWith(groupReferenceName)) {
							continue;
						}
					}
					results.AddRange(SearchWithinGroup(searchName, node, childNode));
				}
			}
			return results;
		}

		private IEnumerable<MtpsNavigationNode> SearchChild(string searchName, MtpsNavigationNode parent, MtpsNodeCore childLink){
			Contract.Requires(!String.IsNullOrEmpty(searchName));
			Contract.Requires(parent != null);
			Contract.Requires(childLink != null);
			Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
			/*var xmlElement = GetTocXmlElement(childLink.SubTreeId.AssetId, childLink.SubTreeId.Version, childLink.SubTreeId.Locale);
			if (null == xmlElement)
				return Enumerable.Empty<MtpsNavigationNode>();*/
			var tocNode = ToNavigationNode(parent, childLink);
			return Search(searchName, tocNode);
		}

		private IEnumerable<MtpsNavigationNode> SearchWithinGroup(string searchName, MtpsNavigationNode parent, MtpsNodeCore childLink){
			Contract.Requires(!String.IsNullOrEmpty(searchName));
			Contract.Requires(parent != null);
			Contract.Requires(childLink != null);
			Contract.Ensures(Contract.Result<IEnumerable<MtpsNavigationNode>>() != null);
			Contract.Assume(null != childLink.SubTreeId);
			var xmlElement = GetTocXmlElement(childLink.SubTreeId.AssetId, childLink.SubTreeId.Version, childLink.SubTreeId.Locale);
			if (null == xmlElement)
				return Enumerable.Empty<MtpsNavigationNode>();
			var tocNode = ToNavigationNode(parent, xmlElement);
			return SearchChildren(searchName, parent, tocNode.ChildLinks);
		}

	}
}
