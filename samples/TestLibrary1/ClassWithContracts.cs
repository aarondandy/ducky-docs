using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public string Text { get; private set; }

		/// <summary>
		/// A pure method that ensures on return.
		/// </summary>
		/// <returns>stuff</returns>
		[Pure] public string SomeStuff(){
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return "stuff";
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(Text));
		}


	}
}
