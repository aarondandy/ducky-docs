using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class DefinitionParameterSimpleModel : IParameterSimpleModel
	{

		protected static readonly IFlairTag DefaultCanBeNullTag = new SimpleFlairTag("null ok", "Null Values", "This parameter can be null.");
		protected static readonly IFlairTag DefaultParamNotNullAndNotEmptyTag = new SimpleFlairTag("no nulls", "Null Values", "Required: not null and not empty.");
		protected static readonly IFlairTag DefaultParamNotNullTag = new SimpleFlairTag("no nulls", "Null Values", "Required: not null.");
		protected static readonly IFlairTag DefaultInstantHandleTag = new SimpleFlairTag("instant", "Usage", "Parameter is used only during method execution.");

		public DefinitionParameterSimpleModel(ParameterDefinition parameter, IInvokableSimpleModel parent, ISimpleMemberPointerModel parameterType, IComplexTextNode summary) {
			if(parameter == null) throw new ArgumentNullException("parameter");
			if(parent == null) throw new ArgumentNullException("parent");
			if(parameterType == null) throw new ArgumentNullException("parameterType");
			Contract.EndContractBlock();
			Parameter = parameter;
			Parent = parent;
			Type = parameterType;
			Summary = summary;

		}

		public IInvokableSimpleModel Parent { get; private set; }

		protected ParameterDefinition Parameter { get; private set; }

		public string Name { get { return Parameter.Name; } }

		public IComplexTextNode DisplayName { get { return new StandardComplexText(Name); } }

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

				if (Parameter.HasAttributeMatchingName("CanBeNullAttribute")){
					tags.Add(DefaultCanBeNullTag);
				}
				else{
					var name = Parameter.Name;
					Contract.Assume(!String.IsNullOrEmpty(name));
					if (Parent.RequiresParameterNotNullOrEmpty(name))
						tags.Add(DefaultParamNotNullAndNotEmptyTag);
					else if (Parent.RequiresParameterNotNull(name))
						tags.Add(DefaultParamNotNullTag);
				}

				if (Parameter.HasAttributeMatchingName("InstantHandleAttribute"))
					tags.Add(DefaultInstantHandleTag);

				return tags;
			}
		}

		public bool HasAttributeMatchingName(string name){
			return Parameter.HasAttributeMatchingName(name);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(Parameter != null);
			Contract.Invariant(Parent != null);
			Contract.Invariant(Type != null);
		}
	}
}
