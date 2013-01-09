using System.Diagnostics.Contracts;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using System;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public class ExceptionViewModel
	{

		private static readonly DisplayNameOverlay ShortNameOverlay = new DisplayNameOverlay();

		public ExceptionViewModel(ParsedXmlException exceptionXml){
			if(null == exceptionXml) throw new ArgumentNullException("exceptionXml");
			Contract.EndContractBlock();
			ExceptionXml = exceptionXml;
		}

		public ParsedXmlException ExceptionXml { get; private set; }

		public bool HasXmlComments{
			get { return ExceptionXml.Children.Count > 0; }
		}

		public string ShortName{
			get{
				var target = ExceptionXml.CrefTarget;
				if(null != target)
					return ShortNameOverlay.GetDisplayName(target);
				if (!String.IsNullOrEmpty(ExceptionXml.CRef))
					return new ParsedCref(ExceptionXml.CRef).CoreName;
				return null;
			}
		}

		public TypeReference ExceptionTypeReference{
			get { return ExceptionXml.CrefTarget as TypeReference; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(ExceptionXml != null);
		}

	}
}
