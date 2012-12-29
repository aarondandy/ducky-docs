using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.Core.ViewModels
{
	public class MethodViewModel : DefinitionViewModelBase<MethodDefinition>
	{

		public MethodViewModel(MethodDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
			Contract.EndContractBlock();
		}

		public override string Title {
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

		public override string ShortName {
			get {
				if (Definition.IsOperatorOverload()) {
					var name = Definition.Name;
					if (name.StartsWith("op_"))
						name = name.Substring(3);
					return name;
				}
				if (Definition.IsConstructor) {
					return String.Concat(Definition.DeclaringType.Name, '(', String.Join(", ", Definition.Parameters.Select(p => p.ParameterType.Name)), ')');
				}

				var isOverloaded = Definition.DeclaringType.Methods.Count(m => m.Name == Definition.Name) > 1;
				if (!isOverloaded)
					return Definition.Name;

				return String.Concat(Definition.Name, '(', String.Join(", ", Definition.Parameters.Select(p => p.ParameterType.Name)), ')');
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

		new public MethodDefinitionXmlDoc XmlDoc { get { return (MethodDefinitionXmlDoc)(base.XmlDoc); } }

		public bool HasReturn { get { return Definition.ReturnType != null && Definition.ReturnType.FullName != "System.Void"; } }

		public ReturnViewModel CreateReturnViewModel() {
			if(!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.EndContractBlock();
			var methodXmlDocs = XmlDoc;
			var docs = null == methodXmlDocs ? null : methodXmlDocs.Returns;
			Contract.Assume(null != Definition.ReturnType);
			return new ReturnViewModel(Definition.ReturnType, docs);
		}

		public IEnumerable<ParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<ParameterViewModel>>() != null);
			var methodXmlDocs = XmlDoc;
			return definitions.Select(item => {
				var docs = null == methodXmlDocs ? null : methodXmlDocs.DocsForParameter(item.Name);
				return new ParameterViewModel(item, docs);
			});
		}
	}
}
