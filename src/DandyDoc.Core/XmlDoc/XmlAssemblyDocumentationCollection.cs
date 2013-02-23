using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    public class XmlAssemblyDocumentationCollection : Collection<XmlAssemblyDocumentation>
    {

        public XmlAssemblyDocumentationCollection()
            : base() { }

        public XmlAssemblyDocumentationCollection(IEnumerable<XmlAssemblyDocumentation> items)
            : this(null == items ? new List<XmlAssemblyDocumentation>() : items.ToList()) { }

        public XmlAssemblyDocumentationCollection(IList<XmlAssemblyDocumentation> items)
            : base(items)
        {
            Contract.Requires(items != null);
        }

        public XmlElement GetMemberRawElement(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();
            return this
                .Select(x => x.GetMemberRawElement(cRef))
                .FirstOrDefault(x => x != null);
        }

        public virtual XmlDocMember GetMember(string cRef) {
            if (String.IsNullOrEmpty(cRef)) throw new ArgumentException("Invalid CRef.", "cRef");
            Contract.EndContractBlock();
            return this
                .Select(x => x.GetMember(cRef))
                .FirstOrDefault(x => x != null);
        }

    }
}
