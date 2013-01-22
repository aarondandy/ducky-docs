using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DandyDoc.SimpleModels.Contracts
{

	[ContractClass(typeof(CodeContractISimpleModel))]
	public interface ISimpleModel
	{

		string Title { get; }

		string SubTitle { get; }

		string ShortName { get; }

		string FullName { get; }

		string CRef { get; }

		string NamespaceName { get; }

		ISimpleModel DeclaringModel { get; }

		IAssemblySimpleModel ContainingAssembly { get; }

		ISimpleModelRepository RootRepository { get; }

		bool HasFlair { get; }

		IList<IFlairTag> FlairTags { get; }

		bool HasSummary { get; }

		IComplexTextNode Summary { get; }

		bool HasRemarks { get; }

		IList<IComplexTextNode> Remarks { get; }

		bool HasExamples { get; }

		IList<IComplexTextNode> Examples { get; }

		bool HasSeeAlso { get; }

		IList<IComplexTextNode> SeeAlso { get; }

	}

	[ContractClassFor(typeof(ISimpleModel))]
	internal abstract class CodeContractISimpleModel : ISimpleModel
	{

		public string Title {
			get{
				throw new NotImplementedException();
			}
		}

		public string SubTitle {
			get {
				throw new NotImplementedException();
			}
		}

		public string ShortName {
			get{
				throw new NotImplementedException();
			}
		}

		public string FullName {
			get{
				throw new NotImplementedException();
			}
		}

		public string CRef {
			get{
				throw new NotImplementedException();
			}
		}

		public string NamespaceName {
			get{
				throw new NotImplementedException();
			}
		}

		public ISimpleModel DeclaringModel {
			get { throw new NotImplementedException(); }
		}

		public abstract IAssemblySimpleModel ContainingAssembly { get; }

		public ISimpleModelRepository RootRepository {
			get{
				throw new NotImplementedException();
			}
		}

		public bool HasFlair {
			get { throw new NotImplementedException(); }
		}

		public IList<IFlairTag> FlairTags {
			get{
				throw new NotImplementedException();
			}
		}

		public bool HasSummary {
			get { throw new NotImplementedException(); }
		}

		public IComplexTextNode Summary {
			get { throw new NotImplementedException(); }
		}

		public bool HasRemarks {
			get { throw new NotImplementedException(); }
		}

		public IList<IComplexTextNode> Remarks {
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				throw new NotImplementedException();
			}
		}

		public bool HasExamples {
			get { throw new NotImplementedException(); }
		}

		public IList<IComplexTextNode> Examples {
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				throw new NotImplementedException();
			}
		}

		public bool HasSeeAlso {
			get { throw new NotImplementedException(); }
		}

		public IList<IComplexTextNode> SeeAlso {
			get{
				Contract.Ensures(Contract.Result<IList<IComplexTextNode>>() != null);
				throw new NotImplementedException();
			}
		}
	}

}
