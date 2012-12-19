using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DandyDoc.Core
{
	public class MemberRecord : IDocumentableEntity
	{

		private Lazy<XmlNode> _memberDocNode;

		internal MemberRecord(TypeRecord parentType, IMemberDefinition memberInfo) {
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

		public IMemberDefinition CoreMemberInfo { get; private set; }

		public bool IsMethod { get { return CoreMemberInfo is MethodDefinition && !((MethodDefinition)CoreMemberInfo).IsConstructor; } }

		public bool IsConstructor { get { return CoreMemberInfo is MethodDefinition && ((MethodDefinition)CoreMemberInfo).IsConstructor; } }

		public bool IsField { get { return CoreMemberInfo is FieldDefinition; } }

		public ParsedXmlDoc Summary { get { return new ParsedXmlDoc(GetXmlDocText("summary"),this); } }

		public ParsedXmlDoc Remarks { get { return new ParsedXmlDoc(GetXmlDocText("remarks"),this); } }

		private Collection<ParameterDefinition> ParameterInfos {
			get {
				var methodDefinition = CoreMemberInfo as MethodDefinition;
				if (null != methodDefinition)
					return methodDefinition.Parameters;

				return null;
			}
		}

		public IList<ParameterRecord> Parameters {
			get {
				var paramInfos = ParameterInfos;
				if (null == paramInfos)
					return null;

				return ParameterInfos.Select(x => new ParameterRecord(this, x)).ToList();
			}
		}

		public string GetXmlDocText(string key) {
			var node = _memberDocNode.Value;
			if (null == node)
				return null;

			var targetNode = node.SelectSingleNode(key);
			if (null == targetNode)
				return null;

			return TextUtilities.ExtractIndentedNormalizedInnerText(targetNode.InnerXml);
		}

		public IList<SeeAlsoReference> SeeAlso {
			get { throw new NotImplementedException(); }
		}
	}
}
