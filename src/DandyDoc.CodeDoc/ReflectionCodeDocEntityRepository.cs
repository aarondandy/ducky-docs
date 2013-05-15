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
    public class ReflectionCodeDocEntityRepository : CodeDocEntityRepositoryBase
    {

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

        public ReflectionCodeDocEntityRepository(ReflectionCRefLookup cRefLookup)
            : this(cRefLookup, null)
        {
            Contract.Requires(cRefLookup != null);
        }

        public ReflectionCodeDocEntityRepository(ReflectionCRefLookup cRefLookup, params XmlAssemblyDocumentation[] xmlDocs)
            : this(cRefLookup, (IEnumerable<XmlAssemblyDocumentation>)xmlDocs)
        {
            Contract.Requires(cRefLookup != null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReflectionCodeDocEntityRepository(ReflectionCRefLookup cRefLookup, IEnumerable<XmlAssemblyDocumentation> xmlDocs) {
            if(cRefLookup == null) throw new ArgumentNullException("cRefLookup");
            Contract.EndContractBlock();
            CRefLookup = cRefLookup;
            XmlDocs = new XmlAssemblyDocumentationCollection(xmlDocs);

            var assemblyModels = new List<CodeDocAssembly>();
            var namespaceModels = new Dictionary<string, CodeDocNamespace>();

            foreach (var assembly in CRefLookup.Assemblies){
                var assemblyShortName = assembly.GetName().Name;
                var assemblyModel = new CodeDocAssembly(GetCRefIdentifier(assembly)){
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

                    CodeDocNamespace namespaceModel;
                    if (!namespaceModels.TryGetValue(namespaceName, out namespaceModel)) {
                        var namespaceTitle = String.IsNullOrEmpty(namespaceName) ? "global" : namespaceName;
                        Contract.Assume(!String.IsNullOrEmpty("N:" + namespaceName));
                        namespaceModel = new CodeDocNamespace(new CRefIdentifier("N:" + namespaceName)){
                            Title = namespaceTitle,
                            ShortName = namespaceTitle,
                            FullName = namespaceTitle,
                            NamespaceName = namespaceTitle,
                            SubTitle = "Namespace",
                            Types = new List<ICodeDocEntity>(),
                            Assemblies = new List<ICodeDocAssembly>()
                        };
                        namespaceModels.Add(namespaceName, namespaceModel);
                    }

                    var simpleEntity = GetSimpleEntity(typeCRef);
                    namespaceModel.Types.Add(simpleEntity);
                    
                    if (assemblyNamespaceNames.Add(namespaceName)) {
                        // this is the first time this assembly has seen this namespace
                        namespaceModel.Assemblies.Add(assemblyModel);
                        assemblyModel.NamespaceCRefs.Add(namespaceModel.CRef);
                    }

                }

                assemblyModel.TypeCRefs = new ReadOnlyCollection<CRefIdentifier>(assemblyTypeCRefs);
                assemblyModels.Add(assemblyModel);
            }

            // freeze the namespace & assembly collections
            foreach (var namespaceModel in namespaceModels.Values) {
                namespaceModel.Assemblies = namespaceModel.Assemblies.AsReadOnly();
            }
            foreach (var assemblyModel in assemblyModels) {
                assemblyModel.NamespaceCRefs = assemblyModel.NamespaceCRefs.AsReadOnly();
            }

            Assemblies = new ReadOnlyCollection<ICodeDocAssembly>(assemblyModels.OrderBy(x => x.Title).ToArray());
            Namespaces = new ReadOnlyCollection<ICodeDocNamespace>(namespaceModels.Values.OrderBy(x => x.Title).ToArray());
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
            Contract.Invariant(XmlDocs != null);
        }

        public XmlAssemblyDocumentationCollection XmlDocs { get; private set; }

        public ReflectionCRefLookup CRefLookup { get; private set; }

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

        protected virtual bool MemberFilter(Type type) {
            return type != null
                && type.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        protected bool MemberFilter(MethodBase methodBase){
            return MemberFilter(methodBase, false);
        }

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

        protected virtual bool MemberFilter(EventInfo eventInfo) {
            if (eventInfo == null)
                return false;
            if (eventInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (eventInfo.IsSpecialName)
                return false;
            return true;
        }

        protected virtual bool MemberFilter(PropertyInfo propertyInfo) {
            if (propertyInfo == null)
                return false;
            if (propertyInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (propertyInfo.IsSpecialName)
                return false;
            return true;
        }

        private ICodeDocAssembly GetCodeDocAssembly(Assembly assembly) {
            Contract.Requires(assembly != null);
            return GetCodeDocAssembly(GetCRefIdentifier(assembly));
        }

        private ICodeDocEntity GetSimpleEntity(MemberInfo memberInfo) {
            Contract.Requires(memberInfo != null);

            // TODO: check a pool of other repositories if not found in this one

            if (MemberFilter(memberInfo))
                return ConvertToSimpleEntity(memberInfo);

            return CreateSimpleEntityPlaceholder(memberInfo);
        }

        private ICodeDocEntity CreateSimpleEntityPlaceholder(MemberInfo memberInfo){
            Contract.Requires(memberInfo != null);
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);
            var result = new CodeDocSimpleEntity(GetCRefIdentifier(memberInfo)) {
                ShortName = memberInfo.Name,
                Title = memberInfo.Name,
                SubTitle = String.Empty,
                FullName = memberInfo.Name,
                NamespaceName = String.Empty,
            };
            ApplySimpleEntityAttributes(result, memberInfo);
            return result;
        }

        public override ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocNamespace(cRef);
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;
            return ConvertToSimpleEntity(memberInfo);
        }

        public override ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocNamespace(cRef);
            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberFilter(memberInfo))
                return null;
            return ConvertToContentEntity(memberInfo);
        }

        private void ApplySimpleEntityAttributes(CodeDocSimpleEntity entity, MemberInfo memberInfo){
            Contract.Requires(entity != null);
            Contract.Requires(memberInfo != null);
            entity.ExternalVisibility = memberInfo.GetExternalVisibility();
            entity.IsStatic = memberInfo.IsStatic();
            entity.IsObsolete = memberInfo.HasAttribute(typeof(ObsoleteAttribute));
        }

        protected virtual ICodeDocEntity ConvertToSimpleEntity(MemberInfo memberInfo) {
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);
            var cRef = GetCRefIdentifier(memberInfo);
            var result = new CodeDocSimpleEntity(cRef);
            ApplyStandardXmlDocs(result, cRef.FullCRef);
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

            return result;
        }

        protected virtual ICodeDocEntityContent ConvertToContentEntity(MemberInfo memberInfo) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);

            if (memberInfo is Type)
                return ConvertToEntity((Type)memberInfo);
            if (memberInfo is FieldInfo)
                return ConvertToEntity((FieldInfo)memberInfo);
            if (memberInfo is MethodBase)
                return ConvertToEntity((MethodBase) memberInfo);
            if (memberInfo is EventInfo)
                return ConvertToEntity((EventInfo) memberInfo);
            if (memberInfo is PropertyInfo)
                return ConvertToEntity((PropertyInfo) memberInfo);
            throw new NotSupportedException();
        }

        private void ApplyStandardXmlDocs(CodeDocEntityContentBase model, string cRef){
            Contract.Requires(model != null);
            Contract.Requires(!String.IsNullOrEmpty(cRef));
            model.XmlDocs = XmlDocs.GetMember(cRef);
        }

        private void ApplyStandardXmlDocs(CodeDocSimpleEntity model, string cRef) {
            Contract.Requires(model != null);
            Contract.Requires(!String.IsNullOrEmpty(cRef));
            model.XmlDocs = XmlDocs.GetMember(cRef);
        }

        private CodeDocEvent ConvertToEntity(EventInfo eventInfo){
            Contract.Requires(eventInfo != null);
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            var cRef = GetCRefIdentifier(eventInfo);
            var model = new CodeDocEvent(cRef);

            ApplyStandardXmlDocs(model, cRef.FullCRef);
            ApplyCommonAttributes(model, eventInfo);

            model.DelegateType = GetSimpleEntity(eventInfo.EventHandlerType);
            Contract.Assume(eventInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(eventInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(eventInfo.DeclaringType);
            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleEntity model, EventInfo eventInfo) {
            Contract.Requires(model != null);
            Contract.Requires(eventInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, eventInfo);

            model.ShortName = StandardReflectionDisplayNameGenerator.RegularTypeDisplayNameOverlay.GetDisplayName(eventInfo);
            model.FullName = StandardReflectionDisplayNameGenerator.FullTypeDisplayNameOverlay.GetDisplayName(eventInfo);
            model.Title = model.ShortName;
            Contract.Assume(eventInfo.DeclaringType != null);
            model.NamespaceName = eventInfo.DeclaringType.Namespace;
            model.SubTitle = "Event";
        }

        private CodeDocField ConvertToEntity(FieldInfo fieldInfo) {
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var model = new CodeDocField(GetCRefIdentifier(fieldInfo));

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, fieldInfo);

            model.ValueType = GetSimpleEntity(fieldInfo.FieldType);
            model.IsLiteral = fieldInfo.IsLiteral;
            model.IsInitOnly = fieldInfo.IsInitOnly;

            Contract.Assume(fieldInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(fieldInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(fieldInfo.DeclaringType);
            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleEntity model, FieldInfo fieldInfo) {
            Contract.Requires(model != null);
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, fieldInfo);

            model.ShortName = StandardReflectionDisplayNameGenerator.RegularTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.FullName = StandardReflectionDisplayNameGenerator.FullTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
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

        private CodeDocProperty ConvertToEntity(PropertyInfo propertyInfo){
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var model = new CodeDocProperty(GetCRefIdentifier(propertyInfo));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, propertyInfo);

            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0){
                model.Parameters = new ReadOnlyCollection<ICodeDocParameter>(Array.ConvertAll(parameters,
                    parameter => {
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
                    }
                ));
            }

            var getterMethodInfo = propertyInfo.GetGetMethod(true);
            if (getterMethodInfo != null && MemberFilter(getterMethodInfo, true)) {
                var accessor = ConvertToEntity(getterMethodInfo);
                if (model.XmlDocs != null && model.XmlDocs.HasGetterElement) {
                    accessor.XmlDocs = model.XmlDocs.GetterElement;
                    ExpandCodeDocMethodXmlDocProperties(accessor);
                }
                model.Getter = accessor;
            }

            var setterMethodInfo = propertyInfo.GetSetMethod(true);
            if (setterMethodInfo != null && MemberFilter(setterMethodInfo, true)) {
                var accessor = ConvertToEntity(setterMethodInfo);
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

        private void ApplyCommonAttributes(CodeDocSimpleEntity model, PropertyInfo propertyInfo) {
            Contract.Requires(model != null);
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, propertyInfo);

            model.ShortName = StandardReflectionDisplayNameGenerator.RegularTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.FullName = StandardReflectionDisplayNameGenerator.FullTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
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
                            exceptionModel = new CodeDocException(GetSimpleEntity(exceptionCRef) ?? CreateSimpleEntityTypePlaceholder(exceptionCRef));
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

                    model.Exceptions = new ReadOnlyCollection<ICodeDocException>(exceptionModels);
                }

                if (model.XmlDocs.HasEnsuresElements) {
                    model.Ensures = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());
                }
                if (model.XmlDocs.HasRequiresElements) {
                    model.Requires = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());
                }
            }
        }

        private CodeDocMethod ConvertToEntity(MethodBase methodBase){
            Contract.Requires(methodBase != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var model = new CodeDocMethod(GetCRefIdentifier(methodBase));

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, methodBase);

            var methodInfo = methodBase as MethodInfo;

            Contract.Assume(methodBase.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodBase.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(methodBase.DeclaringType);
            model.IsOperatorOverload = methodBase.IsOperatorOverload();
            model.IsExtensionMethod = methodBase.IsExtensionMethod();
            model.IsSealed = methodBase.IsSealed();
            
            if (methodBase.DeclaringType != null && !methodBase.DeclaringType.IsInterface) {
                if (methodBase.IsAbstract)
                    model.IsAbstract = true;
                else if (methodBase.IsVirtual && !methodBase.IsFinal && methodBase.Attributes.HasFlag(MethodAttributes.NewSlot))
                    model.IsVirtual = true;
            }

            model.IsPure = methodBase.HasAttribute(t => t.Constructor.Name == "PureAttribute");

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
            model.Parameters = new ReadOnlyCollection<ICodeDocParameter>(parameterModels);

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
                        var xmlDocs = XmlDocs.GetMember(GetCRefIdentifier(methodBase).FullCRef);
                        var genericModels = new List<ICodeDocGenericParameter>();
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
                        model.GenericParameters = new ReadOnlyCollection<ICodeDocGenericParameter>(genericModels);
                    }
                }
            }

            return model;
        }

        private void ApplyCommonAttributes(CodeDocSimpleEntity model, MethodBase methodBase) {
            Contract.Requires(model != null);
            Contract.Requires(methodBase != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, methodBase);

            model.ShortName = StandardReflectionDisplayNameGenerator.RegularTypeDisplayNameOverlay.GetDisplayName(methodBase);
            model.FullName = StandardReflectionDisplayNameGenerator.FullTypeDisplayNameOverlay.GetDisplayName(methodBase);
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

        private CodeDocType ConvertToEntity(Type type) {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);

            var cRef = GetCRefIdentifier(type);
            var model = type.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonAttributes(model, type);

            model.Namespace = GetCodeDocNamespaceByName(type.Namespace);
            model.Assembly = GetCodeDocAssembly(type.Assembly);
            model.IsEnum = type.IsEnum;
            model.IsFlagsEnum = type.IsEnum && type.HasAttribute(typeof(FlagsAttribute));
            model.IsSealed = type.IsSealed;
            model.IsValueType = type.IsValueType;

            if (type.DeclaringType != null) {
                model.DeclaringType = GetSimpleEntity(type.DeclaringType);
            }

            if (!type.IsInterface) {
                var currentBase = type.BaseType;
                if (null != currentBase) {
                    var baseChain = new List<ICodeDocEntity>() {
                        GetSimpleEntity(currentBase)
                    };
                    currentBase = currentBase.BaseType;
                    while (currentBase != null) {
                        baseChain.Add(GetSimpleEntity(currentBase));
                        currentBase = currentBase.BaseType;
                    }
                    model.BaseChain = new ReadOnlyCollection<ICodeDocEntity>(baseChain.ToArray());
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
                        var xmlDocs = XmlDocs.GetMember(GetCRefIdentifier(type).FullCRef);
                        Type[] parentGenericArguments = null;
                        if (type.IsNested) {
                            Contract.Assume(type.DeclaringType != null);
                            parentGenericArguments = type.DeclaringType.GetGenericArguments();
                        }

                        var genericModels = new List<ICodeDocGenericParameter>();
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
                        model.GenericParameters = new ReadOnlyCollection<ICodeDocGenericParameter>(genericModels);
                    }
                }
            }

            var nestedTypeModels = new List<ICodeDocEntity>();
            var nestedDelegateModels = new List<ICodeDocEntity>();
            foreach (var nestedType in type.GetAllNestedTypes().Where(MemberFilter)) {
                var nestedTypeModel = ConvertToContentEntity(nestedType)
                    ?? ConvertToSimpleEntity(nestedType);
                if (nestedType.IsDelegateType())
                    nestedDelegateModels.Add(nestedTypeModel);
                else
                    nestedTypeModels.Add(nestedTypeModel);
            }
            model.NestedTypes = new ReadOnlyCollection<ICodeDocEntity>(nestedTypeModels);
            model.NestedDelegates = new List<ICodeDocEntity>(nestedDelegateModels);

            var methodModels = new List<ICodeDocEntity>();
            var operatorModels = new List<ICodeDocEntity>();
            foreach (var methodInfo in type.GetAllMethods().Where(MemberFilter)) {
                var methodModel = ConvertToContentEntity(methodInfo)
                    ?? ConvertToSimpleEntity(methodInfo);
                if (methodInfo.IsOperatorOverload())
                    operatorModels.Add(methodModel);
                else
                    methodModels.Add(methodModel);
            }
            model.Methods = new ReadOnlyCollection<ICodeDocEntity>(methodModels);
            model.Operators = new ReadOnlyCollection<ICodeDocEntity>(operatorModels);

            model.Constructors = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllConstructors()
                .Where(MemberFilter)
                .Select(x => ConvertToContentEntity(x) ?? ConvertToSimpleEntity(x))
                .ToArray());

            model.Properties = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllProperties()
                .Where(MemberFilter)
                .Select(x => ConvertToContentEntity(x) ?? ConvertToSimpleEntity(x))
                .ToArray());

            model.Fields = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllFields()
                .Where(MemberFilter)
                .Select(x => ConvertToContentEntity(x) ?? ConvertToSimpleEntity(x))
                .ToArray());

            model.Events = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllEvents()
                .Where(MemberFilter)
                .Select(x => ConvertToContentEntity(x) ?? ConvertToSimpleEntity(x))
                .ToArray());

            if (model is CodeDocDelegate){
                var delegateResult = (CodeDocDelegate)model;

                delegateResult.IsPure = type.HasAttribute(t => t.Constructor.Name == "PureAttribute");

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
                delegateResult.Parameters = new ReadOnlyCollection<ICodeDocParameter>(parameterModels);

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
                                exceptionModel = new CodeDocException(GetSimpleEntity(exceptionCRef) ?? CreateSimpleEntityTypePlaceholder(exceptionCRef));
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

                        delegateResult.Exceptions = new ReadOnlyCollection<ICodeDocException>(exceptionModels);
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

        private void ApplyCommonAttributes(CodeDocSimpleEntity model, Type type){
            Contract.Requires(model != null);
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplySimpleEntityAttributes(model, type);

            model.ShortName = StandardReflectionDisplayNameGenerator.RegularTypeDisplayNameOverlay.GetDisplayName(type);
            model.FullName = StandardReflectionDisplayNameGenerator.FullTypeDisplayNameOverlay.GetDisplayName(type);
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
