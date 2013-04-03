using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    [Obsolete]
    public class CRefKeyedCollection<T> : KeyedCollection<CRefIdentifier,T> where T:ICodeDocEntity
    {
        protected override CRefIdentifier GetKeyForItem(T item) {
            return item.CRef;
        }

        public CRefKeyedCollection() { }

        public CRefKeyedCollection(IEnumerable<T> items) {
            if (items != null) {
                foreach (var item in items) {
                    Add(item);
                }
            }
        }
    }
}
