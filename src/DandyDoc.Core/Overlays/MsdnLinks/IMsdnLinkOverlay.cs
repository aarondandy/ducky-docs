using System.Collections.Generic;

namespace DandyDoc.Overlays.MsdnLinks
{
	/// <summary>
	/// Generates links and locates content on MSDN.
	/// </summary>
	public interface IMsdnLinkOverlay
	{

		string RootAssetId { get; }

		string Version { get; }

		string Locale { get; }

		IEnumerable<MtpsNavigationNode> Search(string memberName);

		string GetUrl(MtpsNavigationNode node);

	}
}
