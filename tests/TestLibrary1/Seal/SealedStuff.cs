using System;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1.Seal
{

    public interface ISeal
    {
        /// <summary>
        /// The summary from ISeal.
        /// </summary>
        /// <param name="a">A parameter described by ISeal.</param>
        /// <returns>A result as described by ISeal.</returns>
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
        /// <summary>
        /// From BaseClassToSeal.
        /// </summary>
        /// <param name="a">From BaseClassToSeal.</param>
        /// <returns>From BaseClassToSeal.</returns>
        public abstract int SealMe(int a);

        /// <summary>
        /// From BaseClassToSeal.
        /// </summary>
        public abstract string Prop { get; set; }
    }

    /// <summary>
    /// This class is between the top and bottom, just to make the problem a little harder to solve.
    /// </summary>
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
        /// <summary>
        /// From SealIt.
        /// </summary>
        /// <param name="a">From SealIt.</param>
        /// <returns>From SealIt.</returns>
        public sealed override int SealMe(int a) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// From SealIt.
        /// </summary>
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
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
