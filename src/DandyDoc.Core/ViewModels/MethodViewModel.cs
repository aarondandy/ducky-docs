using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
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
			get{
				string name;
				string kind;
				if (Definition.IsConstructor) {
					kind = "Constructor";
					name = ShortName;
				}
				else {
					if (Definition.IsOperatorOverload()) {
						kind = "Operator";
					}
					else {
						kind = "Method";
					}
					Contract.Assume(null != Definition.DeclaringType);
					name = base.Title;
				}
				if (String.IsNullOrEmpty(kind))
					return name;
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

		new public MethodDefinitionXmlDoc XmlDoc { get { return (MethodDefinitionXmlDoc)(base.XmlDoc); } }

		protected override IEnumerable<string> GetFlairTags(){
			foreach (var item in base.GetFlairTags())
				yield return item;

			if (Definition.IsExtensionMethod()){
				yield return "extension";
			}

			if (AllResultsAndParamsNotNull){
				yield return "nonulls";
			}

			if (IsPure) 
				yield return "pure";

		}

		public bool IsPure {
			get {
				if (HasXmlDoc && XmlDoc.HasPureElement)
					return true;
				if (Definition.HasPureAttribute())
					return true;
				return false;
			}
		}

		public bool AllResultsAndParamsNotNull{
			get{

				var hasReferenceReturn = HasReturn && !Definition.ReturnType.IsValueType;
				if (hasReferenceReturn){
					if (!EnsuresResultNotNull && !EnsuresResultNotNullOrEmpty){
						return false;
					}
				}
				else{
					if (!Definition.HasParameters)
						return false;
				}

				var refParams = Definition.Parameters.Where(p => !p.ParameterType.IsValueType).ToList();
				if (0 == refParams.Count){
					if (!hasReferenceReturn)
						return false;
				}
				else{
					foreach (var paramName in refParams.Select(p => p.Name)) {
						Contract.Assume(!String.IsNullOrEmpty(paramName));
						if (!RequiresParameterNotNull(paramName) && !RequiresParameterNotNullOrEmpty(paramName))
							return false;
					}
				}

				return true;
			}
		}

		public IList<ParsedXmlException> Exceptions {
			get { return null == XmlDoc ? null : XmlDoc.Exceptions; }
		}

		public bool HasExceptions{
			get{
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
			}
		}

		public bool HasReturn { get { return Definition.ReturnType != null && Definition.ReturnType.FullName != "System.Void"; } }

		public bool EnsuresResultNotNull{
			get{
				return HasReturn
					&& HasXmlDoc
					&& XmlDoc.Ensures.Count > 0
					&& XmlDoc.Ensures.Any(x => x.EnsuresResultNotNull);
			}
		}

		public bool EnsuresResultNotNullOrEmpty {
			get{
				return HasReturn
					&& HasXmlDoc
					&& XmlDoc.Ensures.Count > 0
					&& XmlDoc.Ensures.Any(x => x.EnsuresResultNotNullOrEmpty);
			}
		}

		public bool RequiresParameterNotNull(string parameterName){
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!HasXmlDoc || XmlDoc.Requires.Count == 0)
				return false;
			return XmlDoc.Requires.Any(x => x.RequiresParameterNotNull(parameterName));
		}

		public bool RequiresParameterNotNullOrEmpty(string parameterName){
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!HasXmlDoc || XmlDoc.Requires.Count == 0)
				return false;
			return XmlDoc.Requires.Any(x => x.RequiresParameterNotNullOrEmpty(parameterName));
		}

		public ReturnViewModel CreateReturnViewModel() {
			if(!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.EndContractBlock();
			var methodXmlDocs = XmlDoc;
			var docs = null == methodXmlDocs ? null : methodXmlDocs.Returns;
			Contract.Assume(null != Definition.ReturnType);
			return new ReturnViewModel(Definition.ReturnType, this, docs);
		}

		public IEnumerable<ParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<ParameterViewModel>>() != null);
			var methodXmlDocs = XmlDoc;
			return definitions.Select(item => {
				var docs = null == methodXmlDocs ? null : methodXmlDocs.DocsForParameter(item.Name);
				return new ParameterViewModel(item, this, docs);
			});
		}
	}
}
