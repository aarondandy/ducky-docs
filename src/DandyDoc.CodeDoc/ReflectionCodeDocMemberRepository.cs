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

        public override ICodeDocMember GetMemberModel(CRefIdentifier cRef) {
            return GetMemberModel(cRef, light: false);
        }

        private ICodeDocMember GetMemberModel(CRefIdentifier cRef, bool light) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return ToFullNamespace(GetCodeDocSimpleNamespace(cRef));
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocSimpleAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;

            if (memberInfo is Type) {
                return light ? ConvertToModelLite((Type)memberInfo) : ConvertToModel((Type)memberInfo);
            }
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

        private ICodeDocMember GetOrConvert(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it

            // TODO: need to look elsewhere for a model.
            var localModel = GetMemberModel(cRef);
            if (localModel != null)
                return localModel;

            return CreateGeneralMemberPlaceholder(cRef);
        }

        private ICodeDocMember GetOrConvertLite(CRefIdentifier cRef) {
            Contract.Requires(cRef != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it

            // TODO: need to look elsewhere for a model.
            var localModel = GetMemberModel(cRef, light: true);
            if (localModel != null)
                return localModel;

            return CreateGeneralMemberPlaceholder(cRef);
        }

        private CodeDocType GetOrConvert(Type type){
            Contract.Requires(type != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it
            return ConvertToModel(type);
        }

        private CodeDocType GetOrConvertLite(Type type) {
            Contract.Requires(type != null);
            // TODO:
            // 1) Use the repository tree to get the model by cRef (so we can use a cache)
            // 1a) Make sure to include this repostitory in the search, but last (should be behind a cache)
            // 2) Try to make a model for it
            return ConvertToModelLite(type);
        }

        [Obsolete("Use GetCodeDocAssembly")]
        private CodeDocSimpleAssembly GetOrConvert(Assembly assembly){
            Contract.Requires(assembly != null);
            Contract.Assume(CRefLookup.Assemblies.Contains(assembly));
            // NOTE: if we are caling this method it should be for an assembly handled by this repository instance
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

            model.DelegateType = GetOrConvertLite(eventInfo.EventHandlerType);
            Contract.Assume(eventInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(eventInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvertLite(eventInfo.DeclaringType);
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

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToList();
            model.ValueType = GetOrConvertLite(fieldInfo.FieldType);
            model.IsLiteral = fieldInfo.IsLiteral;
            model.IsInitOnly = fieldInfo.IsInitOnly;
            model.IsStatic = fieldInfo.IsStatic;
            model.IsObsolete = fieldInfo.HasAttribute(typeof(ObsoleteAttribute));

            Contract.Assume(fieldInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(fieldInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvertLite(fieldInfo.DeclaringType);
            return model;
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

            model.ValueDescriptionContents = memberDataProvider.GeValueDescriptionContents().ToList();
            model.ValueType = GetOrConvertLite(propertyInfo.PropertyType);
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(propertyInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(propertyInfo.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvertLite(propertyInfo.DeclaringType);

            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0){
                model.Parameters = new ReadOnlyCollection<CodeDocParameter>(Array.ConvertAll(parameters,
                    parameter => {
                        var parameterSummaryElement = xmlDocs != null
                            ? xmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetOrConvertLite(parameter.ParameterType),
                            parameterSummaryElement
                        );
                        ApplyArgumentEntityAttributes(paramModel, parameter, xmlDocs);
                        return paramModel;
                    }
                ));  // TODO: Freeze
            }

            var getterMethodInfo = propertyInfo.GetGetMethod(true);
            if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                var accessor = ConvertToModel(getterMethodInfo);
                // TODO: feed the getter XML docs into the convert method
                if (model.XmlDocs != null && model.XmlDocs.HasGetterElement) {
                    accessor.XmlDocs = model.XmlDocs.GetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Getter = accessor;
            }

            var setterMethodInfo = propertyInfo.GetSetMethod(true);
            if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                var accessor = ConvertToModel(setterMethodInfo);
                // TODO: feed the setter XML docs into the convert method
                if (model.XmlDocs != null && model.XmlDocs.HasSetterElement) {
                    accessor.XmlDocs = model.XmlDocs.SetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Setter = accessor;
            }

            return model;
        }

        [Obsolete]
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
                            exceptionModel = new CodeDocException(GetOrConvertLite(exceptionCRef));
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
                        exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray()); // TODO: Freeze
                        exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray()); // TODO: Freeze
                    }

                    model.Exceptions = new ReadOnlyCollection<CodeDocException>(exceptionModels); // TODO: Freeze
                }

                if (model.XmlDocs.HasEnsuresElements) {
                    model.Ensures = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray()); // TODO: Freeze
                }
                if (model.XmlDocs.HasRequiresElements) {
                    model.Requires = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray()); // TODO: Freeze
                }
            }
        }

        private CodeDocMethod ConvertToModel(MethodBase methodBase){
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

            var methodInfo = methodBase as MethodInfo;

            Contract.Assume(methodBase.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodBase.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
            model.DeclaringType = GetOrConvertLite(methodBase.DeclaringType);
            model.IsOperatorOverload = methodBase.IsOperatorOverload();
            model.IsExtensionMethod = methodBase.IsExtensionMethod();
            model.IsSealed = methodBase.IsSealed();
            model.IsStatic = memberDataProvider.IsStatic ?? methodBase.IsStatic;
            model.IsObsolete = memberDataProvider.IsObsolete.GetValueOrDefault();
            model.IsPure = memberDataProvider.IsPure.GetValueOrDefault();
            
            if (methodBase.DeclaringType != null && !methodBase.DeclaringType.IsInterface) {
                if (methodBase.IsAbstract)
                    model.IsAbstract = true;
                else if (methodBase.IsVirtual && !methodBase.IsFinal && methodBase.Attributes.HasFlag(MethodAttributes.NewSlot))
                    model.IsVirtual = true;
            }

            var parameterModels = methodBase
                .GetParameters()
                .Select(parameter => {
                    var parameterSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.GetParameterSummary(parameter.Name)
                        : null;
                    var paramModel = new CodeDocParameter(
                        parameter.Name,
                        GetOrConvertLite(parameter.ParameterType),
                        parameterSummaryElement
                    );
                    ApplyArgumentEntityAttributes(paramModel, parameter, model.XmlDocs);
                    return paramModel;
                })
                .ToArray();
            model.Parameters = new ReadOnlyCollection<CodeDocParameter>(parameterModels); // TODO: freeze

            if (methodInfo != null) {
                if (methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void)) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    var paramModel = new CodeDocParameter(
                        String.Empty,
                        GetOrConvertLite(methodInfo.ReturnType),
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
                                genericModel.TypeConstraints = Array.AsReadOnly(Array.ConvertAll(typeConstraints, GetOrConvertLite));

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
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels); // TODO: freeze instead
                    }
                }
            }

            return model;
        }

        private CodeDocType ConvertToModelLite(Type type) {
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

            if (type.DeclaringType != null) {
                model.DeclaringType = GetOrConvertLite(type.DeclaringType);
            }

            return model;
        }

        private CodeDocType ConvertToModel(Type type) {
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

            if (type.DeclaringType != null) {
                model.DeclaringType = GetOrConvertLite(type.DeclaringType);
            }

            if (!type.IsInterface) {
                var currentBase = type.BaseType;
                if (null != currentBase) {
                    var baseChain = new List<CodeDocType>() {
                        GetOrConvertLite(currentBase)
                    };
                    currentBase = currentBase.BaseType;
                    while (currentBase != null) {
                        baseChain.Add(GetOrConvertLite(currentBase));
                        currentBase = currentBase.BaseType;
                    }
                    model.BaseChain = new ReadOnlyCollection<CodeDocType>(baseChain.ToArray()); // TODO: freeze instead
                }
            }

            var implementedInterfaces = type.GetInterfaces();
            if (implementedInterfaces.Length > 0) {
                model.Interfaces = Array.AsReadOnly(
                    Array.ConvertAll(implementedInterfaces, GetOrConvertLite));
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
                                genericModel.TypeConstraints = Array.AsReadOnly(Array.ConvertAll(typeConstraints, GetOrConvertLite));

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
                        model.GenericParameters = new ReadOnlyCollection<CodeDocGenericParameter>(genericModels); // TODO: Freeze
                    }
                }
            }

            var nestedTypeModels = new List<ICodeDocMember>();
            var nestedDelegateModels = new List<ICodeDocMember>();
            foreach (var nestedType in type.GetAllNestedTypes().Where(MemberFilter)) {
                var nestedTypeModel = ConvertToModelLite(nestedType);
                if (nestedType.IsDelegateType())
                    nestedDelegateModels.Add(nestedTypeModel);
                else
                    nestedTypeModels.Add(nestedTypeModel);
            }
            model.NestedTypes = new ReadOnlyCollection<ICodeDocMember>(nestedTypeModels); // TODO: freeze
            model.NestedDelegates = new ReadOnlyCollection<ICodeDocMember>(nestedDelegateModels); // TODO: freeze

            var methodModels = new List<ICodeDocMember>();
            var operatorModels = new List<ICodeDocMember>();
            foreach (var methodInfo in type.GetAllMethods().Where(MemberFilter)) {
                var methodModel = ConvertToModel(methodInfo);
                if (methodInfo.IsOperatorOverload())
                    operatorModels.Add(methodModel);
                else
                    methodModels.Add(methodModel);
            }
            model.Methods = new ReadOnlyCollection<ICodeDocMember>(methodModels); // TODO: freeze
            model.Operators = new ReadOnlyCollection<ICodeDocMember>(operatorModels); // TODO: freeze

            model.Constructors = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllConstructors()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray()); // TODO: freeze

            model.Properties = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllProperties()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray()); // TODO: freeze

            model.Fields = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllFields()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray()); // TODO: freeze

            model.Events = new ReadOnlyCollection<ICodeDocMember>(type
                .GetAllEvents()
                .Where(MemberFilter)
                .Select(ConvertToModel)
                .ToArray()); // TODO: freeze

            if (model is CodeDocDelegate){
                var delegateResult = (CodeDocDelegate)model;

                //delegateResult.IsPure = type.HasAttribute(t => t.Constructor.Name == "PureAttribute");
                delegateResult.IsPure = memberDataProvider.IsPure.GetValueOrDefault();
                var parameterModels = type
                    .GetDelegateTypeParameters()
                    .Select(parameter =>{
                        var parameterSummaryElement = model.XmlDocs != null
                            ? model.XmlDocs.GetParameterSummary(parameter.Name)
                            : null;
                        var paramModel = new CodeDocParameter(
                            parameter.Name,
                            GetOrConvertLite(parameter.ParameterType),
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
                        GetOrConvertLite(returnType),
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
                                exceptionModel = new CodeDocException(GetOrConvertLite(exceptionCRef));
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
                            exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray()); // TODO: freeze
                            exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray());  // TODO: freeze
                        }

                        delegateResult.Exceptions = new ReadOnlyCollection<CodeDocException>(exceptionModels);  // TODO: freeze
                    }

                    if (model.XmlDocs.HasEnsuresElements){
                        delegateResult.Ensures =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());  // TODO: freeze
                    }
                    if (model.XmlDocs.HasRequiresElements){
                        delegateResult.Requires =
                            new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());  // TODO: freeze
                    }
                }
            }

            return model;
        }

    }
}
