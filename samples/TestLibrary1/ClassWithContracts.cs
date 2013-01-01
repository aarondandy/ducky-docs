using System;
using System.Diagnostics.Contracts;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
	/// <summary>
	/// A class uses to test code contracts.
	/// </summary>
	public class ClassWithContracts
	{

		/// <summary>
		/// Constructor showing a legacy requires.
		/// </summary>
		/// <param name="text"></param>
		public ClassWithContracts(string text){
			if(String.IsNullOrEmpty(text)) throw new ArgumentException("Nope!","text");
			Contract.EndContractBlock();
			Text = text;
		}

		/// <summary>
		/// Auto-property with an invariant.
		/// </summary>
		[Pure] public string Text { get; private set; }

		/// <summary>
		/// A pure method that ensures on return.
		/// </summary>
		/// <returns>stuff</returns>
		[Pure] public string SomeStuff(){
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return "stuff";
		}

		public string Stuff {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return "stuff";
			}
		}

		public string SameStuff{
			[Pure] get{
				Contract.Ensures(null != Contract.Result<string>());
				return "stuff";
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(Text));
		}

		


	}
}
