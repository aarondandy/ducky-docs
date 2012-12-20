using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class TypeRecord : IDocumentableEntity
	{

		private readonly ConcurrentDictionary<IMemberDefinition, MemberRecord> _memberRecordCache;
		private Lazy<XmlNode> _typeDocNode;

		internal TypeRecord(AssemblyRecord parent, TypeDefinition core) {
			Contract.Requires(null != parent);
			Contract.Requires(null != core);
			CoreType = core;
			Parent = parent;
			_typeDocNode = new Lazy<XmlNode>(GetDocNode, LazyThreadSafetyMode.ExecutionAndPublication);
			_memberRecordCache = new ConcurrentDictionary<IMemberDefinition, MemberRecord>();
		}

		public TypeDefinition CoreType { get; private set; }

		public AssemblyRecord Parent { get; private set; }

		public string Name { get { return CoreType.Name; } }

		public string FullName { get { return CoreType.FullName; } }

		public ParsedXmlDoc Summary { get { return new ParsedXmlDoc(GetSubNode("summary"), this); } }

		public IList<ParsedXmlDoc> Remarks { get { return GetSubNodes("remarks").Select(x => new ParsedXmlDoc(x, this)).ToList(); } }

		public IList<ParsedXmlDoc> Examples { get { return GetSubNodes("example").Select(x => new ParsedXmlDoc(x, this)).ToList(); } }

		public IList<SeeAlsoReference> SeeAlso {
			get {
				var results = new List<SeeAlsoReference>();
				var node = _typeDocNode.Value;
				if (null == node)
					return results;
				var seeAlsoNodes = node.SelectNodes("seealso");
				if(null == seeAlsoNodes)
					return results;

				
				foreach(var seeAlsoNode in seeAlsoNodes.OfType<XmlNode>()) {
					if(null == seeAlsoNode.Attributes)
						continue;
					var crefAttribute = seeAlsoNode.Attributes["cref"];
					if (null == crefAttribute)
						continue;
					var cref = crefAttribute.Value;
					if (String.IsNullOrEmpty(cref))
						continue;
					var entity = Parent.ResolveCref(cref);
					if (null != entity) {
						results.Add(new SeeAlsoReference(entity, new ParsedXmlDoc(seeAlsoNode,this)));
					}
				}
				return results;
			}
		}

		public IEnumerable<MemberRecord> Members{
			get{
				var memberDefinitions = CoreType.Methods.Cast<IMemberDefinition>()
					.Concat(CoreType.Properties.Cast<IMemberDefinition>())
					.Concat(CoreType.Fields.Cast<IMemberDefinition>());
				return memberDefinitions.Select(ToMemberRecord);
			}
		}

		public bool IsPublic { get { return CoreType.IsPublic; } }

		public string Namespace {
			get { return CoreType.Namespace; }
		}

		public string[] NamespaceParts{
			get { return NameUtilities.SplitNamespaceParts(Namespace); }
		}

		public bool IsValueType {
			get { return CoreType.IsValueType; }
		}

		private MemberRecord ToMemberRecord(IMemberDefinition md){
			return _memberRecordCache.GetOrAdd(md, x => new MemberRecord(this, md));
		}

		public MemberRecord ResolveCrefAsMember(string cref) {
			if (String.IsNullOrEmpty(cref))
				return null;
			if (cref.Length > 2 && cref[1] == ':')
				cref = cref.Substring(2);
			var lastNamePart = NameUtilities.GetLastNamePart(cref);
			if (String.IsNullOrEmpty(lastNamePart))
				return null;
			return Members.FirstOrDefault(x => x.CoreMemberInfo.Name == lastNamePart);
		}

		private IEnumerable<XmlNode> GetSubNodes(string query){
			Contract.Requires(!String.IsNullOrEmpty(query));
			var node = XmlDocNode;
			if (null == node)
				return Enumerable.Empty<XmlNode>();
			var result = node.SelectNodes(query);
			if(null == result)
				return Enumerable.Empty<XmlNode>();
			return result.Cast<XmlNode>();
		}

		private XmlNode GetSubNode(string query){
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

		private XmlNode GetDocNode() {
			return Parent.GetXmlNodeForType(CoreType);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != CoreType);
			Contract.Invariant(null != Parent);
		}

		public XmlNode XmlDocNode { get { return _typeDocNode.Value; } }


		public IDocumentableEntity ResolveCref(string cref) {
			if (String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref.", "cref");
			Contract.EndContractBlock();
			return Parent.ResolveCref(cref);
		}


	}
}
