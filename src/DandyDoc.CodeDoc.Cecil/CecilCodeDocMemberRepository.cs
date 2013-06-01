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
                assemblyModel.Uri = assemblyModel.CRef.ToUri();

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
                        namespaceModel.Uri = namespaceModel.CRef.ToUri();
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
            if (memberReference.GetExternalVisibilityOrDefault() == ExternalVisibilityKind.Hidden)
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
            return memberReference.GetExternalVisibilityOrDefault() != ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="typeReference">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(TypeReference typeReference) {
            return typeReference != null
                && typeReference.GetExternalVisibilityOrDefault() != ExternalVisibilityKind.Hidden;
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
            if (methodReference.GetExternalVisibilityOrDefault() == ExternalVisibilityKind.Hidden)
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
            if (fieldReference.GetExternalVisibilityOrDefault() == ExternalVisibilityKind.Hidden)
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
            if (eventReference.GetExternalVisibilityOrDefault() == ExternalVisibilityKind.Hidden)
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
            if (propertyReference.GetExternalVisibilityOrDefault() == ExternalVisibilityKind.Hidden)
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
        protected override ICodeDocMember GetMemberModel(CRefIdentifier cRef, bool lite) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberReference = GetMemberReferencePreferDefinition(cRef);
            if (memberReference == null || !MemberFilter(memberReference))
                return null;

            return ConvertToModel(memberReference, lite: lite);
        }

        /// <summary>
        /// Converts a Cecil member to a code doc model.
        /// </summary>
        /// <param name="memberReference">A Cecil member.</param>
        /// <param name="lite">Indicates that the generated model should be a lite version.</param>
        /// <returns>A code doc model for the given member.</returns>
        protected virtual CodeDocMemberContentBase ConvertToModel(MemberReference memberReference, bool lite) {
            if(memberReference == null) throw new ArgumentNullException("memberReference");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            if (memberReference is TypeReference)
                return ConvertToModel((TypeReference)memberReference, lite: lite);
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

        private CodeDocType GetOrConvert(TypeReference typeReference, bool lite = false) {
            Contract.Requires(typeReference != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it
            return ConvertToModel(typeReference, lite: lite);
        }

        private CodeDocParameter CreateArgument(ParameterReference parameterReference, ICodeDocMemberDataProvider provider) {
            Contract.Requires(parameterReference != null);
            Contract.Requires(provider != null);
            var parameterName = parameterReference.Name;
            var parameterTypeReference = parameterReference.ParameterType;
            var model = new CodeDocParameter(
                parameterName,
                GetOrConvert(parameterTypeReference, lite: true)
            );
            var parameterDefinition = parameterReference.ToDefinition();
            if (parameterDefinition != null) {
                model.IsOut = parameterDefinition.IsOut;
            }
            model.IsByRef = parameterTypeReference.IsByReference;
            model.IsReferenceType = !parameterTypeReference.IsValueType;
            model.SummaryContents = provider.GetParameterSummaryContents(parameterName).ToArray();
            if (!model.NullRestricted.HasValue)
                model.NullRestricted = provider.RequiresParameterNotEverNull(parameterName);
            return model;
        }

        private CodeDocParameter CreateReturn(MethodReturnType methodReturnType, ICodeDocMemberDataProvider provider) {
            Contract.Requires(methodReturnType != null);
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<CodeDocParameter>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocParameter>().Name));
            Contract.Ensures(Contract.Result<CodeDocParameter>().ParameterType != null);
            var returnTypeReference = methodReturnType.ReturnType;
            var model = new CodeDocParameter(
                "result",
                GetOrConvert(returnTypeReference, lite: true)
            );
            model.IsReferenceType = !returnTypeReference.IsValueType;
            model.SummaryContents = provider.GetReturnSummaryContents().ToArray();
            if (!model.NullRestricted.HasValue)
                model.NullRestricted = provider.EnsuresResultNotEverNull;
            return model;
        }

        private CodeDocGenericParameter[] CreateGenericTypeParameters(GenericParameter[] genericArguments, ICodeDocMemberDataProvider provider) {
            Contract.Requires(genericArguments != null);
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<CodeDocGenericParameter[]>() != null);
            Contract.Ensures(Contract.Result<CodeDocGenericParameter[]>().Length == genericArguments.Length);
            return Array.ConvertAll(
                genericArguments,
                genericArgument => CreateGenericTypeParameter(genericArgument, provider));
        }

        private CodeDocGenericParameter CreateGenericTypeParameter(GenericParameter genericArgument, ICodeDocMemberDataProvider provider) {
            Contract.Requires(genericArgument != null);
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<CodeDocGenericParameter>() != null);
            var argumentName = genericArgument.Name;
            Contract.Assume(!String.IsNullOrEmpty(argumentName));

            var model = new CodeDocGenericParameter(argumentName);

            model.SummaryContents = provider
                .GetGenericTypeSummaryContents(argumentName)
                .ToArray();

            var typeConstraints = genericArgument.Constraints.ToArray();
            if (typeConstraints.Length > 0)
                model.TypeConstraints = Array.ConvertAll(typeConstraints, t => GetOrConvert(t, lite: true));

            model.IsContravariant = genericArgument.IsContravariant;
            model.IsCovariant = genericArgument.IsCovariant;
            model.HasDefaultConstructorConstraint = genericArgument.HasDefaultConstructorConstraint;
            model.HasNotNullableValueTypeConstraint = genericArgument.HasNotNullableValueTypeConstraint;
            model.HasReferenceTypeConstraint = genericArgument.HasReferenceTypeConstraint;

            return model;
        }

        private CodeDocEvent ConvertToModel(EventReference eventReference) {
            Contract.Requires(eventReference != null);
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().Title));
            Contract.Ensures(Contract.Result<CodeDocEvent>().Title == Contract.Result<CodeDocEvent>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().SubTitle));

            var eventDefinition = eventReference.ToDefinition();
            var eventCRef = GetCRefIdentifier(eventReference);
            var model = new CodeDocEvent(eventCRef);
            model.Uri = eventCRef.ToUri();

            var memberDataProvider = new CodeDocMemberReferenceProvider<EventReference>(eventReference);
            var xmlDocs = XmlDocs.GetMember(eventCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            if (eventDefinition != null) {
                var baseEvent = eventDefinition.FindNextAncestor();
                if (baseEvent != null) {
                    var baseEventModel = GetOnly(GetCRefIdentifier(baseEvent), lite: true);
                    if(baseEventModel != null)
                        memberDataProvider.Add(new CodeDocMemberDataProvider(baseEventModel));
                }
            }

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
            model.Title = model.ShortName;
            Contract.Assume(memberDataProvider.Member.DeclaringType != null);
            model.NamespaceName = memberDataProvider.Member.DeclaringType.GetOuterType().Namespace;
            model.SubTitle = "Event";

            model.DelegateType = GetOrConvert(eventReference.EventType, lite: true);
            Contract.Assume(eventReference.DeclaringType != null);
            model.Namespace = GetOrCreateNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(eventReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetOrConvert(eventReference.DeclaringType, lite: true);
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            return model;
        }

        private CodeDocField ConvertToModel(FieldReference fieldReference) {
            Contract.Requires(fieldReference != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().Title));
            Contract.Ensures(Contract.Result<CodeDocField>().Title == Contract.Result<CodeDocField>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().SubTitle));
            var fieldCRef = GetCRefIdentifier(fieldReference);
            var model = new CodeDocField(fieldCRef);
            model.Uri = fieldCRef.ToUri();

            var memberDataProvider = new CodeDocMemberReferenceProvider<FieldReference>(fieldReference);
            var xmlDocs = XmlDocs.GetMember(fieldCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldReference);
            model.Title = model.ShortName;
            Contract.Assume(fieldReference.DeclaringType != null);
            model.NamespaceName = fieldReference.DeclaringType.GetOuterType().Namespace;

            var fieldDefinition = memberDataProvider.Definition as FieldDefinition;
            bool isLiteral;
            bool isInitOnly;
            if (fieldDefinition != null) {
                isLiteral = fieldDefinition.IsLiteral;
                isInitOnly = fieldDefinition.IsInitOnly;
            }
            else {
                isLiteral = false;
                isInitOnly = false;
            }

            model.SubTitle = isLiteral ? "Constant" : "Field";

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
            model.ValueType = GetOrConvert(fieldReference.FieldType, lite: true);
            model.IsLiteral = isLiteral;
            model.IsInitOnly = isInitOnly;
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            Contract.Assume(fieldReference.DeclaringType != null);
            model.Namespace = GetOrCreateNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(fieldReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetOrConvert(fieldReference.DeclaringType, lite: true);
            return model;
        }

        private CodeDocProperty ConvertToModel(PropertyReference propertyReference) {
            Contract.Requires(propertyReference != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().Title));
            Contract.Ensures(Contract.Result<CodeDocProperty>().Title == Contract.Result<CodeDocProperty>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().SubTitle));

            var propertyCRef = GetCRefIdentifier(propertyReference);
            var model = new CodeDocProperty(propertyCRef);
            model.Uri = propertyCRef.ToUri();

            var memberDataProvider = new CodeDocMemberReferenceProvider<PropertyReference>(propertyReference);
            var xmlDocs = XmlDocs.GetMember(propertyCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            var propertyDefinition = memberDataProvider.Definition as PropertyDefinition;
            if (propertyDefinition != null) {
                var propertyBase = propertyDefinition.FindNextAncestor();
                if (propertyBase != null) {
                    var propertyBaseModel = GetOnly(GetCRefIdentifier(propertyBase), lite: true);
                    if(propertyBaseModel != null)
                        memberDataProvider.Add(new CodeDocMemberDataProvider(propertyBaseModel));
                }
            }

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyReference);
            model.Title = model.ShortName;
            Contract.Assume(propertyReference.DeclaringType != null);
            model.NamespaceName = propertyReference.DeclaringType.GetOuterType().Namespace;

            model.SubTitle = propertyReference.IsItemIndexerProperty() ? "Indexer" : "Property";

            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
            model.ValueType = GetOrConvert(propertyReference.PropertyType, lite: true);
            Contract.Assume(propertyReference.DeclaringType != null);
            model.Namespace = GetOrCreateNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(propertyReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetOrConvert(propertyReference.DeclaringType, lite: true);

            var parameters = propertyReference.Parameters.ToArray();
            if (parameters.Length > 0)
                model.Parameters = Array.ConvertAll(parameters, p => CreateArgument(p, memberDataProvider));

            if (propertyDefinition != null) {
                var getterMethodInfo = propertyDefinition.GetMethod;
                if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                    var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasGetterElement)
                        ? new CodeDocMemberXmlDataProvider(xmlDocs.GetterElement)
                        : null;
                    model.Getter = ConvertToModel(getterMethodInfo, accessorExtraProvider);
                }

                var setterMethodInfo = propertyDefinition.SetMethod;
                if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                    var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasSetterElement)
                        ? new CodeDocMemberXmlDataProvider(xmlDocs.SetterElement)
                        : null;
                    model.Setter = ConvertToModel(setterMethodInfo, accessorExtraProvider);
                }
            }

            return model;
        }

        private CodeDocMethod ConvertToModel(MethodReference methodReference, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
            Contract.Requires(methodReference != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().Title));
            Contract.Ensures(Contract.Result<CodeDocMethod>().Title == Contract.Result<CodeDocMethod>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().SubTitle));

            var methodCRef = GetCRefIdentifier(methodReference);
            var model = new CodeDocMethod(methodCRef);
            model.Uri = methodCRef.ToUri();

            var memberDataProvider = new CodeDocMemberReferenceProvider<MethodReference>(methodReference);
            var xmlDocs = XmlDocs.GetMember(methodCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
            if (extraMemberDataProvider != null)
                memberDataProvider.Add(extraMemberDataProvider);

            var methodDefinition = memberDataProvider.Definition as MethodDefinition;
            if (methodDefinition != null) {
                var baseDefinition = methodDefinition.FindNextAncestor();
                if (baseDefinition != null) {
                    var baseDefinitionModel = GetOnly(GetCRefIdentifier(baseDefinition), lite: true);
                    if(baseDefinitionModel != null)
                        memberDataProvider.Add(new CodeDocMemberDataProvider(baseDefinitionModel));
                }
            }

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(methodReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(methodReference);
            model.Title = model.ShortName;
            Contract.Assume(methodReference.DeclaringType != null);
            model.NamespaceName = methodReference.DeclaringType.GetOuterType().Namespace;

            if (methodDefinition != null && methodDefinition.IsConstructor)
                model.SubTitle = "Constructor";
            else if (methodDefinition != null && methodDefinition.IsOperatorOverload())
                model.SubTitle = "Operator";
            else
                model.SubTitle = "Method";

            Contract.Assume(methodReference.DeclaringType != null);
            model.Namespace = GetOrCreateNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(methodReference.DeclaringType.Module.Assembly);
            model.DeclaringType = GetOrConvert(methodReference.DeclaringType, lite: true);
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();
            model.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

            if (methodDefinition != null) {
                model.IsOperatorOverload = methodDefinition.IsOperatorOverload();
                model.IsExtensionMethod = methodDefinition.IsExtensionMethod();
                model.IsSealed = methodDefinition.IsSealed();
                if (methodDefinition.DeclaringType != null && !methodDefinition.DeclaringType.IsInterface) {
                    if (methodDefinition.IsAbstract)
                        model.IsAbstract = true;
                    else if (methodDefinition.IsVirtual && !methodDefinition.IsFinal && methodDefinition.Attributes.HasFlag(MethodAttributes.NewSlot))
                        model.IsVirtual = true;
                }
            }

            model.Parameters = Array.ConvertAll(
                methodReference.Parameters.ToArray(),
                p => CreateArgument(p, memberDataProvider));

            if (methodReference.MethodReturnType != null && !methodReference.ReturnType.IsVoid())
                model.Return = CreateReturn(methodReference.MethodReturnType, memberDataProvider);

            if (memberDataProvider.HasExceptions)
                model.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
            if (memberDataProvider.HasEnsures)
                model.Ensures = memberDataProvider.GetEnsures().ToArray();
            if (memberDataProvider.HasRequires)
                model.Requires = memberDataProvider.GetRequires().ToArray();

            if (methodReference.HasGenericParameters) {
                var genericArguments = methodReference.GenericParameters;
                if (genericArguments.Count > 0) {
                    model.GenericParameters = CreateGenericTypeParameters(
                        genericArguments.ToArray(),
                        memberDataProvider);
                }
            }

            return model;
        }

        private CodeDocType ConvertToModel(TypeReference typeReference, bool lite = false) {
            Contract.Requires(typeReference != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().Title));
            Contract.Ensures(Contract.Result<CodeDocType>().Title == Contract.Result<CodeDocType>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().SubTitle));

            var cRef = GetCRefIdentifier(typeReference);
            var memberDataProvider = new CodeDocMemberReferenceProvider<TypeReference>(typeReference);
            var xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            var typeDefinition = memberDataProvider.Definition as TypeDefinition;

            var model = typeDefinition != null && typeDefinition.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);
            model.Uri = cRef.ToUri();
            var delegateModel = model as CodeDocDelegate;

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(typeReference);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(typeReference);
            model.Title = model.ShortName;
            model.NamespaceName = typeReference.GetOuterType().Namespace;

            bool isEnum;
            bool hasFlagsAttribute;
            bool isSealed;
            bool isInterface;
            if (typeDefinition != null) {
                isEnum = typeDefinition.IsEnum;
                hasFlagsAttribute = typeDefinition.HasAttribute(t => t.AttributeType.FullName == "System.FlagsAttribute");
                isSealed = typeDefinition.IsSealed;
                isInterface = typeDefinition.IsInterface;
            }
            else {
                isEnum = false;
                hasFlagsAttribute = false;
                isSealed = false;
                isInterface = false;
            }

            if (isEnum)
                model.SubTitle = "Enumeration";
            else if (typeReference.IsValueType)
                model.SubTitle = "Structure";
            else if (isInterface)
                model.SubTitle = "Interface";
            else if (typeDefinition != null && typeDefinition.IsDelegateType())
                model.SubTitle = "Delegate";
            else
                model.SubTitle = "Class";

            model.Namespace = GetOrCreateNamespaceByName(model.NamespaceName);
            model.Assembly = GetCodeDocAssembly(typeReference.Module.Assembly);
            model.IsEnum = isEnum;
            model.IsFlagsEnum = isEnum ? hasFlagsAttribute : (bool?)null;
            model.IsSealed = isSealed;
            model.IsValueType = typeReference.IsValueType;
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            if (typeReference.DeclaringType != null)
                model.DeclaringType = GetOrConvert(typeReference.DeclaringType, lite: true);

            if (delegateModel != null)
                delegateModel.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

            if (lite)
                return model;

            var baseChainDefinitions = new TypeDefinition[0];

            if (typeDefinition != null && !typeDefinition.IsInterface) {
                baseChainDefinitions = typeDefinition.GetBaseChainDefinitions().ToArray();
                var baseChainModels = baseChainDefinitions
                    .Select(d => ConvertToModel(d, lite: true))
                    .ToArray();
                model.BaseChain = new ReadOnlyCollection<CodeDocType>(baseChainModels);
            }

            if (typeDefinition != null) {
                var interfaces = typeDefinition.HasInterfaces
                    ? typeDefinition.Interfaces.Select(r => ConvertToModel(r, lite:true)).ToList()
                    : new List<CodeDocType>();
                var interfaceCRefs = new HashSet<CRefIdentifier>(interfaces.Select(x => x.CRef));
                interfaces.AddRange(baseChainDefinitions
                    .Where(d => d.HasInterfaces)
                    .SelectMany(d => d.Interfaces)
                    .Where(r => interfaceCRefs.Add(GetCRefIdentifier(r)))
                    .Select(r => ConvertToModel(r, lite: true))
                );
                model.Interfaces = new ReadOnlyCollection<CodeDocType>(interfaces.ToArray());
            }

            if (typeDefinition != null) {
                var nestedTypeModels = new List<ICodeDocMember>();
                var nestedDelegateModels = new List<ICodeDocMember>();
                foreach (var nestedType in typeDefinition.NestedTypes.Where(MemberFilter)) {
                    var nestedTypeModel = ConvertToModel(nestedType, lite: true);
                    if (nestedType.IsDelegateType())
                        nestedDelegateModels.Add(nestedTypeModel);
                    else
                        nestedTypeModels.Add(nestedTypeModel);
                }
                model.NestedTypes = nestedTypeModels;
                model.NestedDelegates = nestedDelegateModels;

                var methodModels = new List<ICodeDocMember>();
                var operatorModels = new List<ICodeDocMember>();
                var constructorModels = new List<ICodeDocMember>();
                foreach (var methodDefinition in typeDefinition.GetAllMethods(MemberFilter)) {
                    var methodModel = ConvertToModel(methodDefinition);
                    if (methodDefinition.IsConstructor)
                        constructorModels.Add(methodModel);
                    else if (methodDefinition.IsOperatorOverload())
                        operatorModels.Add(methodModel);
                    else
                        methodModels.Add(methodModel);
                }
                model.Methods = new ReadOnlyCollection<ICodeDocMember>(methodModels);
                model.Operators = new ReadOnlyCollection<ICodeDocMember>(operatorModels);
                model.Constructors = new ReadOnlyCollection<ICodeDocMember>(constructorModels);

                model.Properties = typeDefinition
                    .GetAllProperties(MemberFilter)
                    .Select(ConvertToModel)
                    .ToArray();

                model.Fields = typeDefinition
                    .GetAllFields(MemberFilter)
                    .Select(ConvertToModel)
                    .ToArray();

                model.Events = typeDefinition
                    .GetAllEvents(MemberFilter)
                    .Select(ConvertToModel)
                    .ToArray();
            }

            if (delegateModel != null && typeDefinition != null) {
                delegateModel.Parameters = Array.ConvertAll(
                    typeDefinition.GetDelegateTypeParameters().ToArray(),
                    p => CreateArgument(p, memberDataProvider));

                var methodReturn = typeDefinition.GetDelegateMethodReturn();
                if (methodReturn != null && !methodReturn.ReturnType.IsVoid())
                    delegateModel.Return = CreateReturn(methodReturn, memberDataProvider);

                if (memberDataProvider.HasExceptions)
                    delegateModel.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
                if (memberDataProvider.HasEnsures)
                    delegateModel.Ensures = memberDataProvider.GetEnsures().ToArray();
                if (memberDataProvider.HasRequires)
                    delegateModel.Requires = memberDataProvider.GetRequires().ToArray();
            }

            if (typeReference.HasGenericParameters) {
                var genericArguments = typeReference.GenericParameters.ToArray();
                if (genericArguments.Length > 0) {

                    if (typeReference.IsNested) {
                        Contract.Assume(typeReference.DeclaringType != null);
                        var parentGenericArguments = typeReference.DeclaringType.GenericParameters.ToArray();
                        genericArguments = genericArguments
                            .Where(a => parentGenericArguments.All(p => p.Name != a.Name))
                            .ToArray();
                    }

                    if (genericArguments.Length > 0) {
                        model.GenericParameters = CreateGenericTypeParameters(
                            genericArguments,
                            memberDataProvider);
                    }

                }
            }

            return model;
        }

    }
}