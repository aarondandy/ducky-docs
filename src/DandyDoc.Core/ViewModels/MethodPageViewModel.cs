using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class MethodPageViewModel
	{

		private readonly Lazy<MethodDefinitionXmlDoc> _xmlDoc;

		public MethodPageViewModel(MethodDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			Definition = definition;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_xmlDoc = new Lazy<MethodDefinitionXmlDoc>(() => XmlDocOverlay.GetDocumentation(Definition));
		}

		public string Title {
			get {
				string name;
				string kind;
				if (Definition.IsConstructor) {
					kind = "Constructor";
					name = Definition.DeclaringType.Name;
				}
				else {
					name = Definition.Name;
					if (Definition.IsOperatorOverload()) {
						kind = "Operator";
						if (name.StartsWith("op_"))
							name = name.Substring(3);
					}
					else {
						kind = "Method";
					}
					name = String.Concat(Definition.DeclaringType.Name, '.', name);
				}
				if (Definition.HasParameters) {
					name = String.Concat(name, '(', String.Join(", ", Definition.Parameters.Select(x => x.ParameterType.Name)), ')');
				}
				return String.Concat(name, ' ', kind);
			}
		}

		public string InvokeName {
			get {
				if (Definition.IsConstructor)
					return Definition.DeclaringType.Name;
				if(Definition.IsOperatorOverload())
					throw new NotImplementedException();
				return Definition.Name;
			}
		}

		public MethodDefinition Definition { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public MethodDefinitionXmlDoc XmlDoc { get { return _xmlDoc.Value; } }

		public bool HasXmlDoc { get { return XmlDoc != null; } }

		public ParsedXmlElementBase Summary {
			get { return null == XmlDoc ? null : XmlDoc.Summary; }
		}

		public ParsedXmlElementBase Remarks {
			get { return null == XmlDoc ? null : XmlDoc.Remarks; }
		}

		public IList<ParsedXmlElementBase> Examples {
			get { return null == XmlDoc ? null : XmlDoc.Examples; }
		}

		public AssemblyNamespaceViewModel AssemblyNamespace { get { return new AssemblyNamespaceViewModel(Definition); } }

		public bool HasReturn { get { return Definition.ReturnType != null && Definition.ReturnType.FullName != "System.Void"; } }

		public ReturnViewModel CreateReturnViewModel() {
			var methodXmlDocs = XmlDoc;
			var docs = null == methodXmlDocs ? null : methodXmlDocs.Returns;
			return new ReturnViewModel(Definition.ReturnType, docs);
		}

		public IEnumerable<ParameterViewModel> CreateViewModels(IEnumerable<ParameterDefinition> definitions) {
			var methodXmlDocs = XmlDoc;
			return definitions.Select(item => {
				var docs = null == methodXmlDocs ? null : methodXmlDocs.DocsForParameter(item.Name);
				return new ParameterViewModel(item, docs);
			});
		}
	}
}
