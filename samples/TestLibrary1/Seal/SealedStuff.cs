using System;

namespace TestLibrary1.Seal
{
	public sealed class SealedClass
	{
		public int Stuff(int a) {
			throw new NotImplementedException();
		}

		public string Prop { get; set; }
	}

	public abstract class BaseClassToSeal
	{
		public abstract int SealMe(int a);

		public abstract string Prop { get; set; }
	}

	public class KickSealingCan : BaseClassToSeal
	{
		public override int SealMe(int a) {
			throw new NotImplementedException();
		}

		public override string Prop {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
	}

	public class SealIt : KickSealingCan
	{
		public sealed override int SealMe(int a) {
			throw new NotImplementedException();
		}

		public sealed override string Prop {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

	}

	public class SealSomeOfIt : KickSealingCan
	{
		public sealed override int SealMe(int a) {
			throw new NotImplementedException();
		}

		public sealed override string Prop {
			get {
				throw new NotImplementedException();
			}
		}

	}

	public struct SomeStruct
	{

		int Method() {
			throw new NotImplementedException();
		}

		public string Prop {
			get {throw new NotImplementedException();}
			set { throw new NotImplementedException();}
		}
	}
}
