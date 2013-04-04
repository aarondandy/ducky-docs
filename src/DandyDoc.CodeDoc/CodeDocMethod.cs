using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMethod : CodeDocEntityContentBase, ICodeDocMethod
    {

        public CodeDocMethod(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        public IList<ICodeDocParameter> Parameters { get; set; }

        public bool HasReturn { get { return Return != null; } }

        public ICodeDocParameter Return { get; set; }

        public bool HasExceptions { get { return Exceptions != null && Exceptions.Count > 0; } }

        public IList<ICodeDocException> Exceptions { get; set; }

        public bool HasEnsures { get { return Ensures != null & Ensures.Count > 0; } }

        public IList<ICodeDocContractCondition> Ensures { get; set; }

        public bool HasRequires { get { return Requires != null && Requires.Count > 0; } }

        public IList<ICodeDocContractCondition> Requires { get; set; }

        public bool HasGenericParameters { get { return GenericParameters != null && GenericParameters.Count > 0; } }

        public IList<ICodeDocGenericParameter> GenericParameters { get; set; }

    }
}
