using System;
using Test.Annotations;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1.JetbrainsAnnotations
{
	public class Annotated
	{
		[StringFormatMethod("format")]
		public Annotated(string format, params object[] stuff) {
			throw new NotImplementedException();
		}

		[CanBeNull]
		public string Stuff([CanBeNull] string someValue) {
			return someValue;
		}

		[NotNull]
		public string NoNulls([NotNull] string someValue) {
			return someValue;
		}

		[NotNull]
		public string NoNullsOut([CanBeNull] string thing) {
			return "test";
		}

		[CanBeNull]
		public string NoNullsIn([NotNull] string thing) {
			return null;
		}

	}
}
