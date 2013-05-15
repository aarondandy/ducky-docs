using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace DandyDoc.CodeDoc.Utility
{
    internal static class ListUtility
    {

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) {
            Contract.Requires(list != null);
            return new ReadOnlyCollection<T>(list);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array) {
            Contract.Requires(array != null);
            return Array.AsReadOnly(array);
        }

    }
}
