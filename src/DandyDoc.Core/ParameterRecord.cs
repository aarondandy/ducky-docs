using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace DandyDoc.Core
{
	public class ParameterRecord : IDocumentableEntity
	{

		internal ParameterRecord(MemberRecord parent, ParameterInfo parameterInfo) {
			if(null == parent) throw new ArgumentNullException("parent");
			if(null == parameterInfo) throw new ArgumentNullException("parameterInfo");
			Contract.EndContractBlock();
			CoreParameterInfo = parameterInfo;
			ParentEntity = parent;
		}

		public ParameterInfo CoreParameterInfo { get; private set; }

		public MemberRecord ParentEntity { get; private set; }

		public string Summary {
			get { return ParentEntity.GetXmlDocText(String.Format("param[@name=\"{0}\"]",CoreParameterInfo.Name)); }
		}

		public string Remarks {
			get { return null; }
		}

		public System.Collections.Generic.IList<SeeAlsoReference> SeeAlso {
			get { return null; }
		}
	}
}
