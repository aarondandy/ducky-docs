using System;
using System.Linq;
using System.Web.Mvc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.MsdnLinks;
using Mono.Cecil;

namespace Mvc4WebDirectDocSample.Infrastructure
{
	public class TypeDefinitionLinkResolver
	{

		public TypeDefinitionLinkResolver(CrefOverlay crefOverlay, UrlHelper urlHelper, IMsdnLinkOverlay msdnLinkOverlay){
			CrefOverlay = crefOverlay;
			MsdnLinkOverlay = msdnLinkOverlay;
			UrlHelper = urlHelper;
		}

		public CrefOverlay CrefOverlay { get; private set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; private set; }

		public UrlHelper UrlHelper { get; private set; }

		public string GetLink(TypeReference reference){
			if (null == reference)
				return null;
			return GetLink(reference.Resolve());
		}

		public string GetLink(TypeDefinition definition){
			if (null == definition)
				return null;

			if (null != CrefOverlay && null != UrlHelper && CrefOverlay.AssemblyDefinitionCollection.ContainsDefinition(definition)){
				var cref = CrefOverlay.GetCref(definition);
				return UrlHelper.Action("Index", "Doc", new{cref});
			}

			if (null != MsdnLinkOverlay){
				var fullName = definition.FullName;
				if (fullName.StartsWith("System.") || fullName.StartsWith("Microsoft.")){
					try {
						var result = MsdnLinkOverlay.Search(fullName).FirstOrDefault();
						if (result != null)
							return MsdnLinkOverlay.GetUrl(result);
					}
					catch {
						;// exception monster!
					}
				}
			}

			return null;
		}

		public string GetLink(string cref){
			if (String.IsNullOrEmpty(cref))
				return null;

			if (null != CrefOverlay && null != UrlHelper){
				var typeDefinition = CrefOverlay.GetTypeDefinition(cref);
				if (null != typeDefinition) {
					return GetLink(typeDefinition);
				}
			}

			if (null != MsdnLinkOverlay){
				var fullName = new ParsedCref(cref).CoreName;
				if (fullName.StartsWith("System.") || fullName.StartsWith("Microsoft.")) {
					var result = MsdnLinkOverlay.Search(fullName).FirstOrDefault();
					if (result != null)
						return MsdnLinkOverlay.GetUrl(result);
				}
			}

			return null;
		}


	}
}