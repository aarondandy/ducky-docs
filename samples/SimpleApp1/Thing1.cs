using System;
using System.Diagnostics.Contracts;


namespace SimpleApp1
{
	/// <summary>
	/// This is just a thing. Here is some garbage: &amp; &lt; . See <see cref="SimpleApp1.Thing2">Thing #2</see> for another thing.
	/// </summary>
	/// <remarks>
	///     This
	///    is
	///   a
	///  spacing
	/// sample!
	/// </remarks>
	/// <seealso cref="SimpleApp1.Thing2">The other thing.</seealso>
	/// <seealso cref="SimpleApp1.Thing2.DoNothing"/>
	[Pure] public class Thing1
	{

		/// <summary>
		/// This does nothing. <see cref="SimpleApp1.Thing1"><c>This is <b >Shtuff</b ></c></see>.
		/// </summary>
		public static void DoNothing() { }

		/// <summary>
		/// This does something.
		/// </summary>
		/// <param name="n">Just some number.</param>
		/// <returns>Some other number that is equal only when n is not finite or zero.</returns>
		[Pure] public static int DoSomething(int n) {
			if(n < 0) throw new ArgumentOutOfRangeException("n", "n must be 0 or greater.");
			Contract.Ensures(Contract.Result<int>() >= 0);
			return n * 2;
		}

	}
}
