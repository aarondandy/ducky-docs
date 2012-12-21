using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary1
{
	/// <summary>
	/// This class is just for testing and has no real use outside of generating some documentation.
	/// </summary>
    public class Class1
    {

		// Note leave the constructor to be default so a default summary can be generated

		public static void BlankStatic() {
			throw new NotImplementedException();
		}

		public static int DoubleStatic(int n) {
			return n * 2;
		}

		/// <summary>
		/// Doubles the given value.
		/// </summary>
		/// <param name="n">The value to double.</param>
		/// <returns>The result of doubling the value.</returns>
		public static double DoubleStatic(double n) {
			return n * 2;
		}

    }
}
