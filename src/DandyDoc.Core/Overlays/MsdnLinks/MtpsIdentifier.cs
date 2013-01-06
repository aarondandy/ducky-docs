using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.Overlays.MsdnLinks
{
	public class MtpsIdentifier
	{

		internal const string AssetIdPrefix = "AssetId:";

		public static string AppendAssetIdPrefixIfRequired(string assetId) {
			if (String.IsNullOrEmpty(assetId)) throw new ArgumentException();
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return assetId.StartsWith(AssetIdPrefix) ? assetId : String.Concat(AssetIdPrefix, assetId);
		}

		public static string RemoveAssetIdPrefixIfFound(string assetId) {
			if (String.IsNullOrEmpty(assetId)) throw new ArgumentException();
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return assetId.StartsWith(AssetIdPrefix) ? assetId.Substring(AssetIdPrefix.Length) : assetId;
		}

		public MtpsIdentifier(string assetId, string version, string locale){
			if (String.IsNullOrEmpty(assetId)) throw new ArgumentException();
			if (String.IsNullOrEmpty(version)) throw new ArgumentException();
			if (String.IsNullOrEmpty(locale)) throw new ArgumentException();
			Contract.EndContractBlock();
			AssetId = assetId;
			Version = version;
			Locale = locale;
		}

		public string AssetId { get; private set; }

		public string Version { get; private set; }

		public string Locale { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(AssetId));
			Contract.Invariant(!String.IsNullOrEmpty(Version));
			Contract.Invariant(!String.IsNullOrEmpty(Locale));
		}

	}
}
