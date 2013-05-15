using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.CRef
{
    /// <summary>
    /// Base code reference (cref) lookup class.
    /// </summary>
    /// <typeparam name="TAssembly">The assembly type to use for searching.</typeparam>
    /// <typeparam name="TMember">The member type that is to be returned.</typeparam>
    public abstract class CRefLookupBase<TAssembly, TMember>
    {

        private readonly ReadOnlyCollection<TAssembly> _assemblies;

        /// <summary>
        /// Base constructor for a code reference lookup class.
        /// </summary>
        /// <param name="assemblies">The assemblies that are to be searched.</param>
        protected CRefLookupBase(IEnumerable<TAssembly> assemblies) {
            if (assemblies == null) throw new ArgumentNullException("assemblies");
            Contract.EndContractBlock();
            _assemblies = new ReadOnlyCollection<TAssembly>(assemblies.ToArray());
        }

        /// <summary>
        /// The assemblies that are to be searched.
        /// </summary>
        public ReadOnlyCollection<TAssembly> Assemblies {
            get {
                Contract.Ensures(Contract.Result<IList<TAssembly>>() != null);
                return _assemblies;
            }
        }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public abstract TMember GetMember(string cRef);

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public abstract TMember GetMember(CRefIdentifier cRef);

    }
}
