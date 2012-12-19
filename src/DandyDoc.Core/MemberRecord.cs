using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;

namespace DandyDoc.Core
{
	public class MemberRecord : IDocumentableEntity
	{

		private Lazy<XmlNode> _memberDocNode;

		internal MemberRecord(TypeRecord parentType, MemberInfo memberInfo) {
			if(null == parentType) throw new ArgumentNullException("parentType");
			if(null == memberInfo) throw new ArgumentNullException("memberInfo");
			Contract.EndContractBlock();

			ParentType = parentType;
			CoreMemberInfo = memberInfo;
			_memberDocNode = new Lazy<XmlNode>(GetDocNode, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private XmlNode GetDocNode() {
			return ParentType.Parent.GetXmlNodeForMember(ParentType.CoreType, this);
		}

		public string Name { get { return CoreMemberInfo.Name; } }

		public TypeRecord ParentType { get; private set; }

		public MemberInfo CoreMemberInfo { get; private set; }

		public bool IsMethod { get { return CoreMemberInfo.MemberType == MemberTypes.Method; } }

		public bool IsConstructor { get { return CoreMemberInfo.MemberType == MemberTypes.Constructor; } }

		public bool IsField { get { return CoreMemberInfo.MemberType == MemberTypes.Field; } }

		public string Summary { get { return GetXmlDocText("summary"); } }

		public string Remarks { get { return GetXmlDocText("remarks"); } }

		public ParameterInfo[] ParameterInfos {
			get {
				var methodInfo = CoreMemberInfo as MethodInfo;
				if (null != methodInfo)
					return methodInfo.GetParameters();

				var constructorInfo = CoreMemberInfo as ConstructorInfo;
				if (null != constructorInfo)
					return constructorInfo.GetParameters();

				return null;
			}
		}

		public ParameterRecord[] Parameters {
			get {
				var paramInfos = ParameterInfos;
				if (null == paramInfos)
					return null;

				return Array.ConvertAll(paramInfos, x => new ParameterRecord(this, x));
			}
		}

		public string GetXmlDocText(string key) {
			var node = _memberDocNode.Value;
			if (null == node)
				return null;

			var targetNode = node.SelectSingleNode(key);
			if (null == targetNode)
				return null;

			return TextUtilities.ExtractIndentedNormalizedInnerText(targetNode.InnerText);
		}

		public IList<SeeAlsoReference> SeeAlso {
			get { throw new NotImplementedException(); }
		}
	}
}
