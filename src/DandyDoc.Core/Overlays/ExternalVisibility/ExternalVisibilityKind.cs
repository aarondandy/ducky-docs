using System;

namespace DandyDoc.Overlays.ExternalVisibility
{
	/// <summary>
	/// Specifies the level of external visibility for a member.
	/// </summary>
	/// <seealso cref="DandyDoc.Overlays.ExternalVisibility.ExternalVisibilityOverlay"/>
	[Obsolete]
	public enum ExternalVisibilityKind
	{
		/// <summary>
		/// The member is hidden from external view.
		/// </summary>
		Hidden = 0,
		/// <summary>
		/// The member is externally visible only through inheritance.
		/// </summary>
		Protected = 1,
		/// <summary>
		/// The member is externally visible.
		/// </summary>
		Public = 3,
	}
}
