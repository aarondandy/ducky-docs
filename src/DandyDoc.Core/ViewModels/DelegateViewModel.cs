using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class DelegateViewModel :
		TypeViewModel,
		IParameterizedDefinitionViewModel
	{

		private readonly Lazy<ReadOnlyCollection<ParameterDefinition>> _parameters;
		private readonly Lazy<TypeReference> _returnType;

		public DelegateViewModel(TypeDefinition definition, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null)
			: base(definition, xmlDocOverlay, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlDocOverlay);
			_parameters = new Lazy<ReadOnlyCollection<ParameterDefinition>>(() => new ReadOnlyCollection<ParameterDefinition>(Definition.GetDelegateTypeParameters()));
			_returnType = new Lazy<TypeReference>(() => Definition.GetDelegateReturnType());
		}

		new DelegateTypeDefinitionXmlDoc DelegateXmlDoc {
			get { return XmlDoc as DelegateTypeDefinitionXmlDoc; }
		}

		protected override IEnumerable<MemberFlair> GetFlairTags() {
			foreach (var tag in base.GetFlairTags())
				yield return tag;

			if (CanReturnNull)
				yield return new MemberFlair("null result", "Null Values", "This method may return null.");
			else if (AllResultsAndParamsNotNull)
				yield return new MemberFlair("no nulls", "Null Values", "This method does not return or accept null values for reference types.");
		}

		public virtual bool CanReturnNull {
			get { return Definition.HasAttributeMatchingName("CanBeNullAttribute"); }
		}

		public virtual bool AllResultsAndParamsNotNull {
			get {

				var hasReferenceReturn = HasReturn && !ReturnType.IsValueType;
				if (hasReferenceReturn) {
					if (!EnsuresResultNotNull && !EnsuresResultNotNullOrEmpty) {
						return false;
					}
				}
				else {
					if (!HasParameters)
						return false;
				}

				var refParams = Parameters.Where(p => !p.ParameterType.IsValueType).ToList();
				if (0 == refParams.Count) {
					if (!hasReferenceReturn)
						return false;
				}
				else {
					foreach (var paramName in refParams.Select(p => p.Name)) {
						Contract.Assume(!String.IsNullOrEmpty(paramName));
						if (!RequiresParameterNotNull(paramName) && !RequiresParameterNotNullOrEmpty(paramName))
							return false;
					}
				}

				return true;
			}
		}

		public virtual bool HasReturn {
			get {
				var returnType = ReturnType;
				return null != returnType && returnType.FullName != "System.Void";
			}
		}

		public virtual TypeReference ReturnType {
			get { return _returnType.Value; }
		}

		public virtual bool HasParameters {
			get { return CollectionUtility.IsNotNullOrEmpty(Parameters); }
		}

		public virtual IList<ParameterDefinition> Parameters {
			get { return _parameters.Value; }
		}

		public virtual IList<ParsedXmlException> Exceptions {
			get { return null == DelegateXmlDoc ? null : DelegateXmlDoc.Exceptions; }
		}

		public virtual bool HasExceptions {
			get {
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
			}
		}

		public virtual bool EnsuresResultNotNull {
			get {
				return HasReturn
					&& Definition.HasAttributeMatchingName("NotNullAttribute");
			}
		}

		public virtual bool EnsuresResultNotNullOrEmpty {
			get { return false; }
		}

		public virtual bool RequiresParameterNotNull(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			var parameter = Parameters.FirstOrDefault(p => p.Name == parameterName);
			if (null == parameter)
				return false;

			return parameter.HasAttributeMatchingName("NotNullAttribute");
		}

		public virtual bool RequiresParameterNotNullOrEmpty(string parameterName) {
			if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
			Contract.EndContractBlock();
			return false;
		}

		public virtual ReturnViewModel CreateReturnViewModel() {
			if (!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.Ensures(Contract.Result<ReturnViewModel>() != null);
			var delegateXmlDoc = DelegateXmlDoc;
			var docs = null == delegateXmlDoc ? null : delegateXmlDoc.Returns;
			Contract.Assume(null != ReturnType);
			return new ReturnViewModel(this, docs);
		}

		public virtual IEnumerable<ParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<ParameterViewModel>>() != null);
			var delegateXmlDocs = XmlDoc as DelegateTypeDefinitionXmlDoc;
			return null == delegateXmlDocs
				? definitions.Select(item => new ParameterViewModel(this, item, null))
				: definitions.Select(item => new ParameterViewModel(this, item, delegateXmlDocs.DocsForParameter(item.Name)));
		}

	}
}
