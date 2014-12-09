using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using DuckyDocs.CRef;
using DuckyDocs.CodeDoc.Utility;
using DuckyDocs.DisplayName;
using DuckyDocs.ExternalVisibility;
using DuckyDocs.Reflection;
using DuckyDocs.Utility;
using DuckyDocs.XmlDoc;

namespace DuckyDocs.CodeDoc
{

    /// <summary>
    /// Generates code doc member models from reflected members.
    /// </summary>
    public class ReflectionCodeDocMemberRepository : CodeDocMemberRepositoryBase
    {

        private static readonly StandardReflectionDisplayNameGenerator RegularTypeDisplayNameOverlay
            = new StandardReflectionDisplayNameGenerator {
                ShowTypeNameForMembers = false
            };

        private static readonly StandardReflectionDisplayNameGenerator NestedTypeDisplayNameOverlay
            = new StandardReflectionDisplayNameGenerator {
                ShowTypeNameForMembers = true
            };

        private static readonly StandardReflectionDisplayNameGenerator FullTypeDisplayNameOverlay
            = new StandardReflectionDisplayNameGenerator {
                ShowTypeNameForMembers = true,
                IncludeNamespaceForTypes = true
            };

        /// <summary>
        /// Gets a code reference identifier that is forced to a generic definition.
        /// </summary>
        /// <param name="memberInfo">The member to generate a code reference for.</param>
        /// <returns>A code reference.</returns>
        protected static CRefIdentifier GetGenericDefinitionCRefIdentifier(MemberInfo memberInfo) {
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(ReflectionCRefGenerator.WithPrefixGenericDefinition.GetCRef(memberInfo));
        }
        
        /// <summary>
        /// Gets a standard code reference identifier for a given member.
        /// </summary>
        /// <param name="memberInfo">The member to generate a code reference for.</param>
        /// <returns>A code reference.</returns>
        protected static CRefIdentifier GetCRefIdentifier(MemberInfo memberInfo){
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(ReflectionCRefGenerator.WithPrefix.GetCRef(memberInfo));
        }

        /// <summary>
        /// Gets a code reference for the given assembly.
        /// </summary>
        /// <param name="assembly">An assembly to generate a code reference for.</param>
        /// <returns>A code reference.</returns>
        /// <remarks>
        /// There is no standard for assembly code references so do not use them across different implementations.
        /// </remarks>
        protected static CRefIdentifier GetCRefIdentifier(Assembly assembly) {
            Contract.Requires(assembly != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(ReflectionCRefGenerator.WithPrefix.GetCRef(assembly));
        }

        /// <summary>
        /// Creates a new reflection code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into reflected members.</param>
        public ReflectionCodeDocMemberRepository(ReflectionCRefLookup cRefLookup)
            : this(cRefLookup, null) {
            Contract.Requires(cRefLookup != null);
        }

        /// <summary>
        /// Creates a new reflection code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into reflected members.</param>
        /// <param name="xmlDocs">The related XML documentation files for the members.</param>
        public ReflectionCodeDocMemberRepository(ReflectionCRefLookup cRefLookup, params XmlAssemblyDocument[] xmlDocs)
            : this(cRefLookup, (IEnumerable<XmlAssemblyDocument>)xmlDocs) {
            Contract.Requires(cRefLookup != null);
        }

        /// <summary>
        /// Creates a new reflection code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into reflected members.</param>
        /// <param name="xmlDocs">The related XML documentation files for the members.</param>
        public ReflectionCodeDocMemberRepository(ReflectionCRefLookup cRefLookup, IEnumerable<XmlAssemblyDocument> xmlDocs)
            : base(xmlDocs)
        {
            if(cRefLookup == null) throw new ArgumentNullException("cRefLookup");
            Contract.EndContractBlock();
            CRefLookup = cRefLookup;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
        }

        /// <summary>
        /// Generates the core collection of related namespaces and assemblies.
        /// </summary>
        /// <returns>The related assemblies and namespaces.</returns>
        protected override SimpleAssemblyNamespaceColleciton CreateSimpleAssemblyNamespaceCollection() {
            var assemblyModels = new List<CodeDocSimpleAssembly>();
            var namespaceModels = new Dictionary<string, CodeDocSimpleNamespace>();

            foreach (var assembly in CRefLookup.Assemblies) {
                var assemblyShortName = assembly.GetName().Name;
                var assemblyModel = new CodeDocSimpleAssembly(GetCRefIdentifier(assembly)) {
                    AssemblyFileName = Path.GetFileName(assembly.GetFilePath()),
                    Title = assemblyShortName,
                    ShortName = assemblyShortName,
                    FullName = assembly.FullName,
                    NamespaceName = assemblyShortName,
                    SubTitle = "Assembly",
                    NamespaceCRefs = new List<CRefIdentifier>()
                };
                assemblyModel.Uri = assemblyModel.CRef.ToUri();

                var assemblyTypeCRefs = new List<CRefIdentifier>();
                var assemblyNamespaceNames = new HashSet<string>();
                foreach (var type in assembly
                    .GetTypes()
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
                        Contract.Assume(!String.IsNullOrEmpty("N:" + namespaceName));
                        namespaceModel = new CodeDocSimpleNamespace(new CRefIdentifier("N:" + namespaceName)) {
                            Title = namespaceTitle,
                            ShortName = namespaceTitle,
                            FullName = namespaceTitle,
                            NamespaceName = namespaceTitle,
                            SubTitle = "Namespace",
                            TypeCRefs = new List<CRefIdentifier>(),
                            AssemblyCRefs = new List<CRefIdentifier>(),
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

            return new SimpleAssemblyNamespaceColleciton(
                assemblyModels.OrderBy(x => x.Title).ToArray(),
                namespaceModels.Values.OrderBy(x => x.Title).ToArray());
        }

        /// <summary>
        /// The code reference lookup.
        /// </summary>
        public ReflectionCRefLookup CRefLookup { get; private set; }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="memberInfo">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected bool MemberFilter(MemberInfo memberInfo) {
            if (memberInfo == null)
                return false;
            if (memberInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (memberInfo is MethodBase)
                return MemberFilter((MethodBase)memberInfo);
            if (memberInfo is FieldInfo)
                return MemberFilter((FieldInfo)memberInfo);
            if (memberInfo is EventInfo)
                return MemberFilter((EventInfo)memberInfo);
            if (memberInfo is PropertyInfo)
                return MemberFilter((PropertyInfo)memberInfo);
            if (memberInfo is Type)
                return MemberFilter((Type)memberInfo);
            return memberInfo.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="type">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(Type type) {
            return type != null
                && type.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="methodBase">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected bool MemberFilter(MethodBase methodBase){
            return MemberFilter(methodBase, false);
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="methodBase">The member to test.</param>
        /// <param name="isPropertyMethod">Indicates that the method is a property getter or setter.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(MethodBase methodBase, bool isPropertyMethod) {
            if (methodBase == null)
                return false;
            if (methodBase.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = methodBase.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;
            if (methodBase.IsFinalizer())
                return false;
            if(!isPropertyMethod && methodBase.IsSpecialName){
                if(methodBase.IsConstructor)
                    return true;
                if(methodBase.IsOperatorOverload())
                    return true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="fieldInfo">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(FieldInfo fieldInfo) {
            if (fieldInfo == null)
                return false;
            if (fieldInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = fieldInfo.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;
            if (fieldInfo.IsSpecialName)
                return false;
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="eventInfo">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(EventInfo eventInfo) {
            if (eventInfo == null)
                return false;
            if (eventInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (eventInfo.IsSpecialName)
                return false;
            return true;
        }

        /// <summary>
        /// Determines if a member is valid for this repository.
        /// </summary>
        /// <param name="propertyInfo">The member to test.</param>
        /// <returns><c>true</c> if the member is valid.</returns>
        protected virtual bool MemberFilter(PropertyInfo propertyInfo) {
            if (propertyInfo == null)
                return false;
            if (propertyInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (propertyInfo.IsSpecialName)
                return false;
            return true;
        }

        /// <summary>
        /// Creates a member generator that uses the given search context.
        /// </summary>
        /// <param name="searchContext">The search context that is to be used by the member generator.</param>
        /// <returns>A member generator that is used to create member models.</returns>
        protected override MemberGeneratorBase CreateGenerator(CodeDocRepositorySearchContext searchContext) {
            return new MemberGenerator(this, searchContext);
        }

        /// <summary>
        /// The model generation core for reflected members.
        /// </summary>
        protected class MemberGenerator : MemberGeneratorBase
        {

            /// <summary>
            /// Creates a new member generator using the given repository and search context.
            /// </summary>
            /// <param name="repository">The repository that is used to generate the models.</param>
            /// <param name="searchContext">The search context used to get other models from.</param>
            public MemberGenerator(ReflectionCodeDocMemberRepository repository, CodeDocRepositorySearchContext searchContext)
                : base(repository, searchContext){
                Contract.Requires(repository != null);
            }

            /// <summary>
            /// The core repository cast as a <see cref="ReflectionCodeDocMemberRepository"/>.
            /// </summary>
            protected ReflectionCodeDocMemberRepository ReflectionRepository {
                get {
                    Contract.Ensures(Contract.Result<ReflectionCodeDocMemberRepository>() != null);
                    return (ReflectionCodeDocMemberRepository)(Repository);
                }
            }

            /// <summary>
            /// The code reference lookup.
            /// </summary>
            public ReflectionCRefLookup CRefLookup {
                get {
                    Contract.Ensures(Contract.Result<ReflectionCRefLookup>() != null);
                    return ReflectionRepository.CRefLookup;
                }
            }

            /// <summary>
            /// Gets a code doc assembly from a reflected assembly.
            /// </summary>
            /// <param name="assembly">An assembly to get the code doc model for.</param>
            /// <returns>The assembly code doc model if found.</returns>
            private CodeDocSimpleAssembly GetCodeDocAssembly(Assembly assembly) {
                Contract.Requires(assembly != null);
                return GetCodeDocSimpleAssembly(GetCRefIdentifier(assembly));
            }

            /// <summary>
            /// Gets a code doc model for a reflected member.
            /// </summary>
            /// <param name="cRef">The code reference of a reflected member to create a model from.</param>
            /// <param name="detailLevel">Indicates the desired detail level of the model.</param>
            /// <returns>A code doc model.</returns>
            public override ICodeDocMember GetMemberModel(CRefIdentifier cRef, CodeDocMemberDetailLevel detailLevel) {
                if (cRef == null) throw new ArgumentNullException("cRef");
                Contract.EndContractBlock();

                if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                    return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
                if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                    return GetCodeDocSimpleAssembly(cRef);

                var memberInfo = ReflectionRepository.CRefLookup.GetMember(cRef);
                if (memberInfo == null || !ReflectionRepository.MemberFilter(memberInfo))
                    return null;

                return ConvertToModel(memberInfo, detailLevel);
            }

            /// <summary>
            /// Converts a reflected member to a code doc model.
            /// </summary>
            /// <param name="memberInfo">A reflected member.</param>
            /// <param name="detailLevel">Indicates the desired detail level of the model.</param>
            /// <param name="extraMemberDataProvider">A member data provider to include additional information that may be be easily obtained from normal sources.</param>
            /// <returns>A code doc model for the given member.</returns>
            protected virtual CodeDocMemberContentBase ConvertToModel(MemberInfo memberInfo, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                if (memberInfo == null) throw new ArgumentNullException("memberInfo");
                Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
                if (memberInfo is Type)
                    return ConvertToModel((Type)memberInfo, detailLevel, extraMemberDataProvider);
                if (memberInfo is FieldInfo)
                    return ConvertToModel((FieldInfo)memberInfo, detailLevel, extraMemberDataProvider);
                if (memberInfo is MethodBase)
                    return ConvertToModel((MethodBase)memberInfo, detailLevel, extraMemberDataProvider);
                if (memberInfo is EventInfo)
                    return ConvertToModel((EventInfo)memberInfo, detailLevel, extraMemberDataProvider);
                if (memberInfo is PropertyInfo)
                    return ConvertToModel((PropertyInfo)memberInfo, detailLevel, extraMemberDataProvider);
                throw new NotSupportedException();
            }

            private CodeDocType GetOrConvert(Type type, CodeDocMemberDetailLevel detailLevel) {
                Contract.Requires(type != null);
                Contract.Ensures(Contract.Result<CodeDocType>() != null);
                var getResult = GetOnlyType(GetCRefIdentifier(type), detailLevel);
                return getResult ?? ConvertToModel(type, detailLevel);
            }

            private CodeDocMethod GetOrConvert(MethodBase methodBase, CodeDocMemberDetailLevel detailLevel) {
                Contract.Requires(methodBase != null);
                Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
                var getResult = GetOnlyMethod(GetCRefIdentifier(methodBase), detailLevel);
                return getResult ?? ConvertToModel(methodBase, detailLevel);
            }

            private CodeDocProperty GetOrConvert(PropertyInfo propertyInfo, CodeDocMemberDetailLevel detailLevel) {
                Contract.Requires(propertyInfo != null);
                Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
                var getResult = GetOnlyProperty(GetCRefIdentifier(propertyInfo), detailLevel);
                return getResult ?? ConvertToModel(propertyInfo, detailLevel);
            }

            private CodeDocField GetOrConvert(FieldInfo fieldInfo, CodeDocMemberDetailLevel detailLevel) {
                Contract.Requires(fieldInfo != null);
                Contract.Ensures(Contract.Result<CodeDocField>() != null);
                var getResult = GetOnlyField(GetCRefIdentifier(fieldInfo), detailLevel);
                return getResult ?? ConvertToModel(fieldInfo, detailLevel);
            }

            private CodeDocEvent GetOrConvert(EventInfo eventInfo, CodeDocMemberDetailLevel detailLevel) {
                Contract.Requires(eventInfo != null);
                Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
                var getResult = GetOnlyEvent(GetCRefIdentifier(eventInfo), detailLevel);
                return getResult ?? ConvertToModel(eventInfo, detailLevel);
            }

            private CodeDocParameter CreateArgument(ParameterInfo parameterInfo, ICodeDocMemberDataProvider provider) {
                Contract.Requires(parameterInfo != null);
                Contract.Requires(provider != null);
                var parameterName = parameterInfo.Name;
                var parameterType = parameterInfo.ParameterType;
                var model = new CodeDocParameter(
                    parameterName,
                    GetOrConvert(parameterType, CodeDocMemberDetailLevel.Minimum));
                model.IsOut = parameterInfo.IsOut;
                model.IsByRef = parameterType.IsByRef;
                model.IsReferenceType = !parameterType.IsValueType;
                model.SummaryContents = provider.GetParameterSummaryContents(parameterName).ToArray();
                if (!model.NullRestricted.HasValue)
                    model.NullRestricted = provider.RequiresParameterNotEverNull(parameterName);
                return model;
            }

            private CodeDocParameter CreateReturn(ParameterInfo parameterInfo, ICodeDocMemberDataProvider provider) {
                Contract.Requires(parameterInfo != null);
                Contract.Requires(provider != null);
                Contract.Ensures(Contract.Result<CodeDocParameter>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocParameter>().Name));
                Contract.Ensures(Contract.Result<CodeDocParameter>().ParameterType != null);
                var returnType = parameterInfo.ParameterType;
                var model = new CodeDocParameter(
                    "result",
                    GetOrConvert(returnType, CodeDocMemberDetailLevel.Minimum));
                model.IsReferenceType = !returnType.IsValueType;
                model.SummaryContents = provider.GetReturnSummaryContents().ToArray();
                if (!model.NullRestricted.HasValue)
                    model.NullRestricted = provider.EnsuresResultNotEverNull;
                return model;
            }

            private CodeDocGenericParameter[] CreateGenericTypeParameters(Type[] genericArguments, ICodeDocMemberDataProvider provider) {
                Contract.Requires(genericArguments != null);
                Contract.Requires(provider != null);
                Contract.Ensures(Contract.Result<CodeDocGenericParameter[]>() != null);
                Contract.Ensures(Contract.Result<CodeDocGenericParameter[]>().Length == genericArguments.Length);
                return Array.ConvertAll(
                    genericArguments,
                    genericArgument => CreateGenericTypeParameter(genericArgument, provider));
            }

            private CodeDocGenericParameter CreateGenericTypeParameter(Type genericArgument, ICodeDocMemberDataProvider provider) {
                Contract.Requires(genericArgument != null);
                Contract.Requires(provider != null);
                Contract.Ensures(Contract.Result<CodeDocGenericParameter>() != null);
                var argumentName = genericArgument.Name;
                Contract.Assume(!String.IsNullOrEmpty(argumentName));
                var typeConstraints = genericArgument.GetGenericParameterConstraints();
                var model = new CodeDocGenericParameter(argumentName);

                model.SummaryContents = provider
                    .GetGenericTypeSummaryContents(argumentName)
                    .ToArray();

                if (typeConstraints.Length > 0)
                    model.TypeConstraints = Array.ConvertAll(typeConstraints, t => GetOrConvert(t, CodeDocMemberDetailLevel.Minimum));

                model.IsContravariant = genericArgument.GenericParameterAttributes.HasFlag(
                    GenericParameterAttributes.Contravariant);
                model.IsCovariant = genericArgument.GenericParameterAttributes.HasFlag(
                    GenericParameterAttributes.Covariant);
                model.HasDefaultConstructorConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                    GenericParameterAttributes.DefaultConstructorConstraint);
                model.HasNotNullableValueTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                    GenericParameterAttributes.NotNullableValueTypeConstraint);
                model.HasReferenceTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                    GenericParameterAttributes.ReferenceTypeConstraint);

                return model;
            }

            /// <summary>
            /// Gets a URI that links to or identifies a given member.
            /// </summary>
            /// <param name="memberInfo">The member to get a URI for.</param>
            /// <returns>A URI representing the given member.</returns>
            protected virtual Uri GetUri(MemberInfo memberInfo) {
                if (memberInfo == null)
                    return null;

                if (memberInfo is Type) {
                    var type = memberInfo as Type;
                    if (type.IsGenericType && !type.IsGenericTypeDefinition)
                        memberInfo = type.GetGenericTypeDefinition();
                }
                else if (memberInfo is MethodInfo) {
                    var methodInfo = memberInfo as MethodInfo;
                    if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
                        memberInfo = methodInfo.GetGenericMethodDefinition();
                }

                var cRef = GetGenericDefinitionCRefIdentifier(memberInfo);
                if(cRef.TargetType == "T"){
                    if(cRef.FullCRef.EndsWith("&") || cRef.FullCRef.EndsWith("@"))
                        cRef = new CRefIdentifier(cRef.FullCRef.Substring(0,cRef.FullCRef.Length-1));
                    if(cRef.FullCRef.EndsWith("]")){
                        var lastOpenSquareBracket = cRef.FullCRef.LastIndexOf('[');
                        if(lastOpenSquareBracket >= 0) {
                            var toLastSquareBracket = cRef.FullCRef.Substring(0, lastOpenSquareBracket);
                            Contract.Assume(!String.IsNullOrEmpty(toLastSquareBracket));
                            cRef = new CRefIdentifier(toLastSquareBracket);
                        }
                    }
                }
                return cRef.ToUri();
            }

            private CodeDocEvent ConvertToModel(EventInfo eventInfo, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                Contract.Requires(eventInfo != null);
                Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().ShortName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().FullName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().Title));
                Contract.Ensures(Contract.Result<CodeDocEvent>().Title == Contract.Result<CodeDocEvent>().ShortName);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().SubTitle));

                var includeInheritance = detailLevel.HasFlag(CodeDocMemberDetailLevel.Inheritance);
                var appendXmlDoc = detailLevel.HasFlag(CodeDocMemberDetailLevel.Summary) || detailLevel.HasFlag(CodeDocMemberDetailLevel.AdditionalContents);
                var provideXmlDoc = detailLevel != CodeDocMemberDetailLevel.Minimum;

                var eventCRef = GetCRefIdentifier(eventInfo);
                var model = new CodeDocEvent(eventCRef);
                model.Uri = GetUri(eventInfo);

                var memberDataProvider = new CodeDocMemberInfoProvider<EventInfo>(eventInfo);

                if (provideXmlDoc) {
                    var xmlDocs = XmlDocs.GetMember(eventCRef.FullCRef);
                    if (xmlDocs != null)
                        memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
                }

                if (extraMemberDataProvider != null)
                    memberDataProvider.Add(extraMemberDataProvider);

                if (includeInheritance) {
                    var baseEvent = eventInfo.FindNextAncestor();
                    if (baseEvent != null) {
                        var baseEventModel = GetOnly(GetCRefIdentifier(baseEvent), detailLevel);
                        if (baseEventModel != null)
                            memberDataProvider.Add(new CodeDocMemberDataProvider(baseEventModel));
                    }
                }

                if (appendXmlDoc)
                    ApplyContentXmlDocs(model, memberDataProvider);

                model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
                model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
                model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
                model.Title = model.ShortName;
                Contract.Assume(memberDataProvider.Member.DeclaringType != null);
                model.NamespaceName = memberDataProvider.Member.DeclaringType.Namespace;
                model.SubTitle = "Event";

                model.DelegateType = GetOrConvert(eventInfo.EventHandlerType, CodeDocMemberDetailLevel.Minimum);
                Contract.Assume(eventInfo.DeclaringType != null);
                model.Namespace = GetOrCreateNamespaceByName(eventInfo.DeclaringType.Namespace);
                model.Assembly = GetCodeDocAssembly(eventInfo.DeclaringType.Assembly);
                model.DeclaringType = GetOrConvert(eventInfo.DeclaringType, CodeDocMemberDetailLevel.Minimum);
                model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
                model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

                return model;
            }

            private CodeDocField ConvertToModel(FieldInfo fieldInfo, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                Contract.Requires(fieldInfo != null);
                Contract.Ensures(Contract.Result<CodeDocField>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().ShortName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().FullName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().Title));
                Contract.Ensures(Contract.Result<CodeDocField>().Title == Contract.Result<CodeDocField>().ShortName);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().SubTitle));

                var appendXmlDoc = detailLevel.HasFlag(CodeDocMemberDetailLevel.Summary) || detailLevel.HasFlag(CodeDocMemberDetailLevel.AdditionalContents);
                var provideXmlDoc = detailLevel != CodeDocMemberDetailLevel.Minimum;

                var fieldCRef = GetCRefIdentifier(fieldInfo);
                var model = new CodeDocField(fieldCRef);
                model.Uri = GetUri(fieldInfo);

                var memberDataProvider = new CodeDocMemberInfoProvider<FieldInfo>(fieldInfo);

                if (provideXmlDoc) {
                    var xmlDocs = XmlDocs.GetMember(fieldCRef.FullCRef);
                    if (xmlDocs != null)
                        memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
                }

                if (extraMemberDataProvider != null)
                    memberDataProvider.Add(extraMemberDataProvider);

                if (appendXmlDoc) {
                    model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
                    ApplyContentXmlDocs(model, memberDataProvider);
                }

                model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
                model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
                model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
                model.Title = model.ShortName;
                Contract.Assume(fieldInfo.DeclaringType != null);
                model.NamespaceName = fieldInfo.DeclaringType.Namespace;
                model.SubTitle = fieldInfo.IsLiteral ? "Constant" : "Field";

                model.ValueType = GetOrConvert(fieldInfo.FieldType, CodeDocMemberDetailLevel.Minimum);

                model.IsLiteral = fieldInfo.IsLiteral;
                model.IsInitOnly = fieldInfo.IsInitOnly;
                model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
                model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

                Contract.Assume(fieldInfo.DeclaringType != null);
                model.Namespace = GetOrCreateNamespaceByName(fieldInfo.DeclaringType.Namespace);
                model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
                model.DeclaringType = GetOrConvert(fieldInfo.DeclaringType, CodeDocMemberDetailLevel.Minimum);
                return model;
            }

            private CodeDocProperty ConvertToModel(PropertyInfo propertyInfo, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                Contract.Requires(propertyInfo != null);
                Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().ShortName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().FullName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().Title));
                Contract.Ensures(Contract.Result<CodeDocProperty>().Title == Contract.Result<CodeDocProperty>().ShortName);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().SubTitle));

                var includeExceptions = detailLevel != CodeDocMemberDetailLevel.Minimum;
                var appendXmlDoc = detailLevel.HasFlag(CodeDocMemberDetailLevel.Summary) || detailLevel.HasFlag(CodeDocMemberDetailLevel.AdditionalContents);
                var provideXmlDoc = includeExceptions || detailLevel != CodeDocMemberDetailLevel.Minimum;
                var includeInheritance = detailLevel.HasFlag(CodeDocMemberDetailLevel.Inheritance);
                var includeParameters = detailLevel != CodeDocMemberDetailLevel.Minimum;

                var propertyCRef = GetCRefIdentifier(propertyInfo);
                var model = new CodeDocProperty(propertyCRef);
                model.Uri = GetUri(propertyInfo);

                var memberDataProvider = new CodeDocMemberInfoProvider<PropertyInfo>(propertyInfo);

                XmlDocMember xmlDocs = null;
                if (provideXmlDoc) {
                    xmlDocs = XmlDocs.GetMember(propertyCRef.FullCRef);
                    if (xmlDocs != null)
                        memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
                }

                if (extraMemberDataProvider != null)
                    memberDataProvider.Add(extraMemberDataProvider);

                if (includeInheritance) {
                    var propertyBase = propertyInfo.FindNextAncestor();
                    if (propertyBase != null) {
                        var propertyBaseModel = GetOnly(GetCRefIdentifier(propertyBase), detailLevel);
                        if (propertyBaseModel != null)
                            memberDataProvider.Add(new CodeDocMemberDataProvider(propertyBaseModel));
                    }
                }

                if (appendXmlDoc) {
                    ApplyContentXmlDocs(model, memberDataProvider);
                    model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
                }

                model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
                model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
                model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
                model.Title = model.ShortName;
                Contract.Assume(propertyInfo.DeclaringType != null);
                model.NamespaceName = propertyInfo.DeclaringType.Namespace;
                model.SubTitle = propertyInfo.IsItemIndexerProperty() ? "Indexer" : "Property";

                model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
                model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

                model.ValueType = GetOrConvert(propertyInfo.PropertyType, CodeDocMemberDetailLevel.Minimum);
                Contract.Assume(propertyInfo.DeclaringType != null);
                model.Namespace = GetOrCreateNamespaceByName(propertyInfo.DeclaringType.Namespace);
                model.Assembly = GetCodeDocAssembly(propertyInfo.DeclaringType.Assembly);
                model.DeclaringType = GetOrConvert(propertyInfo.DeclaringType, CodeDocMemberDetailLevel.Minimum);

                if (includeParameters) {
                    var parameters = propertyInfo.GetIndexParameters();
                    if (parameters.Length > 0)
                        model.Parameters = Array.ConvertAll(parameters, p => CreateArgument(p, memberDataProvider));
                }

                var getterMethodInfo = propertyInfo.GetGetMethod(true);
                if (getterMethodInfo != null && ReflectionRepository.MemberFilter(getterMethodInfo, true)) {
                    var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasGetterElement)
                        ? new CodeDocMemberXmlDataProvider(xmlDocs.GetterElement)
                        : null;
                    model.Getter = ConvertToModel(getterMethodInfo, detailLevel, accessorExtraProvider);
                }

                var setterMethodInfo = propertyInfo.GetSetMethod(true);
                if (setterMethodInfo != null && ReflectionRepository.MemberFilter(setterMethodInfo, true)) {
                    var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasSetterElement)
                        ? new CodeDocMemberXmlDataProvider(xmlDocs.SetterElement)
                        : null;
                    model.Setter = ConvertToModel(setterMethodInfo, detailLevel, accessorExtraProvider);
                }

                return model;
            }

            private CodeDocMethod ConvertToModel(MethodBase methodBase, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                Contract.Requires(methodBase != null);
                Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().ShortName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().FullName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().Title));
                Contract.Ensures(Contract.Result<CodeDocMethod>().Title == Contract.Result<CodeDocMethod>().ShortName);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().SubTitle));

                var includeExceptions = detailLevel != CodeDocMemberDetailLevel.Minimum;
                var appendXmlDoc = detailLevel.HasFlag(CodeDocMemberDetailLevel.Summary) || detailLevel.HasFlag(CodeDocMemberDetailLevel.AdditionalContents);
                var provideXmlDoc = includeExceptions || detailLevel != CodeDocMemberDetailLevel.Minimum;
                var includeInheritance = detailLevel.HasFlag(CodeDocMemberDetailLevel.Inheritance);
                var includeParameters = detailLevel != CodeDocMemberDetailLevel.Minimum;

                var methodInfo = methodBase as MethodInfo;
                var methodCRef = GetCRefIdentifier(methodBase);
                var model = new CodeDocMethod(methodCRef);
                model.Uri = GetUri(methodBase);

                var memberDataProvider = new CodeDocMemberInfoProvider<MethodBase>(methodBase);

                if (provideXmlDoc) {
                    var xmlDocs = XmlDocs.GetMember(methodCRef.FullCRef);
                    if (xmlDocs != null)
                        memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
                }

                if (extraMemberDataProvider != null)
                    memberDataProvider.Add(extraMemberDataProvider);

                if (includeInheritance && methodInfo != null) {
                    var baseDefinition = methodInfo.FindNextAncestor();
                    if (baseDefinition != null) {
                        var baseDefinitionModel = GetOnly(GetCRefIdentifier(baseDefinition), detailLevel);
                        if (baseDefinitionModel != null)
                            memberDataProvider.Add(new CodeDocMemberDataProvider(baseDefinitionModel));
                    }
                }

                if (appendXmlDoc)
                    ApplyContentXmlDocs(model, memberDataProvider);

                model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
                model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(methodBase);
                model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(methodBase);
                model.Title = model.ShortName;
                Contract.Assume(methodBase.DeclaringType != null);
                model.NamespaceName = methodBase.DeclaringType.Namespace;

                if (includeParameters) {
                    model.Parameters = Array.ConvertAll(
                        methodBase.GetParameters(),
                        p => CreateArgument(p, memberDataProvider));

                    if (methodInfo != null && methodInfo.ReturnParameter != null &&
                        methodInfo.ReturnType != typeof (void))
                        model.Return = CreateReturn(methodInfo.ReturnParameter, memberDataProvider);
                }

                if (methodBase.IsConstructor)
                    model.SubTitle = "Constructor";
                else if (model.Parameters != null && model.Parameters.Count == 1 && model.HasReturn && CSharpOperatorNameSymbolMap.IsConversionOperatorMethodName(methodBase.Name)) {
                    model.SubTitle = "Conversion";

                    string conversionOperationName;
                    if (methodBase.Name.EndsWith("Explicit", StringComparison.OrdinalIgnoreCase))
                        conversionOperationName = "Explicit";
                    else if (methodBase.Name.EndsWith("Implicit", StringComparison.OrdinalIgnoreCase))
                        conversionOperationName = "Implicit";
                    else
                        conversionOperationName = String.Empty;

                    var conversionParameterPart = String.Concat(
                        model.Parameters[0].ParameterType.ShortName,
                        " to ",
                        model.Return.ParameterType.ShortName);

                    model.ShortName = String.IsNullOrEmpty(conversionOperationName)
                        ? conversionParameterPart
                        : String.Concat(conversionOperationName, ' ', conversionParameterPart);
                    model.Title = model.ShortName;
                }
                else if (methodBase.IsOperatorOverload())
                    model.SubTitle = "Operator";
                else
                    model.SubTitle = "Method";

                Contract.Assume(methodBase.DeclaringType != null);
                model.Namespace = GetOrCreateNamespaceByName(methodBase.DeclaringType.Namespace);
                model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
                model.DeclaringType = GetOrConvert(methodBase.DeclaringType, CodeDocMemberDetailLevel.Minimum);
                model.IsOperatorOverload = methodBase.IsOperatorOverload();
                model.IsExtensionMethod = methodBase.IsExtensionMethod();
                model.IsSealed = methodBase.IsSealed();
                model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
                model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();
                model.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

                if (methodBase.DeclaringType != null && !methodBase.DeclaringType.IsInterface) {
                    if (methodBase.IsAbstract)
                        model.IsAbstract = true;
                    else if (methodBase.IsVirtual && !methodBase.IsFinal && methodBase.Attributes.HasFlag(MethodAttributes.NewSlot))
                        model.IsVirtual = true;
                }

                if (includeExceptions){
                    if (memberDataProvider.HasExceptions)
                        model.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
                    if (memberDataProvider.HasEnsures)
                        model.Ensures = memberDataProvider.GetEnsures().ToArray();
                    if (memberDataProvider.HasRequires)
                        model.Requires = memberDataProvider.GetRequires().ToArray();
                }

                if (includeParameters && methodBase.IsGenericMethod) {
                    var genericDefinition = methodBase.IsGenericMethodDefinition
                        ? methodBase
                        : methodInfo == null ? null : methodInfo.GetGenericMethodDefinition();
                    if (genericDefinition != null) {
                        var genericArguments = genericDefinition.GetGenericArguments();
                        if (genericArguments.Length > 0) {
                            model.GenericParameters = CreateGenericTypeParameters(
                                genericArguments,
                                memberDataProvider);
                        }
                    }
                }

                return model;
            }

            private bool ImplicitlyInheritDataProvider(Type type) {
                if (type == null)
                    return false;
                if (type.IsValueType || type.IsEnum)
                    return false;
                if (type == typeof (object) || type == typeof (Enum))
                    return false;
                return true;
            }

            private CodeDocType ConvertToModel(Type type, CodeDocMemberDetailLevel detailLevel, ICodeDocMemberDataProvider extraMemberDataProvider = null) {
                Contract.Requires(type != null);
                Contract.Ensures(Contract.Result<CodeDocType>() != null);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().ShortName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().FullName));
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().Title));
                Contract.Ensures(Contract.Result<CodeDocType>().Title == Contract.Result<CodeDocType>().ShortName);
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().SubTitle));

                var includeExceptions = detailLevel != CodeDocMemberDetailLevel.Minimum;
                var appendXmlDoc = detailLevel.HasFlag(CodeDocMemberDetailLevel.Summary) || detailLevel.HasFlag(CodeDocMemberDetailLevel.AdditionalContents);
                var provideXmlDoc = includeExceptions || detailLevel != CodeDocMemberDetailLevel.Minimum;
                var includeMembers = detailLevel.HasFlag(CodeDocMemberDetailLevel.Members);
                var includeInheritance = detailLevel.HasFlag(CodeDocMemberDetailLevel.Inheritance);
                var includeParameters = detailLevel != CodeDocMemberDetailLevel.Minimum;

                var cRef = GetCRefIdentifier(type);
                var model = type.IsDelegateType()
                    ? new CodeDocDelegate(cRef)
                    : new CodeDocType(cRef);
                model.Uri = GetUri(type);
                var delegateModel = model as CodeDocDelegate;

                var memberDataProvider = new CodeDocMemberInfoProvider<Type>(type);

                XmlDocMember xmlDocs = null;
                if (provideXmlDoc) {
                    xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
                    if (xmlDocs != null)
                        memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
                }

                if (extraMemberDataProvider != null)
                    memberDataProvider.Add(extraMemberDataProvider);

                if (includeInheritance && type.BaseType != null) {
                    if (ImplicitlyInheritDataProvider(type.BaseType) || (xmlDocs != null && xmlDocs.HasInheritDoc)) {
                        var baseModel = GetOrConvert(type.BaseType, detailLevel);
                        memberDataProvider.Add(new CodeDocMemberDataProvider(baseModel));
                    }
                }

                if (appendXmlDoc)
                    ApplyContentXmlDocs(model, memberDataProvider);

                model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
                model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(type);
                model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(type);
                model.Title = model.ShortName;
                model.NamespaceName = type.Namespace;

                if (type.IsEnum)
                    model.SubTitle = "Enumeration";
                else if (type.IsValueType)
                    model.SubTitle = "Structure";
                else if (type.IsInterface)
                    model.SubTitle = "Interface";
                else if (type.IsDelegateType())
                    model.SubTitle = "Delegate";
                else
                    model.SubTitle = "Class";

                model.Namespace = GetOrCreateNamespaceByName(type.Namespace);
                model.Assembly = GetCodeDocAssembly(type.Assembly);
                model.IsEnum = type.IsEnum;
                model.IsFlagsEnum = type.IsEnum && type.HasAttribute(typeof(FlagsAttribute));
                model.IsSealed = type.IsSealed;
                model.IsValueType = type.IsValueType;
                model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
                model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

                if (type.DeclaringType != null)
                    model.DeclaringType = GetOrConvert(type.DeclaringType, CodeDocMemberDetailLevel.Minimum);

                if (delegateModel != null)
                    delegateModel.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

                if (includeInheritance && !type.IsInterface) {
                    var currentBase = type.BaseType;
                    if (null != currentBase) {
                        var baseChain = new List<CodeDocType>() {
                            GetOrConvert(currentBase, detailLevel)
                        };
                        currentBase = currentBase.BaseType;
                        while (currentBase != null) {
                            baseChain.Add(GetOrConvert(currentBase, CodeDocMemberDetailLevel.Minimum));
                            currentBase = currentBase.BaseType;
                        }
                        model.BaseChain = baseChain;
                    }

                    var implementedInterfaces = type.GetInterfaces();
                    if (implementedInterfaces.Length > 0) {
                        model.Interfaces = Array.ConvertAll(
                            implementedInterfaces,
                            t => GetOrConvert(t, CodeDocMemberDetailLevel.Minimum));
                    }
                }

                if (includeParameters && type.IsGenericType) {
                    var genericDefinition = type.IsGenericTypeDefinition
                        ? type
                        : type.GetGenericTypeDefinition();
                    if (genericDefinition != null) {
                        var genericArguments = genericDefinition.GetGenericArguments();
                        if (type.IsNested && genericArguments.Length > 0) {
                            Contract.Assume(type.DeclaringType != null);
                            var parentGenericArguments = type.DeclaringType.GetGenericArguments();
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

                if (delegateModel != null) {
                    if (includeParameters) {
                        delegateModel.Parameters = Array.ConvertAll(
                            type.GetDelegateTypeParameters(),
                            p => CreateArgument(p, memberDataProvider));

                        var returnParameter = type.GetDelegateReturnParameter();
                        if (returnParameter != null && returnParameter.ParameterType != typeof (void))
                            delegateModel.Return = CreateReturn(returnParameter, memberDataProvider);
                    }

                    if (includeExceptions) {
                        if (memberDataProvider.HasExceptions)
                            delegateModel.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
                        if (memberDataProvider.HasEnsures)
                            delegateModel.Ensures = memberDataProvider.GetEnsures().ToArray();
                        if (memberDataProvider.HasRequires)
                            delegateModel.Requires = memberDataProvider.GetRequires().ToArray();
                    }
                }

                if (includeMembers) {
                    var nestedTypeModels = new List<ICodeDocMember>();
                    var nestedDelegateModels = new List<ICodeDocMember>();
                    foreach (var nestedType in type.GetAllNestedTypes().Where(ReflectionRepository.MemberFilter)) {
                        var nestedTypeModel = GetOrConvert(nestedType, CodeDocMemberDetailLevel.QuickSummary);
                        if (nestedType.IsDelegateType())
                            nestedDelegateModels.Add(nestedTypeModel);
                        else
                            nestedTypeModels.Add(nestedTypeModel);
                    }
                    model.NestedTypes = nestedTypeModels;
                    model.NestedDelegates = nestedDelegateModels;

                    var methodModels = new List<ICodeDocMember>();
                    var operatorModels = new List<ICodeDocMember>();
                    foreach (var methodInfo in type.GetAllMethods().Where(ReflectionRepository.MemberFilter)) {
                        var methodModel = GetOrConvert(methodInfo, CodeDocMemberDetailLevel.QuickSummary);
                        if (methodInfo.IsOperatorOverload())
                            operatorModels.Add(methodModel);
                        else
                            methodModels.Add(methodModel);
                    }
                    model.Methods = methodModels;
                    model.Operators = operatorModels;

                    model.Constructors = type
                        .GetAllConstructors()
                        .Where(ReflectionRepository.MemberFilter)
                        .Select(methodInfo => GetOrConvert(methodInfo, CodeDocMemberDetailLevel.QuickSummary))
                        .ToArray();

                    model.Properties = type
                        .GetAllProperties()
                        .Where(ReflectionRepository.MemberFilter)
                        .Select(propertyInfo => GetOrConvert(propertyInfo, CodeDocMemberDetailLevel.QuickSummary))
                        .ToArray();

                    model.Fields = type
                        .GetAllFields()
                        .Where(ReflectionRepository.MemberFilter)
                        .Select(fieldInfo => GetOrConvert(fieldInfo, CodeDocMemberDetailLevel.QuickSummary))
                        .ToArray();

                    model.Events = type
                        .GetAllEvents()
                        .Where(ReflectionRepository.MemberFilter)
                        .Select(eventInfo => GetOrConvert(eventInfo, CodeDocMemberDetailLevel.QuickSummary))
                        .ToArray();
                }

                return model;
            }

        }

    }
}
