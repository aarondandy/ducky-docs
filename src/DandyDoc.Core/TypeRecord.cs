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

namespace DandyDoc.Core
{
	public class TypeRecord : IDocumentableEntity
	{

		private readonly ConcurrentDictionary<MemberInfo, MemberRecord> _memberRecordCache;
		private Lazy<XmlNode> _typeDocNode;

		internal TypeRecord(AssemblyRecord parent, Type core) {
			Contract.Requires(null != parent);
			Contract.Requires(null != core);
			CoreType = core;
			Parent = parent;
			_typeDocNode = new Lazy<XmlNode>(GetDocNode, LazyThreadSafetyMode.ExecutionAndPublication);
			_memberRecordCache = new ConcurrentDictionary<MemberInfo, MemberRecord>();
		}

		public Type CoreType { get; private set; }

		public AssemblyRecord Parent { get; private set; }

		public string Name { get { return CoreType.Name; } }

		public string Summary { get { return GetXmlDocText("summary"); } }

		public string Remarks { get { return GetXmlDocText("remarks"); } }

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
						results.Add(new SeeAlsoReference(entity, seeAlsoNode.InnerXml));
					}
				}
				return results;
			}
		}

		public IEnumerable<MemberRecord> Members { get { return CoreType.GetMembers().Select(ToMemberRecord); } }

		private MemberRecord ToMemberRecord(MemberInfo mi) {
			return _memberRecordCache.GetOrAdd(mi, x => new MemberRecord(this, x));
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

			return TextUtilities.ExtractIndentedNormalizedInnerText(targetNode.InnerText);
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
