using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class ReturnSimpleModel : IParameterSimpleModel
	{

		protected static readonly IFlairTag DefaultCanReturnNullTag = new SimpleFlairTag("null result", "Null Values", "Can return null.");
		protected static readonly IFlairTag DefaultEnsureResultNotNullTag = new SimpleFlairTag("no nulls", "Null Values", "Ensures: result is not null.");
		protected static readonly IFlairTag DefaultEnsureResultNotNullEndNotEmptyTag = new SimpleFlairTag("no nulls", "Null Values", "Ensures: result is not null and not empty.");

		public ReturnSimpleModel(ISimpleMemberPointerModel parameterType, IInvokableSimpleModel parent, IComplexTextNode summary) {
			if(parameterType == null) throw new ArgumentNullException("parameterType");
			if(parent == null) throw new ArgumentNullException("parent");
			Contract.EndContractBlock();
			Type = parameterType;
			Parent = parent;
			Summary = summary;

		}

		public IInvokableSimpleModel Parent { get; private set; }

		public string Name { get { return Type.CRef; } }

		public IComplexTextNode DisplayName {
			get { return Type.Description; }
		}

		public bool HasSummary {
			get { return Summary != null; }
		}

		public IComplexTextNode Summary { get; private set; }

		public ISimpleMemberPointerModel Type { get; private set; }

		public bool HasFlair {
			get { return Flair.Count > 0; }
		}

		public IList<IFlairTag> Flair {
			get{
				var tags = new List<IFlairTag>();
				if (Parent.CanReturnNull){
					tags.Add(DefaultCanReturnNullTag);
				}
				else{

					if (Parent.EnsuresResultNotNullOrEmpty)
						tags.Add(DefaultEnsureResultNotNullEndNotEmptyTag);
					else if (Parent.EnsuresResultNotNull)
						tags.Add(DefaultEnsureResultNotNullTag);
				}
				return tags;
			}
		}

		public bool HasAttributeMatchingName(string name){
			return false;
		}
	}
}
