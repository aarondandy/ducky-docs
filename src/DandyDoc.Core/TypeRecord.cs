using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

		public ParsedXmlDoc Summary { get { return new ParsedXmlDoc(GetXmlDocText("summary"),this); } }

		public ParsedXmlDoc Remarks { get { return new ParsedXmlDoc(GetXmlDocText("remarks"),this); } }

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
						results.Add(new SeeAlsoReference(entity, new ParsedXmlDoc(seeAlsoNode.InnerXml,this)));
					}
				}
				return results;
			}
		}

		public IEnumerable<MemberRecord> Members{
			get{
				//return CoreType.GetMembers().Select(ToMemberRecord);
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

		/*private MemberRecord ToMemberRecord(MemberInfo mi) {
			return _memberRecordCache.GetOrAdd(mi, x => new MemberRecord(this, x));
		}*/

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

		public string GetXmlDocText(string key) {
			var node = _typeDocNode.Value;
			if (null == node)
				return null;

			var targetNode = node.SelectSingleNode(key);
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

	}
}
