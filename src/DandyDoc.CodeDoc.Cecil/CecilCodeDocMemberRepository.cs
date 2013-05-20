using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.Cecil;
using DandyDoc.CodeDoc.Utility;
using DandyDoc.DisplayName;
using DandyDoc.ExternalVisibility;
using DandyDoc.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// Generates code doc member models from Cecil members.
    /// </summary>
    public class CecilCodeDocMemberRepository : CodeDocMemberRepositoryBase
    {

        private static readonly StandardCecilDisplayNameGenerator RegularTypeDisplayNameOverlay
            = new StandardCecilDisplayNameGenerator {
                ShowTypeNameForMembers = false
            };

        private static readonly StandardCecilDisplayNameGenerator NestedTypeDisplayNameOverlay
            = new StandardCecilDisplayNameGenerator {
                ShowTypeNameForMembers = true
            };

        private static readonly StandardCecilDisplayNameGenerator FullTypeDisplayNameOverlay
            = new StandardCecilDisplayNameGenerator {
                ShowTypeNameForMembers = true,
                IncludeNamespaceForTypes = true
            };

        private static CRefIdentifier GetCRefIdentifier(MemberReference memberReference) {
            Contract.Requires(memberReference != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(CecilCRefGenerator.WithPrefix.GetCRef(memberReference));
        }

        private static CRefIdentifier GetCRefIdentifier(AssemblyDefinition assembly) {
            Contract.Requires(assembly != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(CecilCRefGenerator.WithPrefix.GetCRef(assembly));
        }

        /// <summary>
        /// Creates a new Cecil code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into Cecil members.</param>
        public CecilCodeDocMemberRepository(CecilCRefLookup cRefLookup)
            : this(cRefLookup, null) {
            Contract.Requires(cRefLookup != null);
        }

        /// <summary>
        /// Creates a new Cecil code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into Cecil members.</param>
        /// <param name="xmlDocs">The related XML documentation files for the members.</param>
        public CecilCodeDocMemberRepository(CecilCRefLookup cRefLookup, params XmlAssemblyDocument[] xmlDocs)
            : this(cRefLookup, (IEnumerable<XmlAssemblyDocument>)xmlDocs) {
            Contract.Requires(cRefLookup != null);
        }

        /// <summary>
        /// Creates a new Cecil code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into Cecil members.</param>
        /// <param name="xmlDocs">The related XML documentation files for the members.</param>
        public CecilCodeDocMemberRepository(CecilCRefLookup cRefLookup, IEnumerable<XmlAssemblyDocument> xmlDocs)
            : base(xmlDocs)
        {
            if (cRefLookup == null) throw new ArgumentNullException("cRefLookup");
            Contract.EndContractBlock();
            CRefLookup = cRefLookup;

            var assemblyModels = new List<CodeDocSimpleAssembly>();
            var namespaceModels = new Dictionary<string, CodeDocSimpleNamespace>();

            foreach (var assembly in CRefLookup.Assemblies) {
                var assemblyShortName = assembly.Name.Name;
                var assemblyModel = new CodeDocSimpleAssembly(GetCRefIdentifier(assembly)) {
                    AssemblyFileName = Path.GetFileName(assembly.GetFilePath()),
                    Title = assemblyShortName,
                    ShortName = assemblyShortName,
                    FullName = assembly.Name.FullName,
                    NamespaceName = assemblyShortName,
                    SubTitle = "Assembly",
                    NamespaceCRefs = new List<CRefIdentifier>()
                };

                var assemblyTypeCRefs = new List<CRefIdentifier>();
                var assemblyNamespaceNames = new HashSet<string>();
                foreach (var type in assembly
                    .Modules
                    .SelectMany(m => m.Types)
                    .Where(t => !t.IsNested)
                    .Where(MemberFilter)
                ) {
                    var typeCRef = GetCRefIdentifier(type);
                    assemblyTypeCRefs.Add(typeCRef);
                    var namespaceName = type.Namespace;
                    if (String.IsNullOrWhiteSpace(namespaceName))
                        namespaceName = String.Empty;

                    CodeDocSimpleNamespace namespaceModel;
                    if (!namespaceModels.TryGetValue(namespaceName, out namespaceModel)) {
                        var namespaceTitle = String.IsNullOrEmpty(namespaceName) ? "global" : namespaceName;
                        namespaceModel = new CodeDocSimpleNamespace(new CRefIdentifier("N:" + namespaceName)) {
                            Title = namespaceTitle,
                            ShortName = namespaceTitle,
                            FullName = namespaceTitle,
                            NamespaceName = namespaceTitle,
                            SubTitle = "Namespace",
                            TypeCRefs = new List<CRefIdentifier>(),
                            AssemblyCRefs = new List<CRefIdentifier>()
                        };
                        namespaceModels.Add(namespaceName, namespaceModel);
                    }

                    namespaceModel.TypeCRefs.Add(typeCRef);
                    if (assemblyNamespaceNames.Add(namespaceName)) {
                        // this is the first time this assembly has seen this namespace
                        namespaceModel.AssemblyCRefs.Add(assemblyModel.CRef);
                        assemblyModel.NamespaceCRefs.Add(namespaceModel.CRef);
                    }
                }

                assemblyModel.TypeCRefs = new ReadOnlyCollection<CRefIdentifier>(assemblyTypeCRefs);
                assemblyModels.Add(assemblyModel);
            }

            // freeze the namespace & assembly collections
            foreach (var namespaceModel in namespaceModels.Values) {
                namespaceModel.AssemblyCRefs = namespaceModel.AssemblyCRefs.AsReadOnly();
            }
            foreach (var assemblyModel in assemblyModels) {
                assemblyModel.NamespaceCRefs = assemblyModel.NamespaceCRefs.AsReadOnly();
            }

            Assemblies = new ReadOnlyCollection<CodeDocSimpleAssembly>(assemblyModels.OrderBy(x => x.Title).ToArray());
            Namespaces = new ReadOnlyCollection<CodeDocSimpleNamespace>(namespaceModels.Values.OrderBy(x => x.Title).ToArray());
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
        }

        /// <summary>
        /// The code reference lookup.
        /// </summary>
        public CecilCRefLookup CRefLookup { get; private set; }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="memberReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(MemberReference memberReference) {
            if (memberReference == null)
                return false;
            if (memberReference.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (memberReference is MethodReference)
                return MemberFilter((MethodReference)memberReference);
            if (memberReference is FieldReference)
                return MemberFilter((FieldReference)memberReference);
            if (memberReference is EventReference)
                return MemberFilter((EventReference)memberReference);
            if (memberReference is PropertyReference)
                return MemberFilter((PropertyReference)memberReference);
            if (memberReference is TypeReference)
                return MemberFilter((TypeReference)memberReference);
            return memberReference.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="typeReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(TypeReference typeReference) {
            return typeReference != null
                && typeReference.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="methodReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected bool MemberFilter(MethodReference methodReference) {
            return MemberFilter(methodReference, false);
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="methodReference">The member to test.</param>
        /// <param name="isPropertyMethod">Indicates that the method is a property getter or setter.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(MethodReference methodReference, bool isPropertyMethod) {
            if (methodReference == null)
                return false;
            if (methodReference.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = methodReference.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;

            var methodDefinition = methodReference.ToDefinition();
            if (methodDefinition != null) {
                if (!isPropertyMethod && methodDefinition.IsSpecialName && !methodDefinition.IsConstructor && !methodDefinition.IsOperatorOverload())
                    return false;
                if (methodDefinition.IsFinalizer())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="fieldReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(FieldReference fieldReference) {
            if (fieldReference == null)
                return false;
            if (fieldReference.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = fieldReference.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;
            var fieldDefinition = fieldReference.ToDefinition();
            if (fieldDefinition != null) {
                if (fieldDefinition.IsSpecialName)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="eventReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(EventReference eventReference) {
            if (eventReference == null)
                return false;
            if (eventReference.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var eventDefinition = eventReference.ToDefinition();
            if (eventDefinition != null) {
                if (eventDefinition.IsSpecialName)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="propertyReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(PropertyReference propertyReference) {
            if (propertyReference == null)
                return false;
            if (propertyReference.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var propertyDefinition = propertyReference.ToDefinition();
            if (propertyDefinition != null) {
                if (propertyDefinition.IsSpecialName)
                    return false;
            }
            return true;
        }

        private CodeDocSimpleAssembly GetCodeDocAssembly(AssemblyDefinition assembly) {
            Contract.Requires(assembly != null);
            return GetCodeDocSimpleAssembly(GetCRefIdentifier(assembly));
        }

        private MemberReference GetMemberReferencePreferDefinition(CRefIdentifier cRef) {
            var memberReference = CRefLookup.GetMember(cRef);
            return (MemberReference)(memberReference.ToDefinition()) ?? memberReference;
        }

        /// <inheritdoc/>
        public override ICodeDocMember GetContentMember(CRefIdentifier cRef) {
            /*if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberReference = GetMemberReferencePreferDefinition(cRef);
            if (memberReference == null || !MemberFilter(memberReference))
                return null;

            return ConvertToModel(memberReference);*/
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override ICodeDocMember GetMemberModel(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberReference = GetMemberReferencePreferDefinition(cRef);
            if (memberReference == null || !MemberFilter(memberReference))
                return null;

            return ConvertToModel(memberReference);
        }

        /// <summary>
        /// Converts a Cecil member to a code doc model.
        /// </summary>
        /// <param name="memberReference">A Cecil member.</param>
        /// <returns>A code doc model for the given member.</returns>
        protected virtual CodeDocMemberContentBase ConvertToModel(MemberReference memberReference) {
            if(memberReference == null) throw new ArgumentNullException("memberReference");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            if (memberReference is TypeReference)
                return ConvertToModel((TypeReference)memberReference);
            if (memberReference is FieldReference)
                return ConvertToModel((FieldReference)memberReference);
            if (memberReference is MethodReference)
                return ConvertToModel((MethodReference)memberReference);
            if (memberReference is EventReference)
                return ConvertToModel((EventReference)memberReference);
            if (memberReference is PropertyReference)
                return ConvertToModel((PropertyReference)memberReference);
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override ICodeDocMember GetSimpleMember(CRefIdentifier cRef) {
            /*if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleNamespace(cRef);
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberReference = GetMemberReferencePreferDefinition(cRef);
            memberReference = (MemberReference)(memberReference.ToDefinition()) ?? memberReference;
            if (memberReference == null || !MemberFilter(memberReference))
                return null;

            return ConvertToSimpleModel(memberReference);*/
            throw new NotSupportedException();
        }

        private ICodeDocMember GetSimpleEntity(MemberReference memberReference) {
            Contract.Requires(memberReference != null);

            // TODO: check a pool of other repositories if not found in this one

            if (MemberFilter(memberReference))
                return ConvertToSimpleModel(memberReference);

            return CreateSimpleEntityPlaceholder(memberReference);
        }

        private ICodeDocMember CreateSimpleEntityPlaceholder(MemberReference memberReference) {
            Contract.Requires(memberReference != null);
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            var result = new CodeDocSimpleMember(GetCRefIdentifier(memberReference)) {
                ShortName = memberReference.Name,
                Title = memberReference.Name,
                SubTitle = String.Empty,
                FullName = memberReference.Name,
                NamespaceName = String.Empty,
            };
            ApplySimpleEntityAttributes(result, memberReference);
            return result;
        }

        /// <summary>
        /// Converts a Cecil member to a simple code doc model.
        /// </summary>
        /// <param name="memberReference">A Cecil member.</param>
        /// <returns>A code doc model for the given member.</returns>
        protected virtual ICodeDocMember ConvertToSimpleModel(MemberReference memberReference) {
            if(memberReference == null) throw new ArgumentNullException("memberReference");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            var cRef = GetCRefIdentifier(memberReference);
            var result = new CodeDocSimpleMember(cRef);
            ApplyStandardXmlDocs(result, cRef.FullCRef);
            if (memberReference is TypeReference)
                ApplyCommonAttributes(result, (TypeReference)memberReference);
            else if (memberReference is MethodReference)
                ApplyCommonAttributes(result, (MethodReference)memberReference);
            else if (memberReference is FieldReference)
                ApplyCommonAttributes(result, (FieldReference)memberReference);
            else if (memberReference is EventReference)
                ApplyCommonAttributes(result, (EventReference)memberReference);
            else if (memberReference is PropertyReference)
                ApplyCommonAttributes(result, (PropertyReference)memberReference);
            else
                throw new NotSupportedException();
            return result;
        }

        private void ApplyEntityAttributes(CodeDocParameter model, ParameterReference parameterReference) {
            Contract.Requires(model != null);
            if (parameterReference != null) {
                model.IsByRef = parameterReference.ParameterType.IsByReference;
                var parameterDefinition = parameterReference.ToDefinition();
                if (parameterDefinition != null) {
                    model.IsOut = parameterDefinition.IsOut;

                    model.NullRestricted = null;
                    if (parameterDefinition.HasCustomAttributes) {
                        foreach (var attribute in parameterDefinition.CustomAttributes) {
                            var constructorName = attribute.Constructor.Name;
                            if (constructorName == "CanBeNullAttribute") {
                                model.NullRestricted = false;
                                break;
                            }
                            if (constructorName == "NotNullAttribute") {
                                model.NullRestricted = true;
                                break;
                            }
                        }
                    }

                }
            }
        }

        private void ApplyEntityAttributes(CodeDocParameter model, TypeReference returnType, XmlDocMember xmlDocs) {
            Contract.Requires(model != null);
            ApplyEntityAttributes(model, null);
            if (!model.NullRestricted.HasValue && xmlDocs != null) {
                if (xmlDocs.HasEnsuresElements && xmlDocs.EnsuresElements.Any(c => c.EnsuresResultNotEverNull))
                    model.NullRestricted = true;
            }
            if (returnType != null) {
                model.IsReferenceType = !returnType.IsValueType;
            }
        }

        private void ApplyEntityAttributes(CodeDocParameter model, ParameterReference parameterInfo, XmlDocMember xmlDocs) {
            Contract.Requires(model != null);
            ApplyEntityAttributes(model, parameterInfo);
            if (!model.NullRestricted.HasValue && xmlDocs != null) {
                if (xmlDocs.HasRequiresElements && xmlDocs.RequiresElements.Any(c => c.RequiresParameterNotEverNull(parameterInfo.Name)))
                    model.NullRestricted = true;
            }
            if (parameterInfo != null) {
                model.IsReferenceType = !parameterInfo.ParameterType.IsValueType;
            }
        }

        private void ExpandCodeDocMethodXmlDocProperties(CodeDocMethod model) {
            if (model.XmlDocs != null) {
                if (model.XmlDocs.HasExceptionElements) {
                    var exceptionLookup = new Dictionary<CRefIdentifier, CodeDocException>();
                    foreach (var xmlDocException in model.XmlDocs.ExceptionElements) {
                        var exceptionCRef = String.IsNullOrWhiteSpace(xmlDocException.CRef)
                            ? new CRefIdentifier("T:")
                            : new CRefIdentifier(xmlDocException.CRef);
                        CodeDocException exceptionModel;
                        if (!exceptionLookup.TryGetValue(exceptionCRef, out exceptionModel)) {
                            exceptionModel = new CodeDocException(GetSimpleMember(exceptionCRef) ?? CreateTypeMemberPlaceholder(exceptionCRef));
                            exceptionModel.Ensures = new List<XmlDocNode>();
                            exceptionModel.Conditions = new List<XmlDocNode>();
                            exceptionLookup.Add(exceptionCRef, exceptionModel);
                        }
                        var priorElement = xmlDocException.PriorElement;
                        if (
                            null != priorElement
                            && String.Equals("ensuresOnThrow", priorElement.Name, StringComparison.OrdinalIgnoreCase)
                            && priorElement.Element.GetAttribute("exception") == exceptionCRef.FullCRef
                        ) {
                            if (priorElement.HasChildren) {
                                exceptionModel.Ensures.Add(priorElement);
                            }
                        }
                        else {
                            if (xmlDocException.HasChildren) {
                                exceptionModel.Conditions.Add(xmlDocException);
                            }
                        }
                    }

                    var exceptionModels = exceptionLookup.Values
                        .OrderBy(x => x.ExceptionType.CRef.FullCRef)
                        .ToArray();

                    foreach (var exceptionModel in exceptionModels) {
                        // TODO: freeze collections
                        exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray());
                        exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray());
                    }

                    model.Exceptions = new ReadOnlyCollection<CodeDocException>(exceptionModels);
                }

                if (model.XmlDocs.HasEnsuresElements) {
                    model.Ensures = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());
                }
                if (model.XmlDocs.HasRequiresElements) {
                    model.Requires = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());
                }
            }
        }

        private void ApplyStandardXmlDocs(CodeDocSimpleMember model, string cRef) {
            Contract.Requires(model != null);
            Contract.Requires(!String.IsNullOrEmpty(cRef));
            model.XmlDocs = XmlDocs.GetMember(cRef);
        }

        private void ApplySimpleEntityAttributes(CodeDocSimpleMember member, MemberReference memberReference) {
            Contract.Requires(memberReference != null);
            member.ExternalVisibility = memberReference.GetExternalVisibility();
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, PropertyReference propertyReference) {
            Contract.Requires(model != null);
            Contract.Requires(propertyReference != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, propertyReference);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyReference);
            model.Title = model.ShortName;
            Contract.Assume(propertyReference.DeclaringType != null);
            model.NamespaceName = propertyReference.DeclaringType.Namespace;

            if (propertyReference.IsItemIndexerProperty())
                model.SubTitle = "Indexer";
            else
                model.SubTitle = "Property";
        }

        private CodeDocProperty ConvertToModel(PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var model = new CodeDocProperty(GetCRefIdentifier(propertyReference));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, propertyReference);

            var parameters = propertyReference.Parameters;
            if (parameters.Count > 0) {
                model.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameters.Select(
                    parameter => {
                        var parameterSummaryElement = model.XmlDocs != null
                            ? model.XmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetSimpleEntity(parameter.ParameterType),
                            parameterSummaryElement
                        );
                        ApplyEntityAttributes(paramModel, parameter, model.XmlDocs);
                        return paramModel;
                    }
                ).ToArray());
            }

            var propertyDefinition = propertyReference.ToDefinition();
            if (propertyDefinition != null) {
                model.IsStatic = propertyDefinition.IsStatic();
                model.IsObsolete = propertyDefinition.HasAttribute(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            }

            var getterMethodInfo = propertyDefinition == null ? null : propertyDefinition.GetMethod;
            if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                var accessor = ConvertToModel(getterMethodInfo);
                if (model.XmlDocs != null && model.XmlDocs.HasGetterElement) {
                    accessor.XmlDocs = model.XmlDocs.GetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Getter = accessor;
            }

            var setterMethodInfo = propertyDefinition == null ? null : propertyDefinition.SetMethod;
            if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                var accessor = ConvertToModel(setterMethodInfo);
                if (model.XmlDocs != null && model.XmlDocs.HasSetterElement) {
                    accessor.XmlDocs = model.XmlDocs.SetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Setter = accessor;
            }

            model.ValueType = GetSimpleEntity(propertyReference.PropertyType);
            Contract.Assume(propertyReference.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(propertyReference.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(propertyReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetSimpleEntity(propertyReference.DeclaringType);

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, EventReference eventReference) {
            Contract.Requires(model != null);
            Contract.Requires(eventReference != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, eventReference);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(eventReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(eventReference);
            model.Title = model.ShortName;
            Contract.Assume(eventReference.DeclaringType != null);
            model.NamespaceName = eventReference.DeclaringType.Namespace;
            model.SubTitle = "Event";
        }

        private CodeDocEvent ConvertToModel(EventReference eventReference) {
            Contract.Requires(eventReference != null);
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            var cRef = GetCRefIdentifier(eventReference);
            var model = new CodeDocEvent(cRef);

            ApplyStandardXmlDocs(model, cRef.FullCRef);
            ApplyCommonAttributes(model, eventReference);

            model.DelegateType = GetSimpleEntity(eventReference.EventType);
            Contract.Assume(eventReference.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventReference.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(eventReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetSimpleEntity(eventReference.DeclaringType);

            var eventDefinition = eventReference.ToDefinition();
            if (eventDefinition != null) {
                model.IsStatic = eventDefinition.IsStatic();
                model.IsObsolete = eventDefinition.HasAttribute(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            }

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, FieldReference fieldReference) {
            Contract.Requires(model != null);
            Contract.Requires(fieldReference != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, fieldReference);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldReference);
            model.Title = model.ShortName;
            Contract.Assume(fieldReference.DeclaringType != null);
            model.NamespaceName = fieldReference.DeclaringType.Namespace;

            var fieldDefinition = fieldReference.ToDefinition();
            if (fieldDefinition != null && fieldDefinition.IsLiteral)
                model.SubTitle = "Constant";
            else
                model.SubTitle = "Field";
        }

        private CodeDocField ConvertToModel(FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var model = new CodeDocField(GetCRefIdentifier(fieldReference));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, fieldReference);

            var fieldDefinition = fieldReference.ToDefinition();

            model.ValueType = GetSimpleEntity(fieldReference.FieldType);
            if (fieldDefinition != null) {
                model.IsLiteral = fieldDefinition.IsLiteral;
                model.IsInitOnly = fieldDefinition.IsInitOnly;
                model.IsStatic = fieldDefinition.IsStatic;
                model.IsObsolete = fieldDefinition.HasAttribute(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            }

            var declaringTypeReference = fieldReference.DeclaringType;
            if (declaringTypeReference != null) {
                model.Namespace = GetCodeDocNamespaceByName(declaringTypeReference.Namespace);
                model.Assembly = GetCodeDocAssembly(declaringTypeReference.Module.Assembly);
                model.DeclaringType = GetSimpleEntity(declaringTypeReference);
            }
            return model;
        }

        private string GetMethodSubTitle(MethodDefinition methodDefinition) {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            if (methodDefinition != null) {
                if (methodDefinition.IsConstructor)
                    return "Constructor";
                if (methodDefinition.IsOperatorOverload())
                    return "Operator";
            }
            return "Method";
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, MethodReference methodReference) {
            Contract.Requires(model != null);
            Contract.Requires(methodReference != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, methodReference);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(methodReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(methodReference);
            model.Title = model.ShortName;
            Contract.Assume(methodReference.DeclaringType != null);
            model.NamespaceName = methodReference.DeclaringType.Namespace;

            model.SubTitle = GetMethodSubTitle(methodReference.ToDefinition());
        }

        private CodeDocMethod ConvertToModel(MethodReference methodReference) {
            Contract.Requires(methodReference != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var model = new CodeDocMethod(GetCRefIdentifier(methodReference));

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, methodReference);

            var methodDefinition = methodReference.ToDefinition();

            Contract.Assume(methodReference.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodReference.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetSimpleEntity(methodReference.DeclaringType);

            if (methodDefinition != null) {
                model.IsOperatorOverload = methodDefinition.IsOperatorOverload();
                model.IsExtensionMethod = methodDefinition.IsExtensionMethod();
                model.IsSealed = methodDefinition.IsSealed();
                model.IsStatic = methodDefinition.IsStatic;
                model.IsObsolete = methodDefinition.HasAttribute(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            }

            if (methodDefinition != null) {
                if (!methodDefinition.DeclaringType.IsInterface) {
                    if (methodDefinition.IsAbstract)
                        model.IsAbstract = true;
                    else if (methodDefinition.IsVirtual && !methodDefinition.IsFinal && methodDefinition.IsNewSlot)
                        model.IsVirtual = true;
                }
                model.IsPure = methodDefinition.HasAttribute(t => t.Constructor.Name == "PureAttribute");
            }
            var parameterModels = methodReference
                .Parameters
                .Select(parameter => {
                    var parameterSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.GetParameterSummary(parameter.Name)
                        : null;
                    var paramModel = new CodeDocParameter(
                        parameter.Name,
                        GetSimpleEntity(parameter.ParameterType),
                        parameterSummaryElement
                    );
                    ApplyEntityAttributes(paramModel, parameter, model.XmlDocs);
                    return paramModel;
                })
                .ToArray();
            model.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameterModels);

            if (methodReference.ReturnType != null && !methodReference.ReturnType.IsVoid()) {
                var returnSummaryElement = model.XmlDocs != null
                    ? model.XmlDocs.ReturnsElement
                    : null;
                var paramModel = new CodeDocParameter(
                    String.Empty,
                    GetSimpleEntity(methodReference.ReturnType),
                    returnSummaryElement
                );
                ApplyEntityAttributes(paramModel, methodReference.ReturnType, model.XmlDocs);
                model.Return = paramModel;
            }

            ExpandCodeDocMethodXmlDocProperties(model);

            if (methodReference.HasGenericParameters) {
                /*var genericDefinition = methodReference.g
                    ? methodReference
                    : methodDefinition == null ? null : methodDefinition.GetGenericMethodDefinition();*/
                
                //if (genericDefinition != null) {
                    //var genericArguments = genericDefinition.GetGenericArguments();
                    var genericArguments = methodReference.GenericParameters;
                    if (genericArguments.Count > 0) {
                        var xmlDocs = XmlDocs.GetMember(GetCRefIdentifier(methodReference).FullCRef);
                        var genericModels = new List<CodeDocGenericParameter>();
                        foreach (var genericArgument in genericArguments) {
                            var argumentName = genericArgument.Name;
                            Contract.Assume(!String.IsNullOrEmpty(argumentName));
                            //var typeConstraints = genericArgument.GetGenericParameterConstraints();
                            var genericModel = new CodeDocGenericParameter {
                                Name = argumentName
                            };

                            if (xmlDocs != null)
                                genericModel.Summary = xmlDocs.GetTypeParameterSummary(argumentName);

                            var typeConstraints = genericArgument.Constraints;
                            if (typeConstraints.Count > 0)
                                genericModel.TypeConstraints = new ReadOnlyCollection<ICodeDocMember>(typeConstraints.Select(GetSimpleEntity).ToArray());

                            genericModel.IsContravariant = genericArgument.IsContravariant;
                            genericModel.IsCovariant = genericArgument.IsCovariant;
                            genericModel.HasDefaultConstructorConstraint = genericArgument.HasDefaultConstructorConstraint;
                            genericModel.HasNotNullableValueTypeConstraint = genericArgument.HasNotNullableValueTypeConstraint;
                            genericModel.HasReferenceTypeConstraint = genericArgument.HasReferenceTypeConstraint;

                            genericModels.Add(genericModel);

                        }
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels);
                    }
                //}
            }

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, TypeReference typeReference) {
            Contract.Requires(model != null);
            Contract.Requires(typeReference != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, typeReference);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(typeReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(typeReference);
            model.Title = model.ShortName;
            model.NamespaceName = typeReference.GetOuterType().Namespace;

            var typeDefinition = typeReference.ToDefinition();

            if (typeDefinition != null && typeDefinition.IsEnum)
                model.SubTitle = "Enumeration";
            else if (typeReference.IsValueType)
                model.SubTitle = "Structure";
            else if (typeDefinition != null && typeDefinition.IsInterface)
                model.SubTitle = "Interface";
            else if (typeDefinition != null && typeDefinition.IsDelegateType())
                model.SubTitle = "Delegate";
            else
                model.SubTitle = "Class";
        }

        private CodeDocType ConvertToModel(TypeReference typeReference) {
            Contract.Requires(typeReference != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);

            var cRef = GetCRefIdentifier(typeReference);
            var typeDefinition = typeReference.ToDefinition();
            var model = typeDefinition != null && typeDefinition.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, typeReference);

            model.Namespace = GetCodeDocNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(typeReference.Module.Assembly);
            model.IsValueType = typeReference.IsValueType;
            if (typeDefinition != null) {
                model.IsEnum = typeDefinition.IsEnum;
                model.IsFlagsEnum = typeDefinition.IsEnum && typeDefinition.HasAttribute(x => x.AttributeType.FullName == "System.FlagsAttribute");
                model.IsSealed = typeDefinition.IsSealed;
                model.IsStatic = typeDefinition.IsStatic();
                model.IsObsolete = typeDefinition.HasAttribute(x => x.AttributeType.FullName == "System.ObsoleteAttribute");
            }

            if (typeReference.DeclaringType != null) {
                model.DeclaringType = GetSimpleEntity(typeReference.DeclaringType);
            }

            var baseChainDefinitions = new TypeDefinition[0];

            if (typeDefinition != null && !typeDefinition.IsInterface) {
                baseChainDefinitions = typeDefinition.GetBaseChainDefinitions().ToArray();
                var baseChainModels = baseChainDefinitions
                    .Select(GetSimpleEntity)
                    .ToArray();
                model.BaseChain = new ReadOnlyCollection<ICodeDocMember>(baseChainModels);
            }

            if (typeDefinition != null) {
                var interfaces = typeDefinition.HasInterfaces
                    ? typeDefinition.Interfaces.Select(GetSimpleEntity).ToList()
                    : new List<ICodeDocMember>();
                var interfaceCRefs = new HashSet<CRefIdentifier>(interfaces.Select(x => x.CRef));
                interfaces.AddRange(baseChainDefinitions
                    .Where(x => x.HasInterfaces)
                    .SelectMany(x => x.Interfaces)
                    .Select(x => new {InterfaceReference = x, CRef = GetCRefIdentifier(x)})
                    .Where(x => interfaceCRefs.Add(x.CRef))
                    .Select(x => GetSimpleEntity(x.InterfaceReference))
                );
                model.Interfaces = new ReadOnlyCollection<ICodeDocMember>(interfaces.ToArray());
            }

            if (typeReference.HasGenericParameters) {
                /*var genericTypeDefinition = type.IsGenericTypeDefinition
                    ? type
                    : type.GetGenericTypeDefinition();*/
                if (true /*genericTypeDefinition != null*/) {
                    //var genericArguments = genericTypeDefinition.GetGenericArguments();
                    var genericArguments = typeReference.GenericParameters;
                    if (genericArguments.Count > 0) {
                        var xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
                        GenericParameter[] parentGenericArguments = null;
                        if (typeReference.IsNested) {
                            Contract.Assume(typeReference.DeclaringType != null);
                            parentGenericArguments = typeReference.DeclaringType.GenericParameters.ToArray();
                        }

                        var genericModels = new List<CodeDocGenericParameter>();
                        foreach (var genericArgument in genericArguments) {
                            var argumentName = genericArgument.Name;
                            Contract.Assume(!String.IsNullOrEmpty(argumentName));
                            if (null != parentGenericArguments && parentGenericArguments.Any(p => p.Name == argumentName)) {
                                continue;
                            }

                            var typeConstraints = genericArgument.Constraints;
                            var genericModel = new CodeDocGenericParameter {
                                Name = argumentName,
                            };

                            if (xmlDocs != null)
                                genericModel.Summary = xmlDocs.GetTypeParameterSummary(argumentName);
                            if (typeConstraints.Count > 0)
                                genericModel.TypeConstraints = Array.AsReadOnly(typeConstraints.Select(GetSimpleEntity).ToArray());

                            genericModel.IsContravariant = genericArgument.IsContravariant;
                            genericModel.IsCovariant = genericArgument.IsCovariant;
                            genericModel.HasDefaultConstructorConstraint = genericArgument.HasDefaultConstructorConstraint;
                            genericModel.HasNotNullableValueTypeConstraint = genericArgument.HasNotNullableValueTypeConstraint;
                            genericModel.HasReferenceTypeConstraint = genericArgument.HasReferenceTypeConstraint;

                            genericModels.Add(genericModel);
                        }
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels);
                    }
                }
            }

            if (typeDefinition != null && typeDefinition.HasNestedTypes) {
                var nestedTypeModels = new List<ICodeDocMember>();
                var nestedDelegateModels = new List<ICodeDocMember>();
                foreach (var nestedType in typeDefinition.NestedTypes.Where(MemberFilter)) {
                    var nestedTypeModel = ConvertToModel(nestedType)
                        ?? ConvertToSimpleModel(nestedType);
                    if (nestedType.IsDelegateType())
                        nestedDelegateModels.Add(nestedTypeModel);
                    else
                        nestedTypeModels.Add(nestedTypeModel);
                }
                model.NestedTypes = new ReadOnlyCollection<ICodeDocMember>(nestedTypeModels);
                model.NestedDelegates = new List<ICodeDocMember>(nestedDelegateModels);

                var methodModels = new List<ICodeDocMember>();
                var operatorModels = new List<ICodeDocMember>();
                var constructorModels = new List<ICodeDocMember>();
                foreach (var methodDefinition in typeDefinition.GetAllMethods(MemberFilter)) {
                    var methodModel = ConvertToModel(methodDefinition)
                        ?? ConvertToSimpleModel(methodDefinition);
                    if(methodDefinition.IsConstructor)
                        constructorModels.Add(methodModel);
                    else if (methodDefinition.IsOperatorOverload())
                        operatorModels.Add(methodModel);
                    else
                        methodModels.Add(methodModel);
                }
                model.Methods = new ReadOnlyCollection<ICodeDocMember>(methodModels);
                model.Operators = new ReadOnlyCollection<ICodeDocMember>(operatorModels);
                model.Constructors = new ReadOnlyCollection<ICodeDocMember>(constructorModels);

                model.Properties = new ReadOnlyCollection<ICodeDocMember>(
                    typeDefinition
                    .GetAllProperties(MemberFilter)
                    .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                    .ToArray());

                model.Fields = new ReadOnlyCollection<ICodeDocMember>(
                    typeDefinition
                    .GetAllFields(MemberFilter)
                    .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                    .ToArray());

                model.Events = new ReadOnlyCollection<ICodeDocMember>(
                    typeDefinition
                    .GetAllEvents(MemberFilter)
                    .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                    .ToArray());
            }

            if (model is CodeDocDelegate && typeDefinition != null) {
                var delegateResult = (CodeDocDelegate)model;

                delegateResult.IsPure = typeDefinition
                    .HasAttribute(t => t.Constructor.Name == "PureAttribute");

                var parameterModels = typeDefinition
                    .GetDelegateTypeParameters()
                    .Select(parameter => {
                        var parameterSummaryElement = model.XmlDocs != null
                            ? model.XmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetSimpleEntity(parameter.ParameterType),
                            parameterSummaryElement
                        );
                        ApplyEntityAttributes(paramModel, parameter, model.XmlDocs);
                        return paramModel;
                    })
                    .ToArray();
                delegateResult.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameterModels);

                var returnType = typeDefinition.GetDelegateReturnType();
                if (returnType != null && !returnType.IsVoid()) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    var paramModel = new CodeDocParameter(
                        String.Empty,
                        GetSimpleEntity(returnType),
                        returnSummaryElement
                    );
                    ApplyEntityAttributes(paramModel, returnType, model.XmlDocs);
                    delegateResult.Return = paramModel;
                }

                if (model.XmlDocs != null) {
                    if (model.XmlDocs.HasExceptionElements) {
                        var exceptionLookup = new Dictionary<CRefIdentifier, CodeDocException>();
                        foreach (var xmlDocException in model.XmlDocs.ExceptionElements) {
                            var exceptionCRef = String.IsNullOrWhiteSpace(xmlDocException.CRef)
                                ? new CRefIdentifier("T:")
                                : new CRefIdentifier(xmlDocException.CRef);
                            CodeDocException exceptionModel;
                            if (!exceptionLookup.TryGetValue(exceptionCRef, out exceptionModel)) {
                                exceptionModel = new CodeDocException(GetSimpleMember(exceptionCRef) ?? CreateTypeMemberPlaceholder(exceptionCRef));
                                exceptionModel.Ensures = new List<XmlDocNode>();
                                exceptionModel.Conditions = new List<XmlDocNode>();
                                exceptionLookup.Add(exceptionCRef, exceptionModel);
                            }
                            var priorElement = xmlDocException.PriorElement;
                            if (
                                null != priorElement
                                && String.Equals("ensuresOnThrow", priorElement.Name, StringComparison.OrdinalIgnoreCase)
                                && priorElement.Element.GetAttribute("exception") == exceptionCRef.FullCRef
                            ) {
                                if (priorElement.HasChildren) {
                                    exceptionModel.Ensures.Add(priorElement);
                                }
                            }
                            else {
                                if (xmlDocException.HasChildren) {
                                    exceptionModel.Conditions.Add(xmlDocException);
                                }
                            }
                        }

                        var exceptionModels = exceptionLookup.Values
                            .OrderBy(x => x.ExceptionType.CRef.FullCRef)
                            .ToArray();

                        foreach (var exceptionModel in exceptionModels) {
                            // freeze collections
                            exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray());
                            exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray());
                        }

                        delegateResult.Exceptions = new ReadOnlyCollection<CodeDocException>(exceptionModels);
                    }

                    if (model.XmlDocs.HasEnsuresElements) {
                        delegateResult.Ensures =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());
                    }
                    if (model.XmlDocs.HasRequiresElements) {
                        delegateResult.Requires =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());
                    }
                }
            }

            return model;
        }

    }
}
