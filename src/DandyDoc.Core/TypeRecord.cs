using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class TypeRecord : IDocumentableEntity
	{

		private readonly ConcurrentDictionary<MethodDefinition, MethodRecord> _methodRecordsCache;
		private readonly ConcurrentDictionary<MethodDefinition, ConstructorRecord> _constructorRecordsCache;
		private readonly ConcurrentDictionary<TypeDefinition, NestedTypeRecord> _nestedTypeRecordCache;
		private readonly ConcurrentDictionary<FieldDefinition, FieldRecord> _fieldRecordsCache;
		private readonly ConcurrentDictionary<PropertyDefinition, PropertyRecord> _propertyRecordsCache;
		private readonly Lazy<XmlNode> _typeDocNode;

		internal TypeRecord(AssemblyRecord parent, TypeDefinition core) {
			Contract.Requires(null != parent);
			Contract.Requires(null != core);
			CoreType = core;
			Parent = parent;
			_typeDocNode = new Lazy<XmlNode>(GetDocNode, LazyThreadSafetyMode.ExecutionAndPublication);
			_methodRecordsCache = CoreType.HasMethods ? new ConcurrentDictionary<MethodDefinition, MethodRecord>() : null;
			_constructorRecordsCache = CoreType.HasMethods ? new ConcurrentDictionary<MethodDefinition, ConstructorRecord>() : null;
			_nestedTypeRecordCache = CoreType.HasNestedTypes ? new ConcurrentDictionary<TypeDefinition, NestedTypeRecord>() : null;
			_fieldRecordsCache = CoreType.HasFields ? new ConcurrentDictionary<FieldDefinition, FieldRecord>() : null;
			_propertyRecordsCache = CoreType.HasProperties ? new ConcurrentDictionary<PropertyDefinition, PropertyRecord>() : null;
		}

		public TypeDefinition CoreType { get; private set; }

		public AssemblyRecord Parent { get; private set; }

		public string Name { get { return CoreType.Name; } }

		public string FullName { get { return CoreType.FullName; } }


		public bool MatchesCref(string cref) {
			return FullName == cref
				|| Cref == cref;
		}

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
				return Methods
					.Concat(Constructors);
			}
		}

		public bool HasNestedTypes { get { return CoreType.HasNestedTypes; } }

		public IEnumerable<NestedTypeRecord> NestedTypeRecords {
			get { return CoreType.NestedTypes.ConvertAll(ToNestedTypeRecord); }
		}

		private NestedTypeRecord ToNestedTypeRecord(TypeDefinition type) {
			Contract.Requires(null != type);
			Contract.Assume(null != _nestedTypeRecordCache);
			return _nestedTypeRecordCache.GetOrAdd(type, x => new NestedTypeRecord(this, type));
		}

		public bool HasConstructors { get { return CoreType.HasMethods && CoreConstructorDefinitions.Any(); } }

		private IEnumerable<MethodDefinition> CoreConstructorDefinitions { get { return CoreType.Methods.Where(m => m.IsConstructor); } }

		public IEnumerable<ConstructorRecord> Constructors { get { return CoreConstructorDefinitions.Select(ToConstructorRecord); } }

		private ConstructorRecord ToConstructorRecord(MethodDefinition md) {
			return _constructorRecordsCache.GetOrAdd(md, x => new ConstructorRecord(this, md));
		}

		public bool HasMethods { get { return CoreType.HasMethods && CoreMethodDefinitions.Any(); } }

		private IEnumerable<MethodDefinition> CoreMethodDefinitions { get { return CoreType.Methods.Where(m => !m.IsConstructor); } }

		public IEnumerable<MethodRecord> Methods { get { return CoreMethodDefinitions.Select(ToMethodRecord); } }

		private MethodRecord ToMethodRecord(MethodDefinition md) {
			return _methodRecordsCache.GetOrAdd(md, x => new MethodRecord(this, md));
		}

		public bool HasFields { get { return CoreType.HasFields; } }

		public IEnumerable<FieldRecord> Fields { get { return CoreType.Fields.Select(ToFieldRecord); } }

		private FieldRecord ToFieldRecord(FieldDefinition fd) {
			return _fieldRecordsCache.GetOrAdd(fd, x => new FieldRecord(this, fd));
		}

		public bool HasProperties { get { return CoreType.HasProperties; } }

		public IEnumerable<PropertyRecord> Properties { get { return CoreType.Properties.Select(ToPropertyRecord); } } 

		private PropertyRecord ToPropertyRecord(PropertyDefinition pd) {
			return _propertyRecordsCache.GetOrAdd(pd, x => new PropertyRecord(this, pd));
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

		public MemberRecord ResolveCrefAsMember(string cref) {
			if (String.IsNullOrEmpty(cref))
				return null;
			if (cref.Length > 2 && cref[1] == ':')
				cref = cref.Substring(2);
			var lastNamePart = NameUtilities.GetLastNamePart(cref);
			if (String.IsNullOrEmpty(lastNamePart))
				return null;
			return Members.FirstOrDefault(x => x.MemberDefinition.Name == lastNamePart);
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

		public virtual string Cref {
			get { return FullName; }
		}

	}
}
