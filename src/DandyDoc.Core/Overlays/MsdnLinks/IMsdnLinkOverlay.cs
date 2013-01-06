using System.Collections.Generic;

namespace DandyDoc.Overlays.MsdnLinks
{
	public interface IMsdnLinkOverlay
	{

		string RootAssetId { get; }

		string Version { get; }

		string Locale { get; }

		IEnumerable<MtpsNavigationNode> Search(string memberName);

		string GetUrl(MtpsNavigationNode node);

	}
}
