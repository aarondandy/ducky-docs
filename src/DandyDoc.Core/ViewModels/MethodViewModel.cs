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

		private readonly MethodDefinitionXmlDoc _xmlDocsOverride;

		public MethodViewModel(MethodDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null, MethodDefinitionXmlDoc xmlDocsOverride = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
			_xmlDocsOverride = xmlDocsOverride;
		}

		public override string Title {
			get{
				return Definition.IsConstructor
					? ShortName
					: base.Title;
			}
		}

		public override string SubTitle {
			get{
				if (Definition.IsConstructor)
					return "Constructor";
				if (Definition.IsOperatorOverload())
					return "Operator";
				return "Method";
			}
		}

		public override DefinitionXmlDocBase XmlDoc { get { return _xmlDocsOverride ?? base.XmlDoc; } }

		public virtual MethodDefinitionXmlDoc MethodXmlDoc { get { return _xmlDocsOverride ?? (MethodDefinitionXmlDoc)XmlDoc; } }

		protected override IEnumerable<MemberFlair> GetFlairTags(){
			foreach (var item in base.GetFlairTags())
				yield return item;

			if (Definition.IsExtensionMethod())
				yield return new MemberFlair("extension", "Extension", "This method is an extension method.");

			if (AllResultsAndParamsNotNull)
				yield return new MemberFlair("no nulls", "Null Values", "This method does not return or accept null values for reference types.");

			if (IsPure)
				yield return new MemberFlair("pure", "Purity", "Does not have side effects");

			if(Definition.IsOperatorOverload())
				yield return new MemberFlair("operator", "Operator", "This method is invoked through a language operator.");

			if (Definition.IsSealed()) {
				var subject =
					Definition.IsGetter
						? "getter"
					: Definition.IsSetter
						? "setter"
					: "method";
				yield return new MemberFlair("sealed", "Inheritance", String.Format("This {0} is sealed, preventing inheritance.", subject));
			}

			if (!Definition.DeclaringType.IsInterface) {
				if (Definition.IsAbstract && !Definition.DeclaringType.IsInterface)
					yield return new MemberFlair("abstract", "Inheritance", "This method is abstract and must be implemented by inheriting types.");
				else if (Definition.IsVirtual && Definition.IsNewSlot && !Definition.IsFinal)
					yield return new MemberFlair("virtual", "Inheritance", "This method is virtual and can be overridden by inheriting types.");
			}
		}

		public virtual bool IsPure {
			get {
				if (HasXmlDoc && XmlDoc.HasPureElement)
					return true;
				if (Definition.HasPureAttribute())
					return true;
				return false;
			}
		}

		public virtual bool AllResultsAndParamsNotNull{
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

		public virtual IList<ParsedXmlException> Exceptions {
			get { return null == XmlDoc ? null : MethodXmlDoc.Exceptions; }
		}

		public virtual bool HasExceptions{
			get{
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
			}
		}

		public virtual IList<ParsedXmlContractCondition> Requires {
			get { return null == XmlDoc ? null : MethodXmlDoc.Requires; }
		}

		public virtual bool HasRequires{
			get {
				var requires = Requires;
				return null != requires && requires.Count > 0;
			}
		}

		public virtual IList<ParsedXmlContractCondition> Ensures{
			get { return null == XmlDoc ? null : MethodXmlDoc.Ensures; }
		}

		public virtual bool HasEnsures {
			get {
				var ensures = Ensures;
				return null != ensures && ensures.Count > 0;
			}
		}

		public virtual bool HasReturn { get { return Definition.ReturnType != null && Definition.ReturnType.FullName != "System.Void"; } }

		public virtual bool EnsuresResultNotNull{
			get{
				return HasReturn
					&& HasXmlDoc
					&& MethodXmlDoc.Ensures.Count > 0
					&& MethodXmlDoc.Ensures.Any(x => x.EnsuresResultNotNull);
			}
		}

		public virtual bool EnsuresResultNotNullOrEmpty {
			get{
				return HasReturn
					&& HasXmlDoc
					&& MethodXmlDoc.Ensures.Count > 0
					&& MethodXmlDoc.Ensures.Any(x => x.EnsuresResultNotNullOrEmpty);
			}
		}

		public virtual bool RequiresParameterNotNull(string parameterName){
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!HasXmlDoc || MethodXmlDoc.Requires.Count == 0)
				return false;
			return MethodXmlDoc.Requires.Any(x => x.RequiresParameterNotNull(parameterName));
		}

		public virtual bool RequiresParameterNotNullOrEmpty(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			if (!HasXmlDoc || MethodXmlDoc.Requires.Count == 0)
				return false;
			return MethodXmlDoc.Requires.Any(x => x.RequiresParameterNotNullOrEmpty(parameterName));
		}

		public virtual MethodReturnViewModel CreateReturnViewModel() {
			if(!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.EndContractBlock();
			var methodXmlDocs = MethodXmlDoc;
			var docs = null == methodXmlDocs ? null : methodXmlDocs.Returns;
			Contract.Assume(null != Definition.ReturnType);
			return new MethodReturnViewModel(Definition.ReturnType, this, docs);
		}

		public virtual IEnumerable<MethodParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if(null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<MethodParameterViewModel>>() != null);
			var methodXmlDocs = MethodXmlDoc;
			return definitions.Select(item => {
				var docs = null == methodXmlDocs ? null : methodXmlDocs.DocsForParameter(item.Name);
				return new MethodParameterViewModel(item, this, docs);
			});
		}

		public virtual IEnumerable<RequiresViewModel> ToRequiresViewModels(IEnumerable<ParsedXmlContractCondition> contracts) {
			if(null == contracts) throw new ArgumentNullException("contracts");
			Contract.Ensures(Contract.Result<IEnumerable<RequiresViewModel>>() != null);
			return contracts.Select(c => new RequiresViewModel(this, c));
		}

		public virtual IEnumerable<EnsuresViewModel> ToEnsuresViewModels(IEnumerable<ParsedXmlContractCondition> contracts) {
			if (null == contracts) throw new ArgumentNullException("contracts");
			Contract.Ensures(Contract.Result<IEnumerable<RequiresViewModel>>() != null);
			return contracts.Select(c => new EnsuresViewModel(this, c));
		}

		public virtual IEnumerable<GenericParameterMethodViewModel> ToGenericParameterViewModels(IEnumerable<GenericParameter> parameters) {
			if (null == parameters) throw new ArgumentNullException("parameters");
			Contract.Ensures(Contract.Result<IEnumerable<GenericTypeParameterViewModel>>() != null);
			return parameters.Select(p => new GenericParameterMethodViewModel(p, this));
		}

	}
}
