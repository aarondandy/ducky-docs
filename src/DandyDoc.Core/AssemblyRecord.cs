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

namespace DandyDoc.Core
{
	public class AssemblyRecord
	{

		public static AssemblyRecord CreateFromFilePath(string filePath) {
			if(String.IsNullOrEmpty(filePath)) throw new ArgumentException("Invalid file path.","filePath");
			Contract.EndContractBlock();

			var assembly = Assembly.ReflectionOnlyLoadFrom(filePath);
			var assemblyName = assembly.GetName();
			assembly = AppDomain.CreateDomain("filePath").Load(assemblyName);

			
			if (null == assembly)
				return null;
			return new AssemblyRecord(assembly);
		}

		public static IList<FileInfo> PossibleAssemblyLocations(Assembly assembly) {
			var results = new List<FileInfo>(2);
			var codeBase = assembly.CodeBase;
			if (!String.IsNullOrEmpty(codeBase)) {
				var localPath = new Uri(codeBase).LocalPath;
				if (!String.IsNullOrEmpty(localPath) && File.Exists(localPath))
					results.Add(new FileInfo(localPath));
			}

			var location = assembly.Location;
			if (!String.IsNullOrEmpty(location) && File.Exists(location))
				results.Add(new FileInfo(location));

			return results.Distinct().ToArray();
		}

		public static List<FileInfo> PossibleXmlDocLocations(Assembly assembly) {
			var results = new List<FileInfo>();
			foreach (var assemblyFileLocation in PossibleAssemblyLocations(assembly)) {
				var directory = assemblyFileLocation.Directory;
				if (null == directory) continue;

				var searchName = Path.ChangeExtension(assemblyFileLocation.Name, "XML").ToUpperInvariant();
				var candidates = directory.GetFiles().Where(x => String.Equals(x.Name, searchName, StringComparison.OrdinalIgnoreCase));
				results.AddRange(candidates);
			}
			return results;
		}

		private readonly ConcurrentDictionary<TypeInfo, TypeRecord> _typeRecordCache;
		private readonly Lazy<XmlDocument> _xmlDocument;

		public AssemblyRecord(Assembly coreAssembly, AssemblyGroup parentGroup = null) {
			if(null == coreAssembly) throw new ArgumentNullException("coreAssembly");
			Contract.EndContractBlock();

			CoreAssembly = coreAssembly;
			_typeRecordCache = new ConcurrentDictionary<TypeInfo, TypeRecord>();
			_xmlDocument = new Lazy<XmlDocument>(ReadXmlDocumentation, LazyThreadSafetyMode.ExecutionAndPublication);
			ParentGroup = parentGroup ?? new AssemblyGroup(this);
		}

		public AssemblyGroup ParentGroup { get; private set; }

		public Assembly CoreAssembly { get; private set; }

		public FileInfo CoreAssemblyFilePath {
			get { return PossibleAssemblyLocations(CoreAssembly).FirstOrDefault(); }
		}

		public IEnumerable<TypeRecord> TypeRecords {
			get {
				Contract.Ensures(Contract.Result<IEnumerable<TypeRecord>>() != null);
				return CoreAssembly.DefinedTypes.Select(ToTypeRecord);
			}
		}

		private XmlDocument ReadXmlDocumentation() {
			var location = PossibleXmlDocLocations(CoreAssembly).FirstOrDefault(x => x.Exists);
			if (null == location)
				return null;

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(location.FullName);
			return xmlDoc;
		}

		private TypeRecord ToTypeRecord(TypeInfo arg) {
			Contract.Requires(null != arg);
			Contract.Ensures(Contract.Result<TypeRecord>() != null);
			return _typeRecordCache.GetOrAdd(arg, ti => new TypeRecord(this, ti));
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(CoreAssembly != null);
		}


		public XmlNode GetXmlNodeForType(Type type) {
			var xmlDoc = _xmlDocument.Value;
			if (null == xmlDoc)
				return null;
			return xmlDoc.SelectSingleNode(
				String.Format("/doc/members/member[@name=\"T:{0}\"]", type.FullName));
		}

		public XmlNode GetXmlNodeForMember(Type type, MemberRecord member) {
			var xmlDoc = _xmlDocument.Value;
			if (null == xmlDoc)
				return null;

			var memberName = member.Name;
			var parameters = member.ParameterInfos;
			if (null != parameters && parameters.Length > 0) {
				memberName += String.Concat('(',String.Join(",",parameters.Select(x => x.ParameterType.FullName)),')');
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
			if (hint == 'M') {
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

	}
}
