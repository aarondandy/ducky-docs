using System;
using System.Collections.Generic;

namespace SimpleApp1
{
	public class Generic1<TA, TB>
		where TA : class,new()
		where TB : IEnumerable<TA>
	{

		public class Inner<TC>
		{

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

	}
}
