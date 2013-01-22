using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using DandyDoc.Overlays.Cref;
using DandyDoc.Utility;
using Mono.Cecil;
using System.IO;

namespace DandyDoc.Overlays.XmlDoc
{
	public class XmlDocOverlay
	{

		private readonly ConcurrentDictionary<AssemblyDefinition, XmlDocument> _xmlDocRootCache;

		public XmlDocOverlay(CRefOverlay cRefOverlay, string xmlSearchPath = null) {
			if(null == cRefOverlay) throw new ArgumentNullException("cRefOverlay");
			Contract.EndContractBlock();
			CRefOverlay = cRefOverlay;
			_xmlDocRootCache = new ConcurrentDictionary<AssemblyDefinition, XmlDocument>();
			XmlSearchPath = xmlSearchPath;
		}

		public string XmlSearchPath { get; set; }

		public XmlDocument Load(AssemblyDefinition assemblyDefinition){
			if(null == assemblyDefinition) throw new ArgumentNullException("assemblyDefinition");
			Contract.EndContractBlock();
			if (null == assemblyDefinition.MainModule)
				return null;
			 
			Contract.Assume(null != assemblyDefinition.MainModule.FullyQualifiedName);
			var assemblyFilePath = new FileInfo(assemblyDefinition.MainModule.FullyQualifiedName);
			if (!assemblyFilePath.Exists)
				return null;

			var xmlPath = SuggestedXmlDocPath(assemblyFilePath);
			return LoadFromFilePath(xmlPath);
		}

		public FileInfo SuggestedXmlDocPath(FileInfo assemblyFilePath) {
			if(null == assemblyFilePath) throw new ArgumentNullException("assemblyFilePath");
			Contract.Ensures(Contract.Result<FileInfo>() != null);
			Contract.EndContractBlock();

			var fileName = Path.ChangeExtension(assemblyFilePath.Name, "XML");

			if (!String.IsNullOrEmpty(XmlSearchPath)){
				var searchPath = Path.Combine(XmlSearchPath, fileName);
				if(File.Exists(searchPath))
					return new FileInfo(searchPath);
			}

			var basePath = Path.ChangeExtension(assemblyFilePath.FullName, "XML");
			if(File.Exists(basePath))
				return new FileInfo(basePath);

			var baseFolderPath = Path.GetDirectoryName(assemblyFilePath.FullName);
			var baseBinFolderPath = Path.Combine(baseFolderPath, "bin");
			var baseBinXmlPath = Path.Combine(baseBinFolderPath, fileName);
			if(File.Exists(baseBinXmlPath))
				return new FileInfo(baseBinXmlPath);

			return new FileInfo(basePath);
		}

		public static XmlDocument LoadFromFilePath(FileInfo xmlFilePath) {
			if(null == xmlFilePath) throw new ArgumentNullException("xmlFilePath");
			Contract.EndContractBlock();
			if (!xmlFilePath.Exists)
				return null;

			var result = new XmlDocument();
			result.Load(xmlFilePath.FullName);
			var members = result.SelectNodes("/doc/members/member");
			if (null == members)
				return result;

			foreach (var member in members.Cast<XmlElement>()){
				var subElements = member.ChildNodes.OfType<XmlElement>().ToList();
				foreach (var subElement in subElements) {
					var replacement = result.CreateDocumentFragment();
					replacement.InnerXml = TextUtility.NormalizeAndUnindentElement(subElement.OuterXml) + "\n";
					Contract.Assume(null != subElement.ParentNode);
					subElement.ParentNode.ReplaceChild(replacement, subElement);
				}
			}
			return result;
		}

		private XmlDocument GetDocumentForAssembly(AssemblyDefinition assemblyDefinition){
			Contract.Requires(null != assemblyDefinition);
			return _xmlDocRootCache.GetOrAdd(assemblyDefinition, Load);
		}

		public DefinitionXmlDocBase GetDocumentation(IMemberDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var memberReference = definition as MemberReference;
			return null == memberReference ? null : GetDocumentation(memberReference);
		}

		public DefinitionXmlDocBase GetDocumentation(MemberReference memberReference) {
			if(null == memberReference) throw new ArgumentNullException("memberReference");
			Contract.EndContractBlock();

			if (memberReference is TypeDefinition)
				return GetDocumentation((TypeDefinition)memberReference);
			if (memberReference is MethodDefinition)
				return GetDocumentation((MethodDefinition)memberReference);
			if (memberReference is PropertyDefinition)
				return GetDocumentation((PropertyDefinition)memberReference);
			if (memberReference is FieldDefinition)
				return GetDocumentation((FieldDefinition)memberReference);
			if (memberReference is EventDefinition)
				return GetDocumentation((EventDefinition)memberReference);
			throw new NotSupportedException();
		}

		public TypeDefinitionXmlDoc GetDocumentation(TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			if (null == node)
				return null;

			if (definition.IsDelegateType())
				return new DelegateTypeDefinitionXmlDoc(definition, node, CRefOverlay);
			return new TypeDefinitionXmlDoc(definition, node, CRefOverlay);
		}

		public MethodDefinitionXmlDoc GetDocumentation(MethodDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new MethodDefinitionXmlDoc(definition, node, CRefOverlay);
		}

		public PropertyDefinitionXmlDoc GetDocumentation(PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new PropertyDefinitionXmlDoc(definition, node, CRefOverlay);
		}

		public FieldDefinitionXmlDoc GetDocumentation(FieldDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new FieldDefinitionXmlDoc(definition, node, CRefOverlay);
		}

		public EventDefinitionXmlDoc GetDocumentation(EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new EventDefinitionXmlDoc(definition, node, CRefOverlay);
		}

		private XmlNode GetNodeForDefinition(MemberReference definition) {
			Contract.Requires(null != definition);
			Contract.Assume(null != definition.Module.Assembly);
			var doc = GetDocumentForAssembly(definition.Module.Assembly);
			if (null == doc)
				return null;
			var cref = CRefOverlay.GetCref(definition, false);
			return doc.SelectSingleNode(
				String.Format("/doc/members/member[@name=\"{0}\"]", cref));
		}

		public CRefOverlay CRefOverlay { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != CRefOverlay);
			Contract.Invariant(null != _xmlDocRootCache);
		}

	}
}
