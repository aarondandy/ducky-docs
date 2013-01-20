using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using System.Collections.Generic;

namespace DandyDoc.SimpleModels
{
	public class CrefSimpleMemberPointer : ISimpleMemberPointerModel
	{

		private static IComplexTextNode ToSingleNode(IList<IComplexTextNode> nodes){
			if (null == nodes || nodes.Count == 0)
				return new StandardComplexText(String.Empty);
			if (nodes.Count == 1)
				return nodes[0];

			return new ComplexTextList(nodes);
		}

		private static string StripCrefTypePrefix(string cRef) {
			Contract.Ensures(String.IsNullOrEmpty(cRef) ? String.IsNullOrEmpty(Contract.Result<string>()) : !String.IsNullOrEmpty(Contract.Result<string>()));
			if (String.IsNullOrEmpty(cRef))
				return cRef;
			if (cRef.Length >= 2 && cRef[1] == ':')
				return cRef.Substring(2);
			return cRef;
		}

		public CrefSimpleMemberPointer(string cRef)
			: this(cRef, StripCrefTypePrefix(cRef))
		{ }

		public CrefSimpleMemberPointer(string cRef, string description){
			Description = new StandardComplexText(String.IsNullOrEmpty(description)
				? StripCrefTypePrefix(cRef)
				: description);
			CRef = cRef ?? String.Empty;
		}

		public CrefSimpleMemberPointer(string cRef, IComplexTextNode description) {
			Description = description ?? new StandardComplexText(StripCrefTypePrefix(cRef));
			CRef = cRef ?? String.Empty;
		}

		public CrefSimpleMemberPointer(string cRef, IList<IComplexTextNode> descriptions)
			: this(cRef, ToSingleNode(descriptions)) { }

		public IComplexTextNode Description { get; private set; }

		public string CRef { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Description != null);
			Contract.Invariant(CRef != null);
		}

	}
}
