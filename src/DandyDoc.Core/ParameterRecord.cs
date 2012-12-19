using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class ParameterRecord : IDocumentableEntity
	{

		internal ParameterRecord(MemberRecord parent, ParameterDefinition parameterInfo) {
			if(null == parent) throw new ArgumentNullException("parent");
			if(null == parameterInfo) throw new ArgumentNullException("parameterInfo");
			Contract.EndContractBlock();
			CoreParameterInfo = parameterInfo;
			ParentEntity = parent;
		}

		public ParameterDefinition CoreParameterInfo { get; private set; }

		public MemberRecord ParentEntity { get; private set; }

		public ParsedXmlDoc Summary {
			get { return new ParsedXmlDoc(ParentEntity.GetXmlDocText(String.Format("param[@name=\"{0}\"]", CoreParameterInfo.Name)), this); }
		}

		public ParsedXmlDoc Remarks {
			get { return null; }
		}

		public System.Collections.Generic.IList<SeeAlsoReference> SeeAlso {
			get { return null; }
		}

		public string FullTypeName { get { return CoreParameterInfo.ParameterType.FullName; } }
	}
}
