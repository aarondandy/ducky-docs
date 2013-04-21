using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class CodeDocDelegate : CodeDocType, ICodeDocDelegate
    {

        public CodeDocDelegate(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        public bool HasParameters { get { return Parameters != null && Parameters.Count > 0; } }

        public IList<ICodeDocParameter> Parameters { get; set; }

        public bool HasReturn { get { return Return != null; } }

        public ICodeDocParameter Return { get; set; }

        public bool HasExceptions { get { return Exceptions != null && Exceptions.Count > 0; } }

        public IList<ICodeDocException> Exceptions { get; set; }

        public bool HasEnsures { get { return Ensures != null && Ensures.Count > 0; } }

        public IList<XmlDocContractElement> Ensures { get; set; }

        public bool HasNormalTerminationEnsures {
            get {
                return HasEnsures
                    && NormalTerminationEnsures.Any();
            }
        }

        public IEnumerable<XmlDocContractElement> NormalTerminationEnsures {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<XmlDocContractElement>>() != null);
                return HasEnsures
                    ? Ensures.Where(x => "ENSURES".Equals(x.Name, StringComparison.OrdinalIgnoreCase))
                    : Enumerable.Empty<XmlDocContractElement>();
            }
        }

        public bool HasRequires { get { return Requires != null && Requires.Count > 0; } }

        public IList<XmlDocContractElement> Requires { get; set; }
    }
}
