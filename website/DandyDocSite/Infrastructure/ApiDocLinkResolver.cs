using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Mvc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.MsdnLinks;
using DandyDoc.SimpleModels;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDocSite.Infrastructure
{
	public class ApiDocLinkResolver
	{

		public UrlHelper UrlHelper { get; set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; set; }

		public ISimpleModelRepository LocalRepository { get; set; }

		public virtual string GetLocalCrefUrl(string cRef) {
			return UrlHelper.Action("Api", "Docs", new {cRef});
		}

		public virtual string GetUrl(string cRef) {
			if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef","cRef");
			Contract.EndContractBlock();

			var model = LocalRepository.GetModelFromCref(cRef);
			if (null != model) {
				return GetLocalCrefUrl(model.CRef);
			}

			return GetUrlForExternalCref(cRef);
		}

		public virtual string GetUrl(ISimpleMemberPointerModel pointerModel) {
			if(pointerModel == null) throw new ArgumentException("pointerModel");
			Contract.EndContractBlock();

			var referencePointerModel = pointerModel as ReferenceSimpleMemberPointer;
			if (referencePointerModel != null) {
				var result = GetUrlForMemberReference(referencePointerModel);
				if (!String.IsNullOrEmpty(result))
					return result;
			}

			return GetUrlForExternalCref(pointerModel.CRef);
		}

		protected virtual string GetUrlForExternalCref(string cRef) {
			if (String.IsNullOrEmpty(cRef))
				return null;

			var parsedCref = new ParsedCref(cRef);
			if (parsedCref.CoreName.StartsWith("System") || parsedCref.CoreName.StartsWith("Microsoft")) {
				if (MsdnLinkOverlay != null) {
					try {
						var result = MsdnLinkOverlay.Search(cRef)
							.Select(x => MsdnLinkOverlay.GetUrl(x))
							.FirstOrDefault(x => !String.IsNullOrEmpty(x));
						if (!String.IsNullOrEmpty(result))
							return result;
					}
					catch {
						return null; // exception monster!
					}
				}
				return null;
			}
			if (parsedCref.CoreName.StartsWith("Mono")) {
				var parsedCrefLastDot = parsedCref.CoreName.LastIndexOf('.');
				if (parsedCrefLastDot > 0) {
					var namespacePart = parsedCref.CoreName.Substring(0, parsedCrefLastDot);
					var typenamePart = parsedCref.CoreName.Substring(parsedCrefLastDot + 1);
					return String.Format("https://github.com/jbevain/cecil/blob/master/{0}/{1}.cs", namespacePart, typenamePart);
				}
			}

			return null;
		}

		protected virtual string GetUrlForMemberReference(ReferenceSimpleMemberPointer referenceModel) {
			if (referenceModel == null)
				return null;

			if (LocalRepository != null) {
				string cRef = null;
				var reference = referenceModel.Reference;
				if (reference is IMemberDefinition) {
					cRef = referenceModel.CRef;
				}
				else if(reference is TypeReference) {
					try {
						var resolved = ((TypeReference)reference).Resolve();
						if (null != resolved) {
							cRef = CrefOverlay.GetDefaultCref((MemberReference)resolved);
						}
					}
					catch {
						cRef = null;
					}
				}

				var model = LocalRepository.GetModelFromCref(cRef ?? referenceModel.CRef);
				if (null != model) {
					return GetLocalCrefUrl(model.CRef);
				}
				else if(!String.IsNullOrEmpty(cRef)) {
					return GetUrlForExternalCref(cRef);
				}
			}

			return null;
		}

	}
}