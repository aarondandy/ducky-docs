using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Core.Overlays.XmlDoc;

namespace DandyDoc.Core.ViewModels
{
	public class MemberSummaryViewModel<TDefinition>
	{

		public MemberSummaryViewModel(
			TDefinition definition,
			string displayName,
			string cref,
			ParsedXmlNodeBase summary
		) {
			if(null == definition) throw new ArgumentNullException("definition");
			if(String.IsNullOrEmpty(displayName)) throw new ArgumentException("Invalid display name.","displayName");
			if(String.IsNullOrEmpty(cref)) throw new ArgumentException("Invalid cref","cref");
			Definition = definition;
			DisplayName = displayName;
			Cref = cref;
			Summary = summary;
		}

		public TDefinition Definition { get; private set; }
		public string DisplayName { get; private set; }
		public string Cref { get; private set; }
		public ParsedXmlNodeBase Summary { get; private set; }

	}
}
