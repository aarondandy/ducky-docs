using System;
using System.Text;

namespace DandyDoc.Overlays.MsdnLinks
{
	public class MtpsNodeCore
	{

		private static readonly string[] NamespaceEnding = new []{" Namespace"};
		private static readonly string[] TypeEndings = new[]{" Class", " Interface", " Structure", "Enumeration", " Delegate"};
		private static readonly string[] GroupEndings = new[]{"Namespaces", "Fields", "Properties", "Methods", "Members", "Constructors", "Events", "Overload"};

		public MtpsNodeCore(MtpsIdentifier subTreeId, MtpsIdentifier targetId, string title, MtpsNodeCore parent, bool phantom = false){
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
			get { return GetNamespace() != null; }
		}

		public string GetNamespace(){
			if (null != TargetId){
				var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
				if (assetPrefixRemoved.StartsWith("N:")) {
					return assetPrefixRemoved.Substring(2);
				}
			}
			if (null != Title) {
				foreach (var namespaceEnding in NamespaceEnding){
					if (Title.EndsWith(namespaceEnding)){
						return Title.Substring(0, Title.Length - namespaceEnding.Length);
					}
				}
			}
			return null;
		}

		public bool IsNodeGroup{
			get{
				if (null != TargetId) {
					var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
					foreach (var groupEnding in GroupEndings){
						if (assetPrefixRemoved.StartsWith(groupEnding + ':')){
							return true;
						}
					}
				}
				if (null != Title){
					foreach (var groupEnding in GroupEndings){
						if (Title.EndsWith(' ' + groupEnding)){
							return true;
						}
					}
				}
				return false;
			}
		}

		public string GetNamespaceGroupName(){
			const string namespacesEnding = " Namespaces";
			if (null != Title){
				if (Title.EndsWith(namespacesEnding))
					return Title.Substring(0, Title.Length - namespacesEnding.Length);
			}
			return null;
		}

		public string GetOverloadGroupName(){
			const string overloadPrefix = "Overload:";
			if (null != TargetId) {
				var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
				if (assetPrefixRemoved.StartsWith(overloadPrefix)){
					return assetPrefixRemoved.Substring(overloadPrefix.Length);
				}
			}
			return null;
		}

		public bool IsTypeOrMember{
			get{
				var name = ExtractFullTypeOrMemberName();
				if (!String.IsNullOrEmpty(name))
					return true;
				name = ExtractPartName();
				if (!String.IsNullOrEmpty(name))
					return true;
				return false;
			}
		}

		private string ExtractPartName(){
			if (null != Title) {
				foreach (var typeEnding in TypeEndings) {
					if (Title.EndsWith(typeEnding)) {
						return Title.Substring(0, Title.Length - typeEnding.Length);
					}
				}
			}
			return null;
		}

		private string ExtractFullTypeOrMemberName(){
			if (null != TargetId) {
				var assetPrefixRemoved = MtpsIdentifier.RemoveAssetIdPrefixIfFound(TargetId.AssetId);
				if(assetPrefixRemoved.Length > 2 && assetPrefixRemoved[1] == ':'){
					return assetPrefixRemoved.Substring(2);
				}
			}
			return null;
		}

		public string GetFullName(){
			if (IsTypeOrMember) {
				var name = ExtractFullTypeOrMemberName();
				if (!String.IsNullOrEmpty(name))
					return name;
				name = ExtractPartName();
				if (null != Parent && Parent.IsNamespace){
					var ns = Parent.GetNamespace();
					if (!String.IsNullOrEmpty(ns)){
						name = String.Concat(ns, '.', name);
					}
				}
				return name;
			}
			if (IsNamespace){
				return GetNamespace();
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

		public override string ToString(){
			return Title;
		}
	}
}
