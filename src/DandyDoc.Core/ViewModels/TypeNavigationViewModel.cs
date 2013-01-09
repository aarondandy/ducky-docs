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

		private readonly Lazy<ReadOnlyCollection<TypeNavigationNamespaceViewModel>> _exposedNamespaceViewModels;

		public TypeNavigationViewModel(NavigationOverlay navigationOverlay, XmlDocOverlay xmlDocOverlay, CrefOverlay crefOverlay = null) {
			if (null == navigationOverlay) throw new ArgumentNullException("navigationOverlay");
			if (null == xmlDocOverlay) throw new ArgumentNullException("xmlDocOverlay");
			Contract.EndContractBlock();
			NavigationOverlay = navigationOverlay;
			XmlDocOverlay = xmlDocOverlay;
			CrefOverlay = crefOverlay ?? xmlDocOverlay.CrefOverlay;
			_exposedNamespaceViewModels = new Lazy<ReadOnlyCollection<TypeNavigationNamespaceViewModel>>(CreateExposedNamespaceViewModels);
		}

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public NavigationOverlay NavigationOverlay { get; private set; }

		private ReadOnlyCollection<TypeNavigationNamespaceViewModel> CreateExposedNamespaceViewModels() {
			Contract.Ensures(Contract.Result<ReadOnlyCollection<TypeNavigationNamespaceViewModel>>() != null);
			var viewModels = NavigationOverlay.Namespaces
				.Select(x => new TypeNavigationNamespaceViewModel(x, XmlDocOverlay, CrefOverlay))
				.Where(x => x.ExposedTypeViewModels.Count > 0)
				.OrderBy(x => x.FullName)
				.ToList();
			return new ReadOnlyCollection<TypeNavigationNamespaceViewModel>(viewModels);
		}

		public IList<TypeNavigationNamespaceViewModel> ExposedNamespaceViewModels{
			get{
				Contract.Ensures(Contract.Result<IList<TypeNavigationNamespaceViewModel>>() != null);
				return _exposedNamespaceViewModels.Value;
			}
		}

		public TypeNavigationNamespaceViewModel GetExposedNamespaceViewModel(string nsName) {
			if (String.IsNullOrEmpty(nsName)) {
				return ExposedNamespaceViewModels.FirstOrDefault(n => String.IsNullOrEmpty(nsName))
					?? ExposedNamespaceViewModels.FirstOrDefault(n => n.FullName == "::");
			}
			if ("::".Equals(nsName)) {
				return ExposedNamespaceViewModels.FirstOrDefault(n => n.FullName == "::")
					?? ExposedNamespaceViewModels.FirstOrDefault(n => String.IsNullOrEmpty(nsName));
			}
			return ExposedNamespaceViewModels.FirstOrDefault(n => n.FullName == nsName);
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != XmlDocOverlay);
			Contract.Invariant(null != CrefOverlay);
			Contract.Invariant(null != NavigationOverlay);
		}

	}
}
