using System;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class SimpleFlairTag : IFlairTag
	{

		public SimpleFlairTag(string iconId, string category, string description)
			: this(iconId, category, new StandardComplexText(description))
		{
			if(String.IsNullOrEmpty(description)) throw new ArgumentException("Description is required.", "description");
			Contract.Requires(!String.IsNullOrEmpty(iconId));
			Contract.Requires(!String.IsNullOrEmpty(category));
		}

		public SimpleFlairTag(string iconId, string category, IComplexTextNode description){
			if(String.IsNullOrEmpty(iconId)) throw new ArgumentException("Icon ID is required.","iconId");
			if(String.IsNullOrEmpty(category)) throw new ArgumentException("Category is required.", "category");
			if(description == null) throw new ArgumentNullException("description");
			Contract.EndContractBlock();
			IconId = iconId;
			Category = category;
			Description = description;
		}

		public string IconId { get; private set; }

		public string Category { get; private set; }

		public IComplexTextNode Description { get; private set; }
	}
}
