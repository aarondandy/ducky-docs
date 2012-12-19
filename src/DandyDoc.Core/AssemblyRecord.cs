using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class AssemblyRecord : IDocumentableEntity
	{

		public static AssemblyRecord CreateFromFilePath(string filePath, AssemblyGroup parentGroup = null) {
			if(String.IsNullOrEmpty(filePath)) throw new ArgumentException("Invalid file path.","filePath");
			Contract.EndContractBlock();

			var fileInfo = new FileInfo(filePath);

			if(!fileInfo.Exists)
				throw new FileNotFoundException("The given file was not found.",filePath);


			var assemblyDefinition = AssemblyDefinition.ReadAssembly(fileInfo.FullName);
			return new AssemblyRecord(assemblyDefinition, fileInfo, parentGroup);
		}

		public static List<FileInfo> PossibleXmlDocLocations(FileInfo assemblyFileLocation) {
			if(null == assemblyFileLocation) throw new ArgumentNullException("assemblyFileLocation");
			Contract.EndContractBlock();

			var directory = assemblyFileLocation.Directory;
			if (null == directory) return null;
			var searchName = Path.ChangeExtension(assemblyFileLocation.Name, "XML").ToUpperInvariant();
			var candidates = directory.GetFiles().Where(x => String.Equals(x.Name, searchName, StringComparison.OrdinalIgnoreCase));
			return candidates.ToList();
		}

		private readonly ConcurrentDictionary<TypeDefinition, TypeRecord> _typeRecordCache;
		private readonly Lazy<XmlDocument> _xmlDocument;

		public AssemblyRecord(AssemblyDefinition coreAssembly, FileInfo coreAssemblyFilePath, AssemblyGroup parentGroup = null) {
			if(null == coreAssembly) throw new ArgumentNullException("coreAssembly");
			Contract.EndContractBlock();

			CoreAssemblyDefinition = coreAssembly;
			CoreAssemblyFilePath = coreAssemblyFilePath;
			_typeRecordCache = new ConcurrentDictionary<TypeDefinition, TypeRecord>();
			_xmlDocument = new Lazy<XmlDocument>(ReadXmlDocumentation, LazyThreadSafetyMode.ExecutionAndPublication);
			ParentGroup = parentGroup ?? new AssemblyGroup(this);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(CoreAssemblyDefinition != null);
			Contract.Invariant(ParentGroup != null);
		}

		public AssemblyGroup ParentGroup { get; private set; }

		public AssemblyDefinition CoreAssemblyDefinition { get; private set; }

		public FileInfo CoreAssemblyFilePath { get; private set; }

		public string Name { get { return CoreAssemblyDefinition.Name.Name; } }

		public IEnumerable<TypeRecord> TypeRecords {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<TypeRecord>>() != null);
				return CoreAssemblyDefinition.Modules.SelectMany(x => x.Types).Select(ToTypeRecord);
			}
		}

		private XmlDocument ReadXmlDocumentation() {
			var location = PossibleXmlDocLocations(CoreAssemblyFilePath).FirstOrDefault(x => x.Exists);
			if (null == location)
				return null;

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(location.FullName);
			return xmlDoc;
		}

		private TypeRecord ToTypeRecord(TypeDefinition arg){
			Contract.Requires(null != arg);
			Contract.Ensures(Contract.Result<TypeRecord>() != null);
			return _typeRecordCache.GetOrAdd(arg, td => new TypeRecord(this, td));
		}

		public XmlNode GetXmlNodeForType(TypeDefinition type){
			var xmlDoc = _xmlDocument.Value;
			if (null == xmlDoc)
				return null;
			return xmlDoc.SelectSingleNode(
				String.Format("/doc/members/member[@name=\"T:{0}\"]", type.FullName));
		}

		public XmlNode GetXmlNodeForMember(TypeDefinition type, MemberRecord member) {
			var xmlDoc = _xmlDocument.Value;
			if (null == xmlDoc)
				return null;

			var memberName = member.Name;
			var parameters = member.Parameters;
			if (null != parameters && parameters.Count > 0) {
				memberName += String.Concat('(', String.Join(",", parameters.Select(x => x.FullTypeName)), ')');
			}

			return xmlDoc.SelectSingleNode(
				String.Format("/doc/members/member[@name=\"M:{0}.{1}\"]", type.FullName, memberName));
		}

		public IDocumentableEntity ResolveCref(string cref, bool checkParent = true) {
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref.","cref");
			Contract.EndContractBlock();

			char hint;
			if (cref.Length > 2 && cref[1] == ':') {
				hint = cref[0];
				cref = cref.Substring(2);
			}
			else {
				hint = default(char);
			}

			var result = ResolveCref(hint, cref);
			if (null != result)
				return result;

			if(checkParent)
				throw new NotImplementedException();

			return null;
		}

		private IDocumentableEntity ResolveCref(char hint, string cref) {
			if (hint == 'M' || hint == 'P') {
				return ResolveCrefAsMember(cref)
					?? ResolveCrefAsType(cref);
			}
			else {
				return ResolveCrefAsType(cref)
					?? ResolveCrefAsMember(cref);
			}
		}

		private TypeRecord ResolveCrefAsType(string cref) {
			// TODO: get these from an index lookup
			return TypeRecords.FirstOrDefault(x => x.CoreType.FullName == cref);
		}

		private IDocumentableEntity ResolveCrefAsMember(string cref) {
			var typeName = NameUtilities.ParentDotSeperatedName(cref);
			if (String.IsNullOrEmpty(typeName))
				return null;
			var typeEntity = ResolveCrefAsType(typeName);
			if (null == typeEntity)
				return null;
			return typeEntity.ResolveCrefAsMember(cref);
		}


		public ParsedXmlDoc Summary {
			get { throw new NotImplementedException(); }
		}

		public ParsedXmlDoc Remarks {
			get { throw new NotImplementedException(); }
		}

		public IList<SeeAlsoReference> SeeAlso {
			get { throw new NotImplementedException(); }
		}


	}
}
