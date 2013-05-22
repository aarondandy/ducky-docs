﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using DandyDoc.CRef;
using DandyDoc.CodeDoc.Utility;
using DandyDoc.DisplayName;
using DandyDoc.ExternalVisibility;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
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

        private static CRefIdentifier GetCRefIdentifier(MemberInfo memberInfo){
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);
            return new CRefIdentifier(ReflectionCRefGenerator.WithPrefix.GetCRef(memberInfo));
        }

        private static CRefIdentifier GetCRefIdentifier(Assembly assembly) {
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

            var assemblyModels = new List<CodeDocSimpleAssembly>();
            var namespaceModels = new Dictionary<string, CodeDocSimpleNamespace>();

            foreach (var assembly in CRefLookup.Assemblies){
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

                var assemblyTypeCRefs = new List<CRefIdentifier>();
                var assemblyNamespaceNames = new HashSet<string>();
                foreach (var type in assembly
                    .GetTypes()
                    .Where(t => !t.IsNested)
                    .Where(MemberFilter)
                ){
                    var typeCRef = GetCRefIdentifier(type);
                    assemblyTypeCRefs.Add(typeCRef);
                    var namespaceName = type.Namespace;
                    if (String.IsNullOrWhiteSpace(namespaceName))
                        namespaceName = String.Empty;

                    CodeDocSimpleNamespace namespaceModel;
                    if (!namespaceModels.TryGetValue(namespaceName, out namespaceModel)) {
                        var namespaceTitle = String.IsNullOrEmpty(namespaceName) ? "global" : namespaceName;
                        Contract.Assume(!String.IsNullOrEmpty("N:" + namespaceName));
                        namespaceModel = new CodeDocSimpleNamespace(new CRefIdentifier("N:" + namespaceName)){
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
            if (!isPropertyMethod && methodBase.IsSpecialName && !methodBase.IsConstructor && !methodBase.IsOperatorOverload())
                return false;
            if (methodBase.IsFinalizer())
                return false;
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
        /// Gets a code doc assembly from a reflected assembly.
        /// </summary>
        /// <param name="assembly">An assembly to get the code doc model for.</param>
        /// <returns>The assembly code doc model if found.</returns>
        private CodeDocSimpleAssembly GetCodeDocAssembly(Assembly assembly) {
            Contract.Requires(assembly != null);
            return GetCodeDocSimpleAssembly(GetCRefIdentifier(assembly));
        }

        [Obsolete]
        public override ICodeDocMember GetSimpleMember(CRefIdentifier cRef) {
            throw new NotSupportedException();
        }

        [Obsolete]
        public override ICodeDocMember GetContentMember(CRefIdentifier cRef) {
            throw new NotSupportedException();
        }

        protected override ICodeDocMember GetMemberModel(CRefIdentifier cRef, bool lite) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;

            return ConvertToModel(memberInfo, lite: lite);
        }

        /// <summary>
        /// Converts a reflected member to a code doc model.
        /// </summary>
        /// <param name="memberInfo">A reflected member.</param>
        /// <returns>A code doc model for the given member.</returns>
        protected virtual CodeDocMemberContentBase ConvertToModel(MemberInfo memberInfo, bool lite) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            if (memberInfo is Type)
                return ConvertToModel((Type)memberInfo, lite: lite);
            if (memberInfo is FieldInfo)
                return ConvertToModel((FieldInfo)memberInfo);
            if (memberInfo is MethodBase)
                return ConvertToModel((MethodBase) memberInfo);
            if (memberInfo is EventInfo)
                return ConvertToModel((EventInfo) memberInfo);
            if (memberInfo is PropertyInfo)
                return ConvertToModel((PropertyInfo) memberInfo);
            throw new NotSupportedException();
        }

        private CodeDocType GetOrConvert(Type type, bool lite = false){
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it
            return ConvertToModel(type, lite: lite);
        }

        private CodeDocParameter CreateArgument(ParameterInfo parameterInfo, ICodeDocMemberDataProvider provider) {
            Contract.Requires(parameterInfo != null);
            Contract.Requires(provider != null);
            var parameterName = parameterInfo.Name;
            var parameterType = parameterInfo.ParameterType;
            var model = new CodeDocParameter(
                parameterName,
                GetOrConvert(parameterType, lite: true)
            );
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
                GetOrConvert(returnType, lite: true)
            );
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
                model.TypeConstraints = Array.ConvertAll(typeConstraints, t => GetOrConvert(t, lite: true));

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

        private CodeDocEvent ConvertToModel(EventInfo eventInfo){
            Contract.Requires(eventInfo != null);
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().Title));
            Contract.Ensures(Contract.Result<CodeDocEvent>().Title == Contract.Result<CodeDocEvent>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocEvent>().SubTitle));

            var eventCRef = GetCRefIdentifier(eventInfo);
            var model = new CodeDocEvent(eventCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<EventInfo>(eventInfo);
            var xmlDocs = XmlDocs.GetMember(eventCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(memberDataProvider.Member);
            model.Title = model.ShortName;
            Contract.Assume(memberDataProvider.Member.DeclaringType != null);
            model.NamespaceName = memberDataProvider.Member.DeclaringType.Namespace;
            model.SubTitle = "Event";

            model.DelegateType = GetOrConvert(eventInfo.EventHandlerType, lite: true);
            Contract.Assume(eventInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(eventInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvert(eventInfo.DeclaringType, lite: true);
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            return model;
        }

        private CodeDocField ConvertToModel(FieldInfo fieldInfo) {
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().Title));
            Contract.Ensures(Contract.Result<CodeDocField>().Title == Contract.Result<CodeDocField>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocField>().SubTitle));

            var fieldCRef = GetCRefIdentifier(fieldInfo);
            var model = new CodeDocField(fieldCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<FieldInfo>(fieldInfo);
            var xmlDocs = XmlDocs.GetMember(fieldCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.Title = model.ShortName;
            Contract.Assume(fieldInfo.DeclaringType != null);
            model.NamespaceName = fieldInfo.DeclaringType.Namespace;

            model.SubTitle = fieldInfo.IsLiteral ? "Constant" : "Field";

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
            model.ValueType = GetOrConvert(fieldInfo.FieldType, lite: true);
            model.IsLiteral = fieldInfo.IsLiteral;
            model.IsInitOnly = fieldInfo.IsInitOnly;
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            Contract.Assume(fieldInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(fieldInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvert(fieldInfo.DeclaringType, lite: true);
            return model;
        }

        private CodeDocProperty ConvertToModel(PropertyInfo propertyInfo){
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().Title));
            Contract.Ensures(Contract.Result<CodeDocProperty>().Title == Contract.Result<CodeDocProperty>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocProperty>().SubTitle));

            var propertyCRef = GetCRefIdentifier(propertyInfo);
            var model = new CodeDocProperty(propertyCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<PropertyInfo>(propertyInfo);
            var xmlDocs = XmlDocs.GetMember(propertyCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.Title = model.ShortName;
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.NamespaceName = propertyInfo.DeclaringType.Namespace;

            model.SubTitle = propertyInfo.IsItemIndexerProperty() ? "Indexer" : "Property";

            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToArray();
            model.ValueType = GetOrConvert(propertyInfo.PropertyType, lite: true);
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(propertyInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(propertyInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvert(propertyInfo.DeclaringType, lite: true);

            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0)
                model.Parameters = Array.ConvertAll(parameters,p => CreateArgument(p, memberDataProvider));

            var getterMethodInfo = propertyInfo.GetGetMethod(true);
            if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasGetterElement)
                    ? new CodeDocMemberXmlDataProvider(xmlDocs.GetterElement)
                    : null;
                model.Getter = ConvertToModel(getterMethodInfo, accessorExtraProvider);
            }

            var setterMethodInfo = propertyInfo.GetSetMethod(true);
            if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                var accessorExtraProvider = (xmlDocs != null && xmlDocs.HasSetterElement)
                    ? new CodeDocMemberXmlDataProvider(xmlDocs.SetterElement)
                    : null;
                model.Setter = ConvertToModel(setterMethodInfo, accessorExtraProvider);
            }

            return model;
        }

        private CodeDocMethod ConvertToModel(MethodBase methodBase, ICodeDocMemberDataProvider extraMemberDataProvider = null){
            Contract.Requires(methodBase != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().Title));
            Contract.Ensures(Contract.Result<CodeDocMethod>().Title == Contract.Result<CodeDocMethod>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocMethod>().SubTitle));

            var methodCRef = GetCRefIdentifier(methodBase);
            var model = new CodeDocMethod(methodCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<MethodBase>(methodBase);
            var xmlDocs = XmlDocs.GetMember(methodCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));
            if (extraMemberDataProvider != null)
                memberDataProvider.Add(extraMemberDataProvider);

            ApplyContentXmlDocs(model, memberDataProvider);

            model.ExternalVisibility = memberDataProvider.ExternalVisibility ?? ExternalVisibilityKind.Public;
            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(methodBase);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(methodBase);
            model.Title = model.ShortName;
            Contract.Assume(methodBase.DeclaringType != null);
            model.NamespaceName = methodBase.DeclaringType.Namespace;

            if (methodBase.IsConstructor)
                model.SubTitle = "Constructor";
            else if (methodBase.IsOperatorOverload())
                model.SubTitle = "Operator";
            else
                model.SubTitle = "Method";

            Contract.Assume(methodBase.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodBase.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvert(methodBase.DeclaringType, lite: true);
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

            model.Parameters = Array.ConvertAll(
                methodBase.GetParameters(),
                p => CreateArgument(p, memberDataProvider));

            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void))
                model.Return = CreateReturn(methodInfo.ReturnParameter, memberDataProvider);

            if (memberDataProvider.HasExceptions)
                model.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
            if (memberDataProvider.HasEnsures)
                model.Ensures = memberDataProvider.GetEnsures().ToArray();
            if (memberDataProvider.HasRequires)
                model.Requires = memberDataProvider.GetRequires().ToArray();

            if (methodBase.IsGenericMethod){
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

        private CodeDocType ConvertToModel(Type type, bool lite = false) {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().FullName));
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().Title));
            Contract.Ensures(Contract.Result<CodeDocType>().Title == Contract.Result<CodeDocType>().ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<CodeDocType>().SubTitle));

            var cRef = GetCRefIdentifier(type);
            var model = type.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);
            var delegateModel = model as CodeDocDelegate;

            var memberDataProvider = new CodeDocMemberInfoProvider<Type>(type);
            var xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

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

            model.Namespace = GetCodeDocNamespaceByName(type.Namespace);
            model.Assembly = GetCodeDocAssembly(type.Assembly);
            model.IsEnum = type.IsEnum;
            model.IsFlagsEnum = type.IsEnum && type.HasAttribute(typeof(FlagsAttribute));
            model.IsSealed = type.IsSealed;
            model.IsValueType = type.IsValueType;
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            if (type.DeclaringType != null)
                model.DeclaringType = GetOrConvert(type.DeclaringType, lite: true);

            if (delegateModel != null)
                delegateModel.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

            if (lite)
                return model;

            if (!type.IsInterface) {
                var currentBase = type.BaseType;
                if (null != currentBase) {
                    var baseChain = new List<CodeDocType>() {
                        GetOrConvert(currentBase, lite: true)
                    };
                    currentBase = currentBase.BaseType;
                    while (currentBase != null) {
                        baseChain.Add(GetOrConvert(currentBase, lite: true));
                        currentBase = currentBase.BaseType;
                    }
                    model.BaseChain = baseChain;
                }
            }

            var implementedInterfaces = type.GetInterfaces();
            if (implementedInterfaces.Length > 0) {
                model.Interfaces = Array.ConvertAll(
                    implementedInterfaces,
                    t => GetOrConvert(t, lite: true));
            }

            var nestedTypeModels = new List<ICodeDocMember>();
            var nestedDelegateModels = new List<ICodeDocMember>();
            foreach (var nestedType in type.GetAllNestedTypes().Where(MemberFilter)) {
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
            foreach (var methodInfo in type.GetAllMethods().Where(MemberFilter)) {
                var methodModel = ConvertToModel(methodInfo);
                if (methodInfo.IsOperatorOverload())
                    operatorModels.Add(methodModel);
                else
                    methodModels.Add(methodModel);
            }
            model.Methods = methodModels;
            model.Operators = operatorModels;

            model.Constructors = type
                .GetAllConstructors()
                .Where(MemberFilter)
                .Select(m => ConvertToModel(m))
                .ToArray();

            model.Properties = type
                .GetAllProperties()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray();

            model.Fields = type
                .GetAllFields()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray();

            model.Events = type
                .GetAllEvents()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray();

            if (delegateModel != null) {
                delegateModel.Parameters = Array.ConvertAll(
                    type.GetDelegateTypeParameters(),
                    p => CreateArgument(p, memberDataProvider));

                var returnParameter = type.GetDelegateReturnParameter();
                if (returnParameter != null && returnParameter.ParameterType != typeof(void))
                    delegateModel.Return = CreateReturn(returnParameter, memberDataProvider);

                if (memberDataProvider.HasExceptions)
                    delegateModel.Exceptions = CreateExceptionModels(memberDataProvider.GetExceptions()).ToArray();
                if (memberDataProvider.HasEnsures)
                    delegateModel.Ensures = memberDataProvider.GetEnsures().ToArray();
                if (memberDataProvider.HasRequires)
                    delegateModel.Requires = memberDataProvider.GetRequires().ToArray();
            }

            if (type.IsGenericType) {
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

            return model;
        }

    }
}
