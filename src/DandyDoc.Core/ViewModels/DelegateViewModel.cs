using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			_parameters = new Lazy<ReadOnlyCollection<ParameterDefinition>>(() => new ReadOnlyCollection<ParameterDefinition>(Definition.GetDelegateTypeParameters()));
			_returnType = new Lazy<TypeReference>(() => Definition.GetDelegateReturnType());
		}

		new DelegateTypeDefinitionXmlDoc DelegateXmlDoc {
			get { return XmlDoc as DelegateTypeDefinitionXmlDoc; }
		}

		public bool HasReturn {
			get {
				var returnType = ReturnType;
				return null != returnType && returnType.FullName != "System.Void";
			}
		}

		public TypeReference ReturnType {
			get { return _returnType.Value; }
		}

		public IList<ParsedXmlException> Exceptions {
			get { return null == DelegateXmlDoc ? null : DelegateXmlDoc.Exceptions; }
		}

		public bool HasExceptions {
			get {
				var exceptions = Exceptions;
				return null != exceptions && exceptions.Count > 0;
			}
		}

		public DelegateReturnViewModel CreateReturnViewModel() {
			if (!HasReturn) throw new InvalidOperationException("Method does not return a value.");
			Contract.EndContractBlock();
			var delegateXmlDoc = DelegateXmlDoc;
			var docs = null == delegateXmlDoc ? null : delegateXmlDoc.Returns;
			Contract.Assume(null != ReturnType);
			return new DelegateReturnViewModel(ReturnType, this, docs);
		}

		public IEnumerable<DelegateParameterViewModel> CreateParameterViewModels(IEnumerable<ParameterDefinition> definitions) {
			if (null == definitions) throw new ArgumentNullException("definitions");
			Contract.Ensures(Contract.Result<IEnumerable<ParameterViewModel>>() != null);
			var delegateXmlDocs = XmlDoc as DelegateTypeDefinitionXmlDoc;
			return definitions.Select(item => {
				var docs = null == delegateXmlDocs ? null : delegateXmlDocs.DocsForParameter(item.Name);
				return new DelegateParameterViewModel(item, this, docs);
			});
		}

	}
}
