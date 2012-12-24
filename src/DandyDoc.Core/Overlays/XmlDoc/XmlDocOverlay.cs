using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
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
			return result;
		}

		private XmlDocument GetDocumentForAssembly(AssemblyDefinition assemblyDefinition){
			Contract.Requires(null != assemblyDefinition);
			return _xmlDocRootCache.GetOrAdd(assemblyDefinition, Load);
		}

		public IDefinitionXmlDoc GetDocumentation(TypeDefinition definition){
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var cref = CrefOverlay.GetCref(definition, false);
			var doc = GetDocumentForAssembly(definition.Module.Assembly);
			if (null == doc)
				return null;

			var node = doc.SelectSingleNode(String.Format("/doc/members/member[@name=\"{0}\"]", cref));
			if (null == node)
				return null;

			return new TypeDefinitionXmlDoc(definition, node, CrefOverlay);
		}


		public CrefOverlay CrefOverlay { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != CrefOverlay);
			Contract.Invariant(null != _xmlDocRootCache);
		}

	}
}
