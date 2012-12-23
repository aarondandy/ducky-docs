using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary1
{
	public class Generic1<TA,TB>
		where TA:struct
		where TB:IEnumerable<TA>
	{

		/// <summary>
		/// An operator
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Generic1<TA, TB> operator +(Generic1<int, int[]> a, Generic1<TA, TB> b){
			throw new NotImplementedException();
		}

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
			/// in back
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

			/// <summary>
			/// in front
			/// </summary>
			/// <typeparam name="TY"></typeparam>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="c"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			public static string Junk3<TY>(TY y, TC a, TB b, TA c) {
				return String.Empty;
			}

			/// <summary>
			/// No method generic parameters.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="c"></param>
			/// <returns></returns>
			public static string Junk4(TC a, TB b, TA c){
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

		/// <summary>
		/// N
		/// </summary>
		public int N { get; set; }
		/// <summary>
		/// A
		/// </summary>
		public TA A { get; set; }
		public TB B;

		/// <summary>
		/// A func with generics from the parent class
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public delegate TB MyFunc(TA a);

		/// <summary>
		/// A func with its own generic
		/// </summary>
		/// <typeparam name="TX"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public delegate TX MyFunc<TX>(TX a);

		/// <summary>
		/// an event
		/// </summary>
		public event MyFunc<TB> E;

	}

	public class NotGeneric
	{
		
		/// <summary>
		/// A method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		public bool IsGeneric<T>(T a){
			throw new NotImplementedException();
		}
	}
}
