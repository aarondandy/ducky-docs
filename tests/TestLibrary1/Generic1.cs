using System;
using System.Collections.Generic;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
	/// <summary>
	/// See <typeparamref name="TA"/> for details.
	/// </summary>
	/// <typeparam name="TA">A</typeparam>
	/// <typeparam name="TB">B</typeparam>
	public class Generic1<TA,TB>
		where TA:struct
		where TB:IEnumerable<TA>
	{

		public interface Variance<in TIn, out TOut>
		{
			TOut Get();

			void Set(TIn a);
		}

		public class Constraints<TConstraints>
			where TConstraints: IEnumerable<int>, IDisposable, new()
		{
			/// <summary>
			/// 
			/// </summary>
			/// <typeparam name="TStuff">some stuff</typeparam>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public TStuff GetStuff<TStuff>(TConstraints a, TStuff b)
				where TStuff : IConvertible
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// An operator. See <typeparamref name="TA"/> for details.
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
		/// A func with its own generic. See <typeparamref name="TX"/> for details.
		/// </summary>
		/// <typeparam name="TX">some type</typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public delegate TX MyFunc<TX>(TX a);

		/// <summary>
		/// an event
		/// </summary>
		public event MyFunc<TB> E;

		/// <summary>
		/// A crazy constructor.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="stuff"></param>
		/// <param name="text"></param>
		public Generic1(TA a, TB b, IEnumerable<TA> stuff,  string text){
			throw new NotImplementedException();
		}

		public TB AMix<TOther>(TA a, TOther other){
			throw new NotImplementedException();
		}

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
