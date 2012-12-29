using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;
using Mono.Cecil;
using System.IO;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class XmlDocOverlay
	{

		private readonly ConcurrentDictionary<AssemblyDefinition, XmlDocument> _xmlDocRootCache;

		public XmlDocOverlay(CrefOverlay crefOverlay){
			if(null == crefOverlay) throw new ArgumentNullException("crefOverlay");
			Contract.EndContractBlock();
			CrefOverlay = crefOverlay;
			_xmlDocRootCache = new ConcurrentDictionary<AssemblyDefinition, XmlDocument>();
		}

		public static XmlDocument Load(AssemblyDefinition assemblyDefinition){
			if(null == assemblyDefinition) throw new ArgumentNullException("assemblyDefinition");
			Contract.EndContractBlock();
			if (null == assemblyDefinition.MainModule)
				return null;
			 
			Contract.Assume(null != assemblyDefinition.MainModule.FullyQualifiedName);
			var assemblyFilePath = new FileInfo(assemblyDefinition.MainModule.FullyQualifiedName);
			if (!assemblyFilePath.Exists)
				return null;

			var xmlPath = SuggestedXmlDocPaths(assemblyFilePath);
			return LoadFromFilePath(xmlPath);
		}

		public static FileInfo SuggestedXmlDocPaths(FileInfo assemblyFilePath) {
			if(null == assemblyFilePath) throw new ArgumentNullException("assemblyFilePath");
			Contract.Ensures(Contract.Result<FileInfo>() != null);
			Contract.EndContractBlock();
			return new FileInfo(Path.ChangeExtension(assemblyFilePath.FullName, "XML"));
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
				return new DelegateTypeDefinitionXmlDoc(definition, node, CrefOverlay);
			return new TypeDefinitionXmlDoc(definition, node, CrefOverlay);
		}

		public MethodDefinitionXmlDoc GetDocumentation(MethodDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new MethodDefinitionXmlDoc(definition, node, CrefOverlay);
		}

		public PropertyDefinitionXmlDoc GetDocumentation(PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new PropertyDefinitionXmlDoc(definition, node, CrefOverlay);
		}

		public FieldDefinitionXmlDoc GetDocumentation(FieldDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new FieldDefinitionXmlDoc(definition, node, CrefOverlay);
		}

		public EventDefinitionXmlDoc GetDocumentation(EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var node = GetNodeForDefinition(definition);
			return null == node ? null : new EventDefinitionXmlDoc(definition, node, CrefOverlay);
		}

		private XmlNode GetNodeForDefinition(MemberReference definition) {
			Contract.Requires(null != definition);
			Contract.Assume(null != definition.Module.Assembly);
			var doc = GetDocumentForAssembly(definition.Module.Assembly);
			if (null == doc)
				return null;
			var cref = CrefOverlay.GetCref(definition, false);
			return doc.SelectSingleNode(
				String.Format("/doc/members/member[@name=\"{0}\"]", cref));
		}

		public CrefOverlay CrefOverlay { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != CrefOverlay);
			Contract.Invariant(null != _xmlDocRootCache);
		}

	}
}
