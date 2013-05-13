using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace DandyDoc.XmlDoc
{
    public class XmlDocMember : XmlDocElement
    {

        private struct GutsData
        {
            public XmlDocElement FirstSummary;
            public IList<XmlDocNode> AllSummaryContents;
            public XmlDocElement FirstReturnsSummary;
            public IList<XmlDocNode> AllReturnsSummary;
            public XmlDocElement FirstValueSummary;
            public IList<XmlDocNode> AllValueSummary;
            public IList<XmlDocElement> RemarksElements;
            public IList<XmlDocElement> ExampleElements;
            public IList<XmlDocRefElement> PermissionElements;
            public IList<XmlDocRefElement> ExceptionElements;
            public IList<XmlDocRefElement> SeeAlsoElements;
            public IList<XmlDocNameElement> ParameterSummaries;
            public IList<XmlDocNameElement> TypeParameterSummaries;
            public XmlDocMember Getter;
            public XmlDocMember Setter;

            public XmlDocElement PureElement;
            public IList<XmlDocContractElement> RequiresElements;
            public IList<XmlDocContractElement> EnsuresElements;
            public IList<XmlDocContractElement> InvariantElements;
        }

        private static XmlDocNode Substitute(XmlDocNode node) {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<XmlDocNode>() != null);
            var xmlDocElement = node as XmlDocElement;
            if (xmlDocElement != null) {
                var elementName = xmlDocElement.Node.Name;
                if ("GETTER".Equals(elementName, StringComparison.OrdinalIgnoreCase)
                    || "SETTER".Equals(elementName, StringComparison.OrdinalIgnoreCase)) {
                    return new XmlDocMember(xmlDocElement.Element, node.Children);
                }
            }
            return node;
        }

        private readonly GutsData _guts;

        public XmlDocMember(XmlElement element, IEnumerable<XmlDocNode> children)
            : base(element, children.Select(Substitute)) {
            Contract.Requires(element != null);
            Contract.Requires(children != null);
            Contract.Requires(Contract.ForAll(children, x => x != null));
            _guts = CreateGuts();
        }

        private GutsData CreateGuts() {
            var result = new GutsData();
            var summaryContents = new List<XmlDocNode>();
            var returnsContents = new List<XmlDocNode>();
            var valueContents = new List<XmlDocNode>();
            var remarksElements = new List<XmlDocElement>();
            var examplesElements = new List<XmlDocElement>();
            var exceptionElements = new List<XmlDocRefElement>();
            var permissionElements = new List<XmlDocRefElement>();
            var seeAlsoElements = new List<XmlDocRefElement>();
            var parameterSummaries = new List<XmlDocNameElement>();
            var typeParameterSummaries = new List<XmlDocNameElement>();
            var requiresElements = new List<XmlDocContractElement>();
            var ensuresElements = new List<XmlDocContractElement>();
            var invariantElements = new List<XmlDocContractElement>();
            foreach (var child in Children.OfType<XmlDocElement>()) {
                if ("SUMMARY".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (child.HasChildren) {
                        if (result.FirstSummary == null)
                            result.FirstSummary = child;
                        summaryContents.AddRange(child.Children);
                    }
                }
                else if ("RETURNS".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (child.HasChildren) {
                        if (result.FirstReturnsSummary == null)
                            result.FirstReturnsSummary = child;
                        returnsContents.AddRange(child.Children);
                    }
                }
                else if ("VALUE".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (child.HasChildren) {
                        if (result.FirstValueSummary == null)
                            result.FirstValueSummary = child;
                        valueContents.AddRange(child.Children);
                    }
                }
                else if ("REMARKS".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (child.HasChildren)
                        remarksElements.Add(child);
                }
                else if ("EXAMPLE".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (child.HasChildren)
                        examplesElements.Add(child);
                }
                else if ("PURE".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                    if (result.PureElement == null)
                        result.PureElement = child;
                }
                else if (child is XmlDocMember) {
                    if ("GETTER".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                        if (child.HasChildren && result.Getter == null)
                            result.Getter = (XmlDocMember)child;
                    }
                    else if ("SETTER".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                        if (child.HasChildren && result.Setter == null)
                            result.Setter = (XmlDocMember)child;
                    }
                }
                else if (child is XmlDocContractElement) {
                    if ("REQUIRES".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                        requiresElements.Add((XmlDocContractElement)child);
                    }
                    else if ("ENSURES".Equals(child.Name, StringComparison.OrdinalIgnoreCase) || "ENSURESONTHROW".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                        ensuresElements.Add((XmlDocContractElement)child);
                    }
                    else if ("INVARIANT".Equals(child.Name, StringComparison.OrdinalIgnoreCase)) {
                        invariantElements.Add((XmlDocContractElement)child);
                    }
                }
                else if (child is XmlDocRefElement) {
                    if ("EXCEPTION".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        exceptionElements.Add((XmlDocRefElement)child);
                    else if ("PERMISSION".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        permissionElements.Add((XmlDocRefElement)child);
                    else if ("SEEALSO".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        seeAlsoElements.Add((XmlDocRefElement)child);
                }
                else if (child is XmlDocNameElement) {
                    if ("PARAM".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        parameterSummaries.Add((XmlDocNameElement)child);
                    else if ("TYPEPARAM".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        typeParameterSummaries.Add((XmlDocNameElement)child);
                }
            }
            result.AllSummaryContents = new ReadOnlyCollection<XmlDocNode>(summaryContents.ToArray());
            result.AllReturnsSummary = new ReadOnlyCollection<XmlDocNode>(returnsContents.ToArray());
            result.AllValueSummary = new ReadOnlyCollection<XmlDocNode>(valueContents.ToArray());
            result.RemarksElements = new ReadOnlyCollection<XmlDocElement>(remarksElements.ToArray());
            result.ExampleElements = new ReadOnlyCollection<XmlDocElement>(examplesElements.ToArray());
            result.ExceptionElements = new ReadOnlyCollection<XmlDocRefElement>(exceptionElements.ToArray());
            result.PermissionElements = new ReadOnlyCollection<XmlDocRefElement>(permissionElements.ToArray());
            result.SeeAlsoElements = new ReadOnlyCollection<XmlDocRefElement>(seeAlsoElements.ToArray());
            result.ParameterSummaries = new ReadOnlyCollection<XmlDocNameElement>(parameterSummaries.ToArray());
            result.TypeParameterSummaries = new ReadOnlyCollection<XmlDocNameElement>(typeParameterSummaries.ToArray());
            result.RequiresElements = new ReadOnlyCollection<XmlDocContractElement>(requiresElements.ToArray());
            result.EnsuresElements = new ReadOnlyCollection<XmlDocContractElement>(ensuresElements.ToArray());
            result.InvariantElements = new ReadOnlyCollection<XmlDocContractElement>(invariantElements.ToArray());
            return result;
        }

        public XmlDocElement SummaryElement {
            get { return _guts.FirstSummary; }
        }

        public IList<XmlDocNode> SummaryContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
                return _guts.AllSummaryContents;
            }
        }

        public bool HasSummaryContents {
            get { return SummaryContents.Count > 0; }
        }

        public XmlDocElement PureElement {
            get { return _guts.PureElement; }
        }

        public bool HasPureElement {
            get { return PureElement != null; }
        }

        public XmlDocElement ReturnsElement {
            get { return _guts.FirstReturnsSummary; }
        }

        public IList<XmlDocNode> ReturnsContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
                return _guts.AllReturnsSummary;
            }
        }

        public bool HasReturnsContents {
            get { return ReturnsContents.Count > 0; }
        }

        public XmlDocElement ValueElement {
            get { return _guts.FirstValueSummary; }
        }

        public IList<XmlDocNode> ValueContents {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNode>>(), x => x != null));
                return _guts.AllValueSummary;
            }
        }

        public bool HasValueContents {
            get { return ValueContents.Count > 0; }
        }

        public IList<XmlDocElement> RemarksElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return _guts.RemarksElements;
            }
        }

        public bool HasRemarksElements { get { return RemarksElements.Count > 0; } }

        public IList<XmlDocElement> ExampleElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocElement>>(), x => x != null));
                return _guts.ExampleElements;
            }
        }

        public bool HasExampleElements { get { return ExampleElements.Count > 0; } }

        public IList<XmlDocRefElement> ExceptionElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return _guts.ExceptionElements;
            }
        }

        public bool HasExceptionElements { get { return ExceptionElements.Count > 0; } }

        public IList<XmlDocRefElement> PermissionElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return _guts.PermissionElements;
            }
        }

        public bool HasPermissionElements { get { return PermissionElements.Count > 0; } }

        public IList<XmlDocRefElement> SeeAlsoElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocRefElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocRefElement>>(), x => x != null));
                return _guts.SeeAlsoElements;
            }
        }

        public bool HasSeeAlsoElements { get { return SeeAlsoElements.Count > 0; } }

        public IList<XmlDocNameElement> ParameterSummaries {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNameElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNameElement>>(), x => x != null));
                return _guts.ParameterSummaries;
            }
        }

        public bool HasParameterSummaries { get { return ParameterSummaries.Count > 0; } }

        public XmlDocNameElement GetParameterSummary(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
            Contract.EndContractBlock();
            return ParameterSummaries.FirstOrDefault(x => x.TargetName == parameterName);
        }

        public IList<XmlDocNameElement> TypeParameterSummaries {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocNameElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocNameElement>>(), x => x != null));
                return _guts.TypeParameterSummaries;
            }
        }

        public bool HasTypeParameterSummaries { get { return TypeParameterSummaries.Count > 0; } }

        public XmlDocNameElement GetTypeParameterSummary(string parameterName) {
            if (String.IsNullOrEmpty(parameterName)) throw new ArgumentException("Invalid parameter name.", "parameterName");
            Contract.EndContractBlock();
            return TypeParameterSummaries.FirstOrDefault(x => x.TargetName == parameterName);
        }

        public IList<XmlDocContractElement> RequiresElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocContractElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocContractElement>>(), x => x != null));
                return _guts.RequiresElements;
            }
        }

        public bool HasRequiresElements { get { return RequiresElements.Count > 0; } }

        public IList<XmlDocContractElement> EnsuresElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocContractElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocContractElement>>(), x => x != null));
                return _guts.EnsuresElements;
            }
        }

        public bool HasEnsuresElements { get { return EnsuresElements.Count > 0; } }

        public IList<XmlDocContractElement> InvariantElements {
            get {
                Contract.Ensures(Contract.Result<IList<XmlDocContractElement>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<XmlDocContractElement>>(), x => x != null));
                return _guts.InvariantElements;
            }
        }

        public bool HasInvariantElements { get { return InvariantElements.Count > 0; } }

        public XmlDocMember GetterElement {
            get { return _guts.Getter; }
        }

        public bool HasGetterElement {
            get { return GetterElement != null; }
        }

        public XmlDocMember SetterElement {
            get { return _guts.Setter; }
        }

        public bool HasSetterElement {
            get { return SetterElement != null; }
        }

    }
}
