using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.Core.Utility
{
	internal static class CollectionUtility
	{

		internal static readonly ReadOnlyCollection<string> EmptyStringCollection = Array.AsReadOnly(new string[0]);

		public static List<TTo> ConvertAll<TFrom, TTo>(this IList<TFrom> from, Func<TFrom,TTo> conversion) {
			if(null == from) throw new ArgumentNullException("from");
			if(null == conversion) throw new ArgumentNullException("conversion");
			Contract.Ensures(Contract.Result<List<TTo>>() != null);

			var innerList = new List<TTo>(from.Count);
			innerList.AddRange(from.Select(conversion));
			return innerList;
		}

	}
}
