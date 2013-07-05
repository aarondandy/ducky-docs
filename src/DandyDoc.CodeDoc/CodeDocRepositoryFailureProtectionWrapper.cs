using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DandyDoc.CodeDoc
{

    /// <summary>
    /// Fails gracefully by return null or empty results where appropriate in the event of an exception.
    /// </summary>
    /// <remarks>
    /// Note that due to the nature of this repository wrapper requests may not produce consistent outputs
    /// and may polute the consitency of all related repositories. When using this wrapper be sure to use
    /// a low cache expiration time for any cached member models.
    /// </remarks>
    public class CodeDocRepositoryFailureProtectionWrapper : CodeDocRepositoryWrapperBase
    {

        /// <summary>
        /// Creates a new fail safe repository wrapper.
        /// </summary>
        /// <param name="repository">The repository to wrap.</param>
        /// <param name="deactivationTimeSpan">The time to deactivate the repository for in the event of a failure. Use <see cref="System.TimeSpan.Zero"/> to deactivate forever.</param>
        public CodeDocRepositoryFailureProtectionWrapper(ICodeDocMemberRepository repository, TimeSpan deactivationTimeSpan)
            : base(repository) {
            Contract.Requires(repository != null);
            DeactivationTimeSpan = deactivationTimeSpan;
            _active = true;
            _deactivatedTime = default(DateTime);
        }

        private DateTime _deactivatedTime;
        private volatile bool _active;

        /// <summary>
        /// The amount of time to deactivate the repository wrapper for when an exception is thrown.
        /// </summary>
        public TimeSpan DeactivationTimeSpan { get; set; }

        public override ICodeDocMember GetMemberModel(CRef.CRefIdentifier cRef, CodeDocRepositorySearchContext searchContext = null, CodeDocMemberDetailLevel detailLevel = CodeDocMemberDetailLevel.Full) {
            if (!ActivatedCheck())
                return null;

            try {
                return Repository.GetMemberModel(cRef, searchContext, detailLevel);
            }
            catch (Exception ex) {
                Deactivate();
                return null;
            }
        }

        public override IList<CodeDocSimpleAssembly> Assemblies {
            get {
                if (!ActivatedCheck())
                    return new CodeDocSimpleAssembly[0];

                try {
                    return Repository.Assemblies;
                }
                catch (Exception ex) {
                    Deactivate();
                    return new CodeDocSimpleAssembly[0];
                }
            }
        }

        public override IList<CodeDocSimpleNamespace> Namespaces {
            get {
                if (!ActivatedCheck())
                    return new CodeDocSimpleNamespace[0];

                try {
                    return Repository.Namespaces;
                }
                catch (Exception ex) {
                    Deactivate();
                    return new CodeDocSimpleNamespace[0];
                }
            }
        }

        private bool ActivatedCheck() {
            if (_active)
                return true;

            if (DeactivationTimeSpan != TimeSpan.Zero && (_deactivatedTime + DeactivationTimeSpan) <= DateTime.Now) {
                _active = true;
                _deactivatedTime = default(DateTime);
                return true;
            }

            return false;
        }

        private void Deactivate() {
            _active = false;
            _deactivatedTime = DateTime.Now;
        }

    }
}
