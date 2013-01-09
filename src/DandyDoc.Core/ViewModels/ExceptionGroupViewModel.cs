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
	public class ExceptionGroupViewModel
	{

		public ExceptionGroupViewModel(IList<ExceptionViewModel> exceptions) {
			if(null == exceptions) throw new ArgumentNullException("exceptions");
			if(exceptions.Count == 0) throw new ArgumentException("At least 1 exception is required to create a group.");
			Contract.EndContractBlock();
			var firstException = exceptions[0];
			for (int i = 1; i < exceptions.Count;i++)
				if(firstException.ExceptionXml.CRef != exceptions[i].ExceptionXml.CRef)
					throw new ArgumentException("All exceptions must be of the same CRef type.", "exceptions");
			Exceptions = new ReadOnlyCollection<ExceptionViewModel>(exceptions);
		}

		public IList<ExceptionViewModel> Exceptions { get; private set; }

		public string ExceptionTypeCref { get { return Exceptions.First().ExceptionXml.CRef; } }

		public TypeReference ExceptionTypeReference {
			get {
				return Exceptions
					.Select(x => x.ExceptionTypeReference)
					.FirstOrDefault(x => null != x);
			}
		}

		public string ShortName {
			get {
				return Exceptions
					.Select(x => x.ShortName)
					.FirstOrDefault(x => !String.IsNullOrEmpty(x));
			}
		}

		public bool HasXmlConditions {
			get { return XmlConditions.Count > 0; }
		}

		public IList<ParsedXmlException> XmlConditions {
			get {
				Contract.Ensures(Contract.Result<IList<ParsedXmlException>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<ParsedXmlException>>(), x => null != x));
				return Exceptions
					.Where(x => x.HasXmlComments && !x.ExceptionXml.HasRelatedEnsures)
					.Select(x => x.ExceptionXml)
					.Where(x => null != x)
					.ToList();
			}
		}

		public bool HasXmlEnsures {
			get { return XmlEnsures.Count > 0; }
		}

		public IList<ParsedXmlException> XmlEnsures {
			get {
				Contract.Ensures(Contract.Result<IList<ParsedXmlException>>() != null);
				Contract.Ensures(Contract.ForAll(Contract.Result<IList<ParsedXmlException>>(), x => null != x));
				return Exceptions
					.Where(x => x.HasXmlComments && x.ExceptionXml.HasRelatedEnsures)
					.Select(x => x.ExceptionXml)
					.Where(x => null != x)
					.ToList();
			}
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(null != Exceptions);
			Contract.Invariant(Exceptions.Count != 0);
			Contract.Invariant(Contract.ForAll(Exceptions, ex => ex.ExceptionXml.CRef == Exceptions[0].ExceptionXml.CRef));
		}

	}
}
