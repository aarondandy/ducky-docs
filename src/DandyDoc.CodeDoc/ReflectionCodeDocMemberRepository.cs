using System;
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
            : this(cRefLookup, null)
        {
            Contract.Requires(cRefLookup != null);
        }

        /// <summary>
        /// Creates a new reflection code doc repository.
        /// </summary>
        /// <param name="cRefLookup">The lookup used to resolve code references into reflected members.</param>
        /// <param name="xmlDocs">The related XML documentation files for the members.</param>
        public ReflectionCodeDocMemberRepository(ReflectionCRefLookup cRefLookup, params XmlAssemblyDocument[] xmlDocs)
            : this(cRefLookup, (IEnumerable<XmlAssemblyDocument>)xmlDocs)
        {
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

        private ICodeDocMember GetSimpleEntity(MemberInfo memberInfo) {
            Contract.Requires(memberInfo != null);

            // TODO: check a pool of other repositories if not found in this one

            if (MemberFilter(memberInfo))
                return ConvertToSimpleModel(memberInfo);

            return CreateSimpleEntityPlaceholder(memberInfo);
        }

        private ICodeDocMember CreateSimpleEntityPlaceholder(MemberInfo memberInfo){
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            var memberDataProvider = new CodeDocMemberInfoProvider<MemberInfo>(memberInfo);
            // TODO: better values are needed for these properties
            var result = new CodeDocSimpleMember(GetCRefIdentifier(memberInfo)) {
                ShortName = memberInfo.Name,
                Title = memberInfo.Name,
                SubTitle = String.Empty,
                FullName = memberInfo.Name,
                NamespaceName = String.Empty,
            };
            ApplySimpleAttributes(result, memberDataProvider);
            return result;
        }

        /// <inheritdoc/>
        public override ICodeDocMember GetSimpleMember(CRefIdentifier cRef) {
            /*if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleNamespace(cRef);
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;
            return ConvertToSimpleModel(memberInfo);*/
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override ICodeDocMember GetContentMember(CRefIdentifier cRef) {
            /*
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;
            return ConvertToModel(memberInfo);
            */
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

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;

            return ConvertToModel(memberInfo);
        }

        /// <summary>
        /// Converts a reflected member to a simple code doc model.
        /// </summary>
        /// <param name="memberInfo">A reflected member.</param>
        /// <returns>A code doc model for the given member.</returns>
        [Obsolete]
        protected virtual ICodeDocMember ConvertToSimpleModel(MemberInfo memberInfo) {
            /*if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);
            var cRef = GetCRefIdentifier(memberInfo);
            var result = new CodeDocSimpleMember(cRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<MemberInfo>(memberInfo);
            var xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplySimpleXmlDocs(result, memberDataProvider);

            if (memberInfo is Type)
                ApplyCommonAttributes(result, (Type)memberInfo);
            else if (memberInfo is MethodBase)
                ApplyCommonAttributes(result, (MethodBase) memberInfo);
            else if (memberInfo is FieldInfo)
                ApplyCommonAttributes(result, (FieldInfo)memberInfo);
            else if (memberInfo is EventInfo)
                ApplyCommonAttributes(result, (EventInfo)memberInfo);
            else if (memberInfo is PropertyInfo)
                ApplyCommonAttributes(result, (PropertyInfo)memberInfo);
            else
                throw new NotSupportedException();

            return result;*/
            throw new NotSupportedException();
        }

        /// <summary>
        /// Converts a reflected member to a code doc model.
        /// </summary>
        /// <param name="memberInfo">A reflected member.</param>
        /// <returns>A code doc model for the given member.</returns>
        protected virtual CodeDocMemberContentBase ConvertToModel(MemberInfo memberInfo) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocMember>() != null);

            if (memberInfo is Type)
                return ConvertToModel((Type)memberInfo);
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

        [Obsolete]
        private void ApplySimpleAttributes(CodeDocSimpleMember member, MemberInfo memberInfo) {
            Contract.Requires(member != null);
            Contract.Requires(memberInfo != null);
            member.ExternalVisibility = memberInfo.GetExternalVisibility();
        }

        private void ApplySimpleAttributes(CodeDocSimpleMember model, ICodeDocMemberDataProvider provider) {
            Contract.Requires(model != null);
            Contract.Requires(provider != null);
            model.ExternalVisibility = provider.ExternalVisibility ?? ExternalVisibilityKind.Public;
        }

        private void ApplyContentXmlDocs(CodeDocMemberContentBase model, ICodeDocMemberDataProvider provider) {
            Contract.Requires(model != null);
            Contract.Requires(provider != null);
            model.SummaryContents = provider.GetSummaryContents().ToList();
            model.Examples = provider.GetExamples().ToList();
            model.Permissions = provider.GetPermissions().ToList();
            model.Remarks = provider.GetRemarks().ToList();
            model.SeeAlso = provider.GetSeeAlsos().ToList();
        }

        private CodeDocType GetOrConvert(Type type){
            Contract.Requires(type != null);
            // TODO:
            // 1) If we handle this type, convert to it
            // 2) If somebody else handles this type, get it from them
            // 3) Try to make a placeholder model for it
            return ConvertToModel(type);
        }

        private CodeDocSimpleAssembly GetOrConvert(Assembly assembly){
            Contract.Requires(assembly != null);
            // TODO:
            // 1) If we handle this assembly, convert to it
            // 2) If somebody else handles this assembly, get it from them
            // 3) Try to make a placeholder model for it
            return GetCodeDocAssembly(assembly);
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

            model.DelegateType = GetOrConvert(eventInfo.EventHandlerType);
            Contract.Assume(eventInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventInfo.DeclaringType.Namespace);
            model.Assembly = GetOrConvert(eventInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvert(eventInfo.DeclaringType);
            model.IsStatic = memberDataProvider.IsStatic.GetValueOrDefault();
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();

            return model;
        }

        private CodeDocField ConvertToModel(FieldInfo fieldInfo) {
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var fieldCRef = GetCRefIdentifier(fieldInfo);
            var model = new CodeDocField(fieldCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<FieldInfo>(fieldInfo);
            var xmlDocs = XmlDocs.GetMember(fieldCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);
            ApplyCommonAttributes(model, fieldInfo);

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToList();
            model.ValueType = GetSimpleEntity(fieldInfo.FieldType);
            model.IsLiteral = fieldInfo.IsLiteral;
            model.IsInitOnly = fieldInfo.IsInitOnly;
            model.IsStatic = fieldInfo.IsStatic;
            model.IsObsolete = fieldInfo.HasAttribute(typeof(ObsoleteAttribute));

            Contract.Assume(fieldInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(fieldInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(fieldInfo.DeclaringType);
            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, FieldInfo fieldInfo) {
            Contract.Requires(model != null);
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleAttributes(model, fieldInfo);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.Title = model.ShortName;
            Contract.Assume(fieldInfo.DeclaringType != null);
            model.NamespaceName = fieldInfo.DeclaringType.Namespace;

            if (fieldInfo.IsLiteral)
                model.SubTitle = "Constant";
            else
                model.SubTitle = "Field";
        }

        private void ApplyCommonAttributes(CodeDocParameter model, ParameterInfo parameterInfo){
            Contract.Requires(model != null);
            if (parameterInfo != null) {
                model.IsOut = parameterInfo.IsOut;
                model.IsByRef = parameterInfo.ParameterType.IsByRef;

                model.NullRestricted = null;
                foreach (var attribute in parameterInfo.GetCustomAttributesData()) {
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

        private void ApplyReturnEntityAttributes(CodeDocParameter model, Type returnType, XmlDocMember xmlDocs) {
            Contract.Requires(model != null);
            ApplyCommonAttributes(model, null);
            if (!model.NullRestricted.HasValue && xmlDocs != null) {
                if (xmlDocs.HasEnsuresElements && xmlDocs.EnsuresElements.Any(c => c.EnsuresResultNotEverNull))
                    model.NullRestricted = true;
            }
            if (returnType != null) {
                model.IsReferenceType = !returnType.IsValueType;
            }
        }

        private void ApplyArgumentEntityAttributes(CodeDocParameter model, ParameterInfo parameterInfo, XmlDocMember xmlDocs) {
            Contract.Requires(model != null);
            ApplyCommonAttributes(model, parameterInfo);
            if (!model.NullRestricted.HasValue && xmlDocs != null) {
                if(xmlDocs.HasRequiresElements && xmlDocs.RequiresElements.Any(c => c.RequiresParameterNotEverNull(parameterInfo.Name)))
                    model.NullRestricted = true;
            }
            if (parameterInfo != null) {
                model.IsReferenceType = !parameterInfo.ParameterType.IsValueType;
            }
        }

        private CodeDocProperty ConvertToModel(PropertyInfo propertyInfo){
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var propertyCRef = GetCRefIdentifier(propertyInfo);
            var model = new CodeDocProperty(propertyCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<PropertyInfo>(propertyInfo);
            var xmlDocs = XmlDocs.GetMember(propertyCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);
            ApplyCommonAttributes(model, propertyInfo);

            model.IsStatic = propertyInfo.IsStatic();
            model.IsObsolete = propertyInfo.HasAttribute(typeof(ObsoleteAttribute));

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToList();

            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0){
                model.Parameters = new ReadOnlyCollection<CodeDocParameter>(Array.ConvertAll(parameters,
                    parameter => {
                        var parameterSummaryElement = xmlDocs != null
                            ? xmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetSimpleEntity(parameter.ParameterType),
                            parameterSummaryElement
                        );
                        ApplyArgumentEntityAttributes(paramModel, parameter, xmlDocs);
                        return paramModel;
                    }
                ));
            }

            var getterMethodInfo = propertyInfo.GetGetMethod(true);
            if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                var accessor = ConvertToModel(getterMethodInfo);
                if (model.XmlDocs != null && model.XmlDocs.HasGetterElement) {
                    accessor.XmlDocs = model.XmlDocs.GetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Getter = accessor;
            }

            var setterMethodInfo = propertyInfo.GetSetMethod(true);
            if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                var accessor = ConvertToModel(setterMethodInfo);
                if (model.XmlDocs != null && model.XmlDocs.HasSetterElement) {
                    accessor.XmlDocs = model.XmlDocs.SetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Setter = accessor;
            }

            model.ValueType = GetSimpleEntity(propertyInfo.PropertyType);
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(propertyInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(propertyInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(propertyInfo.DeclaringType);

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, PropertyInfo propertyInfo) {
            Contract.Requires(model != null);
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleAttributes(model, propertyInfo);

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.Title = model.ShortName;
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.NamespaceName = propertyInfo.DeclaringType.Namespace;

            if (propertyInfo.IsItemIndexerProperty())
                model.SubTitle = "Indexer";
            else
                model.SubTitle = "Property";

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

        private CodeDocMethod ConvertToModel(MethodBase methodBase){
            Contract.Requires(methodBase != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var methodCRef = GetCRefIdentifier(methodBase);
            var model = new CodeDocMethod(methodCRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<MethodBase>(methodBase);
            var xmlDocs = XmlDocs.GetMember(methodCRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);
            ApplyCommonAttributes(model, methodBase);

            var methodInfo = methodBase as MethodInfo;

            Contract.Assume(methodBase.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodBase.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(methodBase.DeclaringType);
            model.IsOperatorOverload = methodBase.IsOperatorOverload();
            model.IsExtensionMethod = methodBase.IsExtensionMethod();
            model.IsSealed = methodBase.IsSealed();
            model.IsStatic = methodBase.IsStatic;
            model.IsObsolete = methodBase.HasAttribute(typeof(ObsoleteAttribute));
            
            if (methodBase.DeclaringType != null && !methodBase.DeclaringType.IsInterface) {
                if (methodBase.IsAbstract)
                    model.IsAbstract = true;
                else if (methodBase.IsVirtual && !methodBase.IsFinal && methodBase.Attributes.HasFlag(MethodAttributes.NewSlot))
                    model.IsVirtual = true;
            }

            model.IsPure = memberDataProvider.IsPure.GetValueOrDefault();

            var parameterModels = methodBase
                .GetParameters()
                .Select(parameter => {
                    var parameterSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.GetParameterSummary(parameter.Name)
                        : null;
                    var paramModel = new CodeDocParameter(
                        parameter.Name,
                        GetSimpleEntity(parameter.ParameterType),
                        parameterSummaryElement
                    );
                    ApplyArgumentEntityAttributes(paramModel, parameter, model.XmlDocs);
                    return paramModel;
                })
                .ToArray();
            model.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameterModels);

            if (methodInfo != null) {
                if (methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void)) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    var paramModel = new CodeDocParameter(
                        String.Empty,
                        GetSimpleEntity(methodInfo.ReturnType),
                        returnSummaryElement
                    );
                    ApplyReturnEntityAttributes(paramModel, methodInfo.ReturnParameter.ParameterType, model.XmlDocs);
                    model.Return = paramModel;
                }
            }

            ExpandCodeDocMethodXmlDocProperties(model);

            if (methodBase.IsGenericMethod){
                var genericDefinition = methodBase.IsGenericMethodDefinition
                    ? methodBase
                    : methodInfo == null ? null : methodInfo.GetGenericMethodDefinition();
                if (genericDefinition != null){
                    var genericArguments = genericDefinition.GetGenericArguments();
                    if (genericArguments.Length > 0){
                        //var xmlDocs = XmlDocs.GetMember(GetCRefIdentifier(methodBase).FullCRef);
                        var genericModels = new List<CodeDocGenericParameter>();
                        foreach (var genericArgument in genericArguments){
                            var argumentName = genericArgument.Name;
                            Contract.Assume(!String.IsNullOrEmpty(argumentName));
                            var typeConstraints = genericArgument.GetGenericParameterConstraints();
                            var genericModel = new CodeDocGenericParameter{
                                Name = argumentName
                            };

                            if (xmlDocs != null)
                                genericModel.Summary = xmlDocs.GetTypeParameterSummary(argumentName);
                            if (typeConstraints.Length > 0)
                                genericModel.TypeConstraints = Array.AsReadOnly(Array.ConvertAll(typeConstraints, GetSimpleEntity));

                            genericModel.IsContravariant = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.Contravariant);
                            genericModel.IsCovariant = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.Covariant);
                            genericModel.HasDefaultConstructorConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.DefaultConstructorConstraint);
                            genericModel.HasNotNullableValueTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.NotNullableValueTypeConstraint);
                            genericModel.HasReferenceTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.ReferenceTypeConstraint);

                            genericModels.Add(genericModel);

                        }
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels);
                    }
                }
            }

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, MethodBase methodBase) {
            Contract.Requires(model != null);
            Contract.Requires(methodBase != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleAttributes(model, methodBase);

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
        }

        private CodeDocType ConvertToModel(Type type) {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);

            var cRef = GetCRefIdentifier(type);
            var model = type.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);

            var memberDataProvider = new CodeDocMemberInfoProvider<Type>(type);
            var xmlDocs = XmlDocs.GetMember(cRef.FullCRef);
            if (xmlDocs != null)
                memberDataProvider.Add(new CodeDocMemberXmlDataProvider(xmlDocs));

            ApplyContentXmlDocs(model, memberDataProvider);
            ApplyCommonAttributes(model, type);

            model.Namespace = GetCodeDocNamespaceByName(type.Namespace);
            model.Assembly = GetCodeDocAssembly(type.Assembly);
            model.IsEnum = type.IsEnum;
            model.IsFlagsEnum = type.IsEnum && type.HasAttribute(typeof(FlagsAttribute));
            model.IsSealed = type.IsSealed;
            model.IsValueType = type.IsValueType;
            model.IsStatic = type.IsStatic();
            model.IsObsolete = type.HasAttribute(typeof(ObsoleteAttribute));

            if (type.DeclaringType != null) {
                model.DeclaringType = GetSimpleEntity(type.DeclaringType);
            }

            if (!type.IsInterface) {
                var currentBase = type.BaseType;
                if (null != currentBase) {
                    var baseChain = new List<ICodeDocMember>() {
                        GetSimpleEntity(currentBase)
                    };
                    currentBase = currentBase.BaseType;
                    while (currentBase != null) {
                        baseChain.Add(GetSimpleEntity(currentBase));
                        currentBase = currentBase.BaseType;
                    }
                    model.BaseChain = new ReadOnlyCollection<ICodeDocMember>(baseChain.ToArray());
                }
            }

            var implementedInterfaces = type.GetInterfaces();
            if (implementedInterfaces.Length > 0) {
                model.Interfaces = Array.AsReadOnly(
                    Array.ConvertAll(implementedInterfaces, GetSimpleEntity));
            }

            if (type.IsGenericType) {
                var genericTypeDefinition = type.IsGenericTypeDefinition
                    ? type
                    : type.GetGenericTypeDefinition();
                if (genericTypeDefinition != null) {
                    var genericArguments = genericTypeDefinition.GetGenericArguments();
                    if (genericArguments.Length > 0) {
                        //var xmlDocs = XmlDocs.GetMember(GetCRefIdentifier(type).FullCRef);
                        Type[] parentGenericArguments = null;
                        if (type.IsNested) {
                            Contract.Assume(type.DeclaringType != null);
                            parentGenericArguments = type.DeclaringType.GetGenericArguments();
                        }

                        var genericModels = new List<CodeDocGenericParameter>();
                        foreach (var genericArgument in genericArguments) {
                            var argumentName = genericArgument.Name;
                            Contract.Assume(!String.IsNullOrEmpty(argumentName));
                            if (null != parentGenericArguments && parentGenericArguments.Any(p => p.Name == argumentName)) {
                                continue;
                            }

                            var typeConstraints = genericArgument.GetGenericParameterConstraints();
                            var genericModel = new CodeDocGenericParameter {
                                Name = argumentName,
                            };

                            if (xmlDocs != null)
                                genericModel.Summary = xmlDocs.GetTypeParameterSummary(argumentName);
                            if (typeConstraints.Length > 0)
                                genericModel.TypeConstraints = Array.AsReadOnly(Array.ConvertAll(typeConstraints, GetSimpleEntity));

                            genericModel.IsContravariant = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.Contravariant);
                            genericModel.IsCovariant = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.Covariant);
                            genericModel.HasDefaultConstructorConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.DefaultConstructorConstraint);
                            genericModel.HasNotNullableValueTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.NotNullableValueTypeConstraint);
                            genericModel.HasReferenceTypeConstraint = genericArgument.GenericParameterAttributes.HasFlag(
                                GenericParameterAttributes.ReferenceTypeConstraint);

                            genericModels.Add(genericModel);
                        }
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels);
                    }
                }
            }

            var nestedTypeModels = new List<ICodeDocMember>();
            var nestedDelegateModels = new List<ICodeDocMember>();
            foreach (var nestedType in type.GetAllNestedTypes().Where(MemberFilter)) {
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
            foreach (var methodInfo in type.GetAllMethods().Where(MemberFilter)) {
                var methodModel = ConvertToModel(methodInfo)
                    ?? ConvertToSimpleModel(methodInfo);
                if (methodInfo.IsOperatorOverload())
                    operatorModels.Add(methodModel);
                else
                    methodModels.Add(methodModel);
            }
            model.Methods = new ReadOnlyCollection<ICodeDocMember>(methodModels);
            model.Operators = new ReadOnlyCollection<ICodeDocMember>(operatorModels);

            model.Constructors = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllConstructors()
                .Where(MemberFilter)
                .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                .ToArray());

            model.Properties = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllProperties()
                .Where(MemberFilter)
                .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                .ToArray());

            model.Fields = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllFields()
                .Where(MemberFilter)
                .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                .ToArray());

            model.Events = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllEvents()
                .Where(MemberFilter)
                .Select(x => ConvertToModel(x) ?? ConvertToSimpleModel(x))
                .ToArray());

            if (model is CodeDocDelegate){
                var delegateResult = (CodeDocDelegate)model;

                delegateResult.IsPure = type.HasAttribute(t => t.Constructor.Name == "PureAttribute");
                delegateResult.IsPure = memberDataProvider.IsPure.GetValueOrDefault();
                var parameterModels = type
                    .GetDelegateTypeParameters()
                    .Select(parameter =>{
                        var parameterSummaryElement = model.XmlDocs != null
                            ? model.XmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetSimpleEntity(parameter.ParameterType),
                            parameterSummaryElement
                        );
                        ApplyArgumentEntityAttributes(paramModel, parameter, model.XmlDocs);
                        return paramModel;
                    })
                    .ToArray();
                delegateResult.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameterModels);

                var returnType = type.GetDelegateReturnType();
                if(returnType != null && returnType != typeof(void)) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    var paramModel = new CodeDocParameter(
                        String.Empty,
                        GetSimpleEntity(returnType),
                        returnSummaryElement
                    );
                    ApplyReturnEntityAttributes(paramModel, returnType, model.XmlDocs);
                    delegateResult.Return = paramModel;
                }

                if (model.XmlDocs != null){
                    if (model.XmlDocs.HasExceptionElements){
                        var exceptionLookup = new Dictionary<CRefIdentifier, CodeDocException>();
                        foreach (var xmlDocException in model.XmlDocs.ExceptionElements){
                            var exceptionCRef = String.IsNullOrWhiteSpace(xmlDocException.CRef)
                                ? new CRefIdentifier("T:")
                                : new CRefIdentifier(xmlDocException.CRef);
                            CodeDocException exceptionModel;
                            if (!exceptionLookup.TryGetValue(exceptionCRef, out exceptionModel)){
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
                            ){
                                if (priorElement.HasChildren){
                                    exceptionModel.Ensures.Add(priorElement);
                                }
                            }
                            else{
                                if (xmlDocException.HasChildren){
                                    exceptionModel.Conditions.Add(xmlDocException);
                                }
                            }
                        }

                        var exceptionModels = exceptionLookup.Values
                            .OrderBy(x => x.ExceptionType.CRef.FullCRef)
                            .ToArray();

                        foreach (var exceptionModel in exceptionModels){
                            // freeze collections
                            exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray());
                            exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray());
                        }

                        delegateResult.Exceptions = new ReadOnlyCollection<CodeDocException>(exceptionModels);
                    }

                    if (model.XmlDocs.HasEnsuresElements){
                        delegateResult.Ensures =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());
                    }
                    if (model.XmlDocs.HasRequiresElements){
                        delegateResult.Requires =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());
                    }
                }
            }

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleMember model, Type type){
            Contract.Requires(model != null);
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleAttributes(model, type);

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
        }

    }
}
