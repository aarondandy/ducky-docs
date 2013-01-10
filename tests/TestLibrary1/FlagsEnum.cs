using System;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
	/// <summary>
	/// An enumeration to check detection of the flags attribute.
	/// </summary>
	/// <example>
	/// <code>
	/// FlagsEnum.AB == FlagsEnum.A | FlagsEnum.B;
	/// </code>
	/// </example>
	[Flags]
	public enum FlagsEnum : byte
	{
		/// <summary>
		/// Nothing.
		/// </summary>
		Nothing = 0,
		/// <summary>
		/// An enumeration value for A.
		/// </summary>
		A = 1,
		/// <summary>
		///  An enumeration value for B.
		/// </summary>
		B = 2,
		/// <summary>
		/// An enumeration value for both A and B.
		/// </summary>
		AB = A | B

	}
}
