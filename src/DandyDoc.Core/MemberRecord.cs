using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DandyDoc.Core
{
	public class MemberRecord : IDocumentableEntity
	{

		private readonly Lazy<XmlNode> _memberDocNode;

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

		public string FullName { get { return CoreMemberInfo.FullName; } }

		public TypeRecord ParentType { get; private set; }

		public IMemberDefinition CoreMemberInfo { get; private set; }

		public bool IsMethod { get { return CoreMemberInfo is MethodDefinition && !((MethodDefinition)CoreMemberInfo).IsConstructor; } }

		public bool IsConstructor { get { return CoreMemberInfo is MethodDefinition && ((MethodDefinition)CoreMemberInfo).IsConstructor; } }

		public bool IsField { get { return CoreMemberInfo is FieldDefinition; } }

		public ParsedXmlDoc Summary { get { return new ParsedXmlDoc(GetSubNode("summary"), this); } }

		public IList<ParsedXmlDoc> Remarks { get { return GetSubNodes("remarks").Select(x => new ParsedXmlDoc(x, this)).ToList(); } }

		public IList<ParsedXmlDoc> Examples { get { return GetSubNodes("example").Select(x => new ParsedXmlDoc(x, this)).ToList(); } }

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

		private IEnumerable<XmlNode> GetSubNodes(string query) {
			Contract.Requires(!String.IsNullOrEmpty(query));
			var node = XmlDocNode;
			if (null == node)
				return Enumerable.Empty<XmlNode>();
			var result = node.SelectNodes(query);
			if (null == result)
				return Enumerable.Empty<XmlNode>();
			return result.Cast<XmlNode>();
		}

		private XmlNode GetSubNode(string query) {
			Contract.Requires(!String.IsNullOrEmpty(query));
			var node = XmlDocNode;
			if (null == node)
				return null;
			return node.SelectSingleNode(query);
		}

		public string GetXmlDocText(string key) {
			Contract.Requires(!String.IsNullOrEmpty(key));
			var targetNode = GetSubNode(key);
			if (null == targetNode)
				return null;

			return TextUtilities.ExtractIndentedNormalizedInnerText(targetNode.InnerXml);
		}

		public IList<SeeAlsoReference> SeeAlso {
			get { throw new NotImplementedException(); }
		}


		public XmlNode XmlDocNode {
			get { return _memberDocNode.Value; }
		}

		public IDocumentableEntity ResolveCref(string crefName){
			return ParentType.ResolveCref(crefName);
		}
	}
}
