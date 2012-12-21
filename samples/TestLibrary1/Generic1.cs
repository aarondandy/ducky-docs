using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary1
{
	public class Generic1<TA,TB>
		where TA:class,new()
		where TB:IEnumerable<TA>
	{

		public class Inner<TC>
		{
			/// <summary>
			/// Inner junk.
			/// </summary>
			/// <typeparam name="TY"></typeparam>
			/// <param name="crap"></param>
			/// <returns></returns>
			public static string Junk2<TY>(TY crap) {
				return String.Empty;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <typeparam name="TY"></typeparam>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="c"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public static string Junk3<TY>(TC a, TB b, TA c, TY y) {
				return String.Empty;
			}

			public int N;

			/// <summary>
			/// This is A thing.
			/// </summary>
			public TA A;
			public TB B;
			public TC C;
		}

		/// <summary>
		/// Some junk.
		/// </summary>
		/// <typeparam name="TX"></typeparam>
		/// <param name="garbage"></param>
		/// <returns></returns>
		public static string Junk1<TX>(TX garbage) {
			return String.Empty;
		}

		public int N;
		public TA A;
		public TB B;

	}
}
