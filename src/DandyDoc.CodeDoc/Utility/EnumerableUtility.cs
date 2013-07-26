using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.CodeDoc.Utility
{
    internal static class EnumerableUtility
    {

        public static T? FirstSetNullableOrDefault<T>(this IEnumerable<T?> items) where T : struct {
            Contract.Requires(items != null);
            return items.FirstOrDefault(x => x.HasValue);
        }

    }
}
