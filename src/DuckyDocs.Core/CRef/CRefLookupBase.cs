﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DuckyDocs.CRef
{
    /// <summary>
    /// Base code reference (cref) lookup class.
    /// </summary>
    /// <typeparam name="TAssembly">The assembly type to use for searching.</typeparam>
    /// <typeparam name="TMember">The member type that is to be returned.</typeparam>
    public abstract class CRefLookupBase<TAssembly, TMember>
        where TMember : class
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
            AttemptSimplifiedResolution = true;
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
        /// Indicates that the lookup will make another attempt at resolution using a simplified code reference.
        /// </summary>
        public bool AttemptSimplifiedResolution { get; set; }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public virtual TMember GetMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetMember(new CRefIdentifier(cRef));
        }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public virtual TMember GetMember(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            var result = GetMemberCore(cRef);
            //if (result == null && AttemptSimplifiedResolution)
            //    result = GetMemberCore(CRefTransformer.FullSimplification.Transform(cRef));
            return result;
        }

        /// <summary>
        /// Locates a member based on a code reference.
        /// </summary>
        /// <param name="cRef">The code reference to search for.</param>
        /// <returns>The member if found.</returns>
        public abstract TMember GetMemberCore(CRefIdentifier cRef);

    }
}
