using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DandyDoc.CRef;
using System.Diagnostics.Contracts;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public abstract class CodeDocEntityContentBase : ICodeDocEntityContent
	{

		protected CodeDocEntityContentBase(CRefIdentifier cRef){
			if(cRef == null) throw new ArgumentNullException("cRef");
			Contract.EndContractBlock();
			CRef = cRef;
		}

		public string Title { get; set; }

		public string SubTitle { get; set; }

		public string ShortName { get; set; }

		public string FullName { get; set; }

		public CRefIdentifier CRef { get; protected set; }

		public string NamespaceName { get; set; }

		public bool HasExamples { get { return Examples.Count > 0; } }

        public IList<XmlDocNode> Examples {
			get{
				Contract.Ensures(Contract.Result<IList<XmlNodeList>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				throw new NotImplementedException();
			}
		}

		public bool HasPermissions { get { return Permissions.Count > 0; } }

        public IList<XmlDocNode> Permissions {
			get{
				Contract.Ensures(Contract.Result<IList<XmlNodeList>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				throw new NotImplementedException();
			}
		}

		public bool HasRemarks { get { return Remarks.Count > 0; } }

        public IList<XmlDocNode> Remarks {
			get{
				Contract.Ensures(Contract.Result<IList<XmlNodeList>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				throw new NotImplementedException();
			}
		}

		public bool HasSeeAlso { get { return SeeAlso.Count > 0; } }

        public IList<XmlDocNode> SeeAlso {
			get{
				Contract.Ensures(Contract.Result<IList<XmlNodeList>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
				throw new NotImplementedException();
			}
		}

		public bool HasSummary { get { return Summary != null && Summary.HasChildren; } }

        public XmlDocNode Summary {
			get {
				throw new NotImplementedException();
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariants(){
			Contract.Invariant(CRef != null);
		}

	}
}
