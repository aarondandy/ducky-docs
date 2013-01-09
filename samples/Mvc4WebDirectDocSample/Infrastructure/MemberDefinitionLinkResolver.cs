using System;
using System.Linq;
using System.Web.Mvc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.MsdnLinks;
using Mono.Cecil;

namespace Mvc4WebDirectDocSample.Infrastructure
{
	public class MemberDefinitionLinkResolver
	{

		public MemberDefinitionLinkResolver(CrefOverlay crefOverlay, UrlHelper urlHelper, IMsdnLinkOverlay msdnLinkOverlay){
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
			return GetLink((IMemberDefinition)definition);
		}

		public string GetLink(IMemberDefinition definition){
			if (null == definition)
				return null;

			if (null != CrefOverlay && null != UrlHelper && CrefOverlay.AssemblyDefinitionCollection.ContainsDefinition(definition as MemberReference)) {
				var cref = CrefOverlay.GetCref(definition);
				return UrlHelper.Action("Index", "Doc", new { cref });
			}

			if (null != MsdnLinkOverlay){
				string fullName;
				fullName = null != CrefOverlay
					? CrefOverlay.GetCref(definition, true)
					: definition.FullName;
				if (!String.IsNullOrEmpty(fullName) && (fullName.StartsWith("System.") || fullName.StartsWith("Microsoft."))) {
					try {
						var result = MsdnLinkOverlay.Search(fullName).FirstOrDefault();
						if (result != null)
							return MsdnLinkOverlay.GetUrl(result);
					}
					catch {
						; // exception monster!
					}
				}
			}

			return null;

		}

		public string GetLink(string cref){
			if (String.IsNullOrEmpty(cref))
				return null;

			if (null != CrefOverlay && null != UrlHelper){
				var memberDefinition = CrefOverlay.GetMemberDefinition(cref);
				if (null != memberDefinition) {
					return GetLink(memberDefinition);
				}
			}

			if (null != MsdnLinkOverlay){
				var fullName = new ParsedCref(cref).CoreName;
				if (fullName.StartsWith("System.") || fullName.StartsWith("Microsoft.")) {
					try {
						var result = MsdnLinkOverlay.Search(fullName).FirstOrDefault();
						if (result != null)
							return MsdnLinkOverlay.GetUrl(result);
					}
					catch {
						; // exception monster!
					}
				}
			}

			return null;
		}


	}
}