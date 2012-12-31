using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.Navigation;
using DandyDoc.Overlays.XmlDoc;

namespace DandyDoc.ViewModels
{
	public class TypeNavigationViewModel
	{

		private readonly Lazy<ReadOnlyCollection<TypeNavigationNamespaceViewModelcs>> _exposedNamespaceViewModels;

		public TypeNavigationViewModel(NavigationOverlay navigationOverlay, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == navigationOverlay) throw new ArgumentNullException("navigationOverlay");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			NavigationOverlay = navigationOverlay;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_exposedNamespaceViewModels = new Lazy<ReadOnlyCollection<TypeNavigationNamespaceViewModelcs>>(CreateExposedNamespaceViewModels);
		}

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public NavigationOverlay NavigationOverlay { get; private set; }

		private ReadOnlyCollection<TypeNavigationNamespaceViewModelcs> CreateExposedNamespaceViewModels() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<TypeNavigationNamespaceViewModelcs>>() != null);
			var viewModels = NavigationOverlay.Namespaces
				.Select(x => new TypeNavigationNamespaceViewModelcs(x, XmlDocOverlay, CrefOverlay))
				.Where(x => x.ExposedTypeViewModels.Count > 0)
				.OrderBy(x => x.FullName)
				.ToList();
			return new ReadOnlyCollection<TypeNavigationNamespaceViewModelcs>(viewModels);
		}

		public IList<TypeNavigationNamespaceViewModelcs> ExposedNamespaceViewModels{
			get{
				Contract.Ensures(Contract.Result<IList<TypeNavigationNamespaceViewModelcs>>() != null);
				return _exposedNamespaceViewModels.Value;
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
			Contract.Invariant(null != NavigationOverlay);
		}

	}
}
