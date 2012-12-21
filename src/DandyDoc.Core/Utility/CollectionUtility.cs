using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.Core.Utility
{
	internal static class CollectionUtility
	{

		public static List<TTo> ConvertAll<TFrom, TTo>(this IList<TFrom> from, Func<TFrom,TTo> conversion) {
			if(null == from) throw new ArgumentNullException("from");
			Contract.Ensures(Contract.Result<List<TTo>>() != null);
			var innerList = new List<TTo>(from.Count);
			innerList.AddRange(from.Select(conversion));
			return innerList;
		}

	}
}
