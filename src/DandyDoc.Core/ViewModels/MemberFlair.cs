using System;
using System.Collections.ObjectModel;

namespace DandyDoc.ViewModels
{
	public class MemberFlair
	{

		public static readonly ReadOnlyCollection<MemberFlair> EmptyList = Array.AsReadOnly(new MemberFlair[0]);

		public MemberFlair(string id, string categoryName, string description) {
			if(String.IsNullOrEmpty(id)) throw new ArgumentException("Invalid ID.", "id");
			if(String.IsNullOrEmpty(categoryName)) throw new ArgumentException("Invalid category name.","categoryName");
			if(String.IsNullOrEmpty(description)) throw new ArgumentException("A valid description is required.", "description");
			Id = id;
			CategoryName = categoryName;
			Description = description;
		}

		public string Id { get; private set; }
		public string CategoryName { get; private set; }
		public string Description { get; private set; }

	}
}
