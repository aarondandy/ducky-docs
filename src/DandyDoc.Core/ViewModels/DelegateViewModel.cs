using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class DelegateViewModel : TypeViewModel
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

		public virtual bool HasReturn {
			get {
				var returnType = ReturnType;
				return null != returnType && returnType.FullName != "System.Void";
			}
		}

		public virtual TypeReference ReturnType {
			get { return _returnType.Value; }
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

		public virtual ReturnViewModel CreateReturnViewModel() {
			if (!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.Ensures(Contract.Result<ReturnViewModel>() != null);
			var delegateXmlDoc = DelegateXmlDoc;
			var docs = null == delegateXmlDoc ? null : delegateXmlDoc.Returns;
			Contract.Assume(null != ReturnType);
			return new ReturnViewModel(ReturnType, docs);
		}

		public virtual IEnumerable<ParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<MethodParameterViewModel>>() != null);
			var delegateXmlDocs = XmlDoc as DelegateTypeDefinitionXmlDoc;
			return null == delegateXmlDocs
				? definitions.Select(item => new ParameterViewModel(item, null))
				: definitions.Select(item => new ParameterViewModel(item, delegateXmlDocs.DocsForParameter(item.Name)));
		}

	}
}
