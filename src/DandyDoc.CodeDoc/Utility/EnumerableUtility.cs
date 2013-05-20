using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DandyDoc.CodeDoc.Utility
{
    internal static class EnumerableUtility
    {

        public static T? FirstSetNullableOrDefault<T>(this IEnumerable<T?> items) where T : struct {
            return items.FirstOrDefault(x => x.HasValue);
        }

    }
}
