using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.ExternalVisibility;
using DandyDoc.Core.Overlays.Navigation;
using DandyDoc.Core.Overlays.XmlDoc;

namespace DandyDoc.Core.ViewModels
{
	public class TypeNavigationNamespaceViewModelcs
	{
		private readonly Lazy<ReadOnlyCollection<TypeViewModel>> _exposedTypeViewModels;

		internal TypeNavigationNamespaceViewModelcs(NavigationOverlayCompositeNamespace overlayCompositeNamespace, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == overlayCompositeNamespace) throw new ArgumentNullException("overlayCompositeNamespace");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			OverlayCompositeNamespace = overlayCompositeNamespace;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_exposedTypeViewModels = new Lazy<ReadOnlyCollection<TypeViewModel>>(CreateExposedTypeViewModels, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public NavigationOverlayCompositeNamespace OverlayCompositeNamespace { get; private set; }

		public string FullName{
			get{
				Contract.Ensures(null != Contract.Result<string>());
				return OverlayCompositeNamespace.Namespace;
			}
		}

		private ReadOnlyCollection<TypeViewModel> CreateExposedTypeViewModels() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<TypeViewModel>>() != null);
			var viewModels = OverlayCompositeNamespace.Types
				.Where(x => x.IsExternallyVisible())
				.Select(x => new TypeViewModel(x, XmlDocOverlay, CrefOverlay))
				.OrderBy(x => x.ShortName)
				.ToList();
			return new ReadOnlyCollection<TypeViewModel>(viewModels);
		}

		public IList<TypeViewModel> ExposedTypeViewModels {
			get {
				Contract.Ensures(Contract.Result<IList<TypeViewModel>>() != null);
				return _exposedTypeViewModels.Value;
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
			Contract.Invariant(null != OverlayCompositeNamespace);
		}

	}
}
