using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class DescriptionListComplexText : ComplexTextList<DescriptionListComplexText.ListItem>
	{

		public class ListItem : IComplexTextNode
		{

			public ListItem(bool isHeader, IComplexTextNode term, IComplexTextNode description){
				IsHeader = isHeader;
				Term = term;
				Description = description;
			}

			public bool IsHeader { get; private set; }

			public IComplexTextNode Term { get; private set; }

			public IComplexTextNode Description { get; private set; }
		}

		public DescriptionListComplexText(string listType, IList<ListItem> items)
			: base(items)
		{
			Contract.Requires(items != null);
			ListType = listType ?? "Bullets";
		}

		public string ListType { get; private set; }

	}
}
