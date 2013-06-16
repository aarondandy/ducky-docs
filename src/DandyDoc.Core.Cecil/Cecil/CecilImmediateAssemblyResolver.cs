using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Cecil
{
    /// <summary>
    /// An assembly resolver designed to load all meta-data ahead of time using
    /// <see cref="Mono.Cecil.ReadingMode.Immediate" />.
    /// </summary>
    /// <remarks>
    /// This resolver is useful in situations that require thread safety
    /// when using a version of Mono Cecil that is not thread safe.
    /// Be warned however that this will cause Cecil to be very slow
    /// and to consume more resources.
    /// </remarks>
    /// <seealso cref="T:DandyDoc.CodeDoc.ThreadSafeCodeDocRepositoryWrapper"/>
    public class CecilImmediateAssemblyResolver : IAssemblyResolver
    {

        /// <summary>
        /// A default instance of the immediate assembly resolver.
        /// </summary>
        public static readonly CecilImmediateAssemblyResolver Default
            = new CecilImmediateAssemblyResolver();

        /// <summary>
        /// Constructs an immediate assembly resolver.
        /// </summary>
        protected CecilImmediateAssemblyResolver() {
            Core = new DefaultAssemblyResolver();
            ImmediateParameters = new ReaderParameters(ReadingMode.Immediate) {
                AssemblyResolver = this
            };
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
            Contract.Invariant(ImmediateParameters != null);
        }

        /// <summary>
        /// The core assembly resolver that is used.
        /// </summary>
        protected IAssemblyResolver Core { get; private set; }

        /// <summary>
        /// The reader parameters that trigger immediate loading.
        /// </summary>
        protected ReaderParameters ImmediateParameters { get; private set; }

        /// <inheritdoc/>
        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters) {
            return Core.Resolve(fullName, parameters);
        }

        /// <inheritdoc/>
        public AssemblyDefinition Resolve(string fullName) {
            return Core.Resolve(fullName, ImmediateParameters);
        }

        /// <inheritdoc/>
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
            return Core.Resolve(name, parameters);
        }

        /// <inheritdoc/>
        public AssemblyDefinition Resolve(AssemblyNameReference name) {
            return Core.Resolve(name, ImmediateParameters);
        }

    }
}
