using System;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1.Seal
{

	public interface ISeal
	{
		int Stuff(int a);
	}

	public class NotSealed : ISeal
	{

		public int Stuff(int a) {
			throw new NotImplementedException();
		}
	}

	public sealed class SealedClass : ISeal
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

	public abstract class KickSealingCan : BaseClassToSeal
	{
		public abstract override int SealMe(int a);

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
