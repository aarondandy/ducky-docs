﻿using System;
using System.Diagnostics.Contracts;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
	/// <summary>
	/// This class is just for testing and has no real use outside of generating some documentation.
	/// </summary>
	/// <remarks>
	/// These are some remarks.
	/// </remarks>
	/// <example>
	/// <code>
	/// Example 1
	/// </code>
	/// </example>
	/// <example>
	/// <code>
	/// Example 2
	/// </code>
	/// </example>
	public class Class1
	{

        public static implicit operator string(Class1 input){
            Contract.Ensures(Contract.Result<string>() == String.Empty);
            return String.Empty;
        }

        public static implicit operator int(Class1 input) {
            Contract.Ensures(Contract.Result<int>() == 0);
            return 0;
        }

        public static explicit operator Class1(string input){
            Contract.Ensures(Contract.Result<Class1>() == null);
            return null;
        }

		
		/// <inheritdoc/>
		public class Inherits : Class1
		{
			public Inherits() : base(String.Empty) { }

			public void SomeNewMethod() {
				var result = new Inherits() + new Inherits();
				var a = result.GetHashCode() * 7;
			}

			public void VirtualInstanceMethod(int n) {
				throw new NotImplementedException();
			}

			new public void VirtualInstanceMethod(string n) {
				throw new NotImplementedException();
			}

		}

		/// <remarks>
		/// This is just some class.
		/// </remarks>
		[Obsolete]
		public class Inner
		{

			/// <summary>
			/// A name.
			/// </summary>
			[Obsolete]
			public string Name { get; set; }

			[Obsolete]
			public string GetAName(){ throw new NotImplementedException();}

		}

		public Class1[] SomeClasses;

		public Int32? SomeNullableInt;

		/// <exception cref="System.NotImplementedException">Too lazy to implement.</exception>
		public void HasStrangeParams(Int32? a, Class1[] someClass) {
			throw new NotImplementedException();
		}

		public class NoDocs
		{
			
		}

		/// <summary>
		/// no remarks here
		/// </summary>
		public class NoRemarks : NoDocs
		{
			
		}

		public interface IThing
		{
            string DoIt();
		}

		protected struct ProtectedStruct : IThing
		{
		    string IThing.DoIt() {
                throw new NotImplementedException();
            }
        }

		/// <summary>
		/// The static constructor.
		/// </summary>
		static Class1() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// The instance constructor.
		/// </summary>
		/// <remarks>
		/// A remark.
		/// </remarks>
		/// <param name="crap">Whatever.</param>
		public Class1(string crap) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// This is another instance constructor.
		/// </summary>
		/// <param name="crap">Crap param.</param>
		/// <param name="dookie">Dookie param.</param>
		/// <remarks>
		/// <paramref name="crap"/> is a parameter.
		/// </remarks>
		protected Class1(string crap, string dookie) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// This does nothing at all.
		/// </summary>
		/// <param name="a">The left hand parameter.</param>
		/// <param name="b">The right hand parameter.</param>
		/// <returns>Nope!</returns>
		/// <exception cref="System.NotImplementedException">This is not implemented.</exception>
		/// <exception cref="System.NotImplementedException">This is a duplicate.</exception>
		public static Class1 operator +(Class1 a, Class1 b) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Just your average indexer.
		/// </summary>
		/// <param name="n">an index</param>
		/// <returns>a number</returns>
		/// <value>Some number.</value>
		/// <remarks>The <paramref name="n">index</paramref> to read from.</remarks>
		public int this[int n] {
			get {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// just a const
		/// </summary>
		/// <value>
		/// 1
		/// </value>
		/// <permission cref="System.Security.PermissionSet">I have no idea what this is for.</permission>
		public const int MyConst = 1;

		/// <summary>
		/// My delegate.
		/// </summary>
		/// <param name="a">param a</param>
		/// <param name="b">param b</param>
		/// <returns>some int</returns>
		/// <remarks>
		/// Parameter <paramref name="a"/> is an int.
		/// </remarks>
		/// <exception cref="System.Exception">This could throw an exception maybe.</exception>
		public delegate int MyFunc(int a, int b);

		/// <summary>
		/// My event!
		/// </summary>
		/// <seealso cref="TestLibrary1.Class1.MyFunc">The delegate.</seealso>
		/// <seealso cref="TestLibrary1.Class1.DoubleStatic(System.Int32)"/>
		/// <remarks>
		/// <see cref="TestLibrary1.Class1">stuff</see>
		/// </remarks>
		public static event MyFunc DoStuff;

		protected event MyFunc DoStuffInstance;

		// Note leave the constructor to be default so a default summary can be generated

		/// <summary>
		/// blank
		/// </summary>
		public static void BlankStatic() {
			DoStuff(1, 2);
			throw new NotImplementedException();
		}

		/// <summary>
		/// <list type="bullet">
		/// <listheader>
		/// <term>Col 1</term>
		/// <description>Col 2</description>
		/// </listheader>
		/// <item>
		/// <term>A term.</term>
		/// <description>A description.</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		/// <remarks>
		/// <code>
		/// This
		///  is
		///   some
		///    text.
		/// </code>
		/// </remarks>
		/// <example>Example 1</example>
		/// <example>Example 2</example>
		public static int DoubleStatic(int n) {
			return n * 2;
		}

		/// <summary>
		/// Doubles the given value like so: <c>result = value + value</c>.
		/// </summary>
		/// <param name="n">The value to double.</param>
		/// <returns>The result of doubling the value.</returns>
		public static double DoubleStatic(double n) {
			return n * 2;
		}

		/// <summary>Some property with a double value.</summary>
		/// <value>A double value.</value>
		public static double SomeProperty { get; set; }

		/// <summary>
		/// A field.
		/// </summary>
		/// <value>A double value.</value>
		public static double SomeField;

		protected readonly int ReadonlyField;

		/// <summary>
		/// This has a table in the remarks section.
		/// </summary>
		/// <remarks>
		/// <list type="table">
		/// <listheader>
		/// <term>Col 1</term>
		/// <description>Col 2</description>
		/// </listheader>
		/// <item>
		/// <term>A term.</term>
		/// <description>A description.</description>
		/// </item>
		/// </list>
		/// </remarks>
		public bool HasTableInRemarks { private get; set; }

		public virtual string VirtualInstanceMethod(string stuff) {
			throw new NotImplementedException();
		}

        /// <summary>
        /// This method has some ref/out parameters/
        /// </summary>
        /// <param name="a">An 'out' parameter.</param>
        /// <param name="b">A 'ref' parameter.</param>
        /// <returns><code>true</code></returns>
        /// <remarks>
        /// <para>
        /// P1
        /// </para>
        /// <para>
        /// P2
        /// </para>
        /// </remarks>
        public bool TrySomeOutRefStuff(out int a, ref int b) {
            a = 2 + b;
            b = 1;
            return true;
        }

		/// <summary>
		/// A finalizer that does nothing.
		/// </summary>
		/// <remarks>
		/// <para>
		/// a paragraph
		/// </para>
		/// <para>
		/// and another
		/// </para>
		/// <para>
		/// a third
		/// </para>
		/// </remarks>
		~Class1() {
			throw new NotImplementedException();
		}

    }
}
