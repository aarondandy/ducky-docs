using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.CRef;
using DandyDoc.DisplayName;
using DandyDoc.ExternalVisibility;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    public class ReflectionCodeDocEntityRepository : ICodeDocEntityRepository
    {

        protected static readonly StandardReflectionDisplayNameGenerator RegularTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
            ShowTypeNameForMembers = false
        };

        protected static readonly StandardReflectionDisplayNameGenerator NestedTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
            ShowTypeNameForMembers = true
        };

        protected static readonly StandardReflectionDisplayNameGenerator FullTypeDisplayNameOverlay = new StandardReflectionDisplayNameGenerator {
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
                    AssemblyFileName = assembly.GetFileName(),
                    Title = assemblyShortName,
                    ShortName = assemblyShortName,
                    FullName = assembly.FullName,
                    NamespaceName = assemblyShortName,
                    SubTitle = "Assembly",
                    Namespaces = new List<ICodeDocNamespace>()
                };

                var assemblyTypeCRefs = new List<CRefIdentifier>();
                var assemblyNamespaceNames = new HashSet<string>();
                foreach (var type in assembly
                    .GetTypes()
                    .Where(t => !t.IsNested)
                    .Where(TypeFilter)
                ){
                    var typeCRef = GetCRefIdentifier(type);
                    assemblyTypeCRefs.Add(typeCRef);
                    var namespaceName = type.Namespace;
                    if (String.IsNullOrWhiteSpace(namespaceName))
                        namespaceName = String.Empty;

                    CodeDocNamespace namespaceModel;
                    if (!namespaceModels.TryGetValue(namespaceName, out namespaceModel)) {
                        var namespaceTitle = String.IsNullOrWhiteSpace(namespaceName) ? "global" : namespaceName;
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
                        assemblyModel.Namespaces.Add(namespaceModel);
                    }

                }

                assemblyModel.TypeCRefs = new ReadOnlyCollection<CRefIdentifier>(assemblyTypeCRefs);
                assemblyModels.Add(assemblyModel);
            }

            // freeze the namespace & assembly collections
            foreach (var namespaceModel in namespaceModels.Values) {
                namespaceModel.Assemblies = new ReadOnlyCollection<ICodeDocAssembly>(namespaceModel.Assemblies);
            }
            foreach (var assemblyModel in assemblyModels) {
                assemblyModel.Namespaces = new ReadOnlyCollection<ICodeDocNamespace>(assemblyModel.Namespaces);
            }

            Assemblies = new ReadOnlyCollection<ICodeDocAssembly>(assemblyModels.OrderBy(x => x.Title).ToArray());
            Namespaces = new ReadOnlyCollection<ICodeDocNamespace>(namespaceModels.Values.OrderBy(x => x.Title).ToArray());
        }

        public IList<ICodeDocAssembly> Assemblies { get; private set; }

        public IList<ICodeDocNamespace> Namespaces { get; private set; }

        public XmlAssemblyDocumentationCollection XmlDocs { get; private set; }

        public ReflectionCRefLookup CRefLookup { get; private set; }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
            Contract.Invariant(XmlDocs != null);
        }

        protected bool MemberInfoFilter(MemberInfo memberInfo) {
            if (memberInfo == null)
                return false;
            if (memberInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (memberInfo is MethodBase)
                return MethodBaseFilter((MethodBase)memberInfo);
            if (memberInfo is FieldInfo)
                return FieldInfoFilter((FieldInfo)memberInfo);
            if (memberInfo is EventInfo)
                return EventInfoFilter((EventInfo)memberInfo);
            if (memberInfo is PropertyInfo)
                return PropertyInfoFilter((PropertyInfo)memberInfo);
            return memberInfo.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        protected virtual bool TypeFilter(Type type) {
            return type != null
                && type.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        protected bool MethodBaseFilter(MethodBase methodBase){
            return MethodBaseFilter(methodBase, false);
        }

        protected virtual bool MethodBaseFilter(MethodBase methodBase, bool isPropertyMethod) {
            if (methodBase == null)
                return false;
            if (methodBase.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = methodBase.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;
            if (methodBase.IsSpecialName && !isPropertyMethod && !methodBase.IsConstructor && !methodBase.IsOperatorOverload())
                return false;
            if (methodBase.IsFinalizer())
                return false;
            return true;
        }

        protected virtual bool FieldInfoFilter(FieldInfo fieldInfo) {
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

        protected virtual bool EventInfoFilter(EventInfo eventInfo) {
            if (eventInfo == null)
                return false;
            if (eventInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (eventInfo.IsSpecialName)
                return false;
            return true;
        }

        protected virtual bool PropertyInfoFilter(PropertyInfo propertyInfo) {
            if (propertyInfo == null)
                return false;
            if (propertyInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (propertyInfo.IsSpecialName)
                return false;
            return true;
        }

        private ICodeDocNamespace GetCodeDocNamespace(CRefIdentifier cRef){
            Contract.Requires(cRef != null);
            return Namespaces.FirstOrDefault(x => cRef.Equals(x.CRef));
        }

        private ICodeDocAssembly GetCodeDocAssembly(CRefIdentifier cRef){
            Contract.Requires(cRef != null);
            return Assemblies.FirstOrDefault(x => cRef.Equals(x.CRef))
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.AssemblyFileName)
                ?? Assemblies.FirstOrDefault(x => cRef.CoreName == x.ShortName);
        }

        private ICodeDocAssembly GetCodeDocAssembly(Assembly assembly) {
            Contract.Requires(assembly != null);
            return GetCodeDocAssembly(GetCRefIdentifier(assembly));
        }

        private ICodeDocEntity GetSimpleEntity(MemberInfo memberInfo) {
            Contract.Requires(memberInfo != null);

            // TODO: check a pool of other repositories if not found in this one

            if (MemberInfoFilter(memberInfo))
                return ConvertToSimpleEntity(memberInfo);

            return new CodeDocSimpleEntity(GetCRefIdentifier(memberInfo)) {
                ShortName = memberInfo.Name,
                Title = memberInfo.Name,
                SubTitle = String.Empty,
                FullName = memberInfo.Name,
                NamespaceName = String.Empty,
            };
        }

        private ICodeDocEntity GetCodeDocNamespaceByName(string namespaceName) {
            return Namespaces.FirstOrDefault(x => x.FullName == namespaceName)
                ?? CreateNamespaceFromName(namespaceName);
        }

        private ICodeDocEntity CreateNamespaceFromName(string namespaceName) {
            string namespaceFriendlyName;
            if (String.IsNullOrWhiteSpace(namespaceName)) {
                namespaceName = String.Empty;
                namespaceFriendlyName = "global";
            }
            else {
                namespaceFriendlyName = namespaceName;
            }
            return new CodeDocSimpleEntity(new CRefIdentifier("N:" + namespaceName)) {
                FullName = namespaceFriendlyName,
                ShortName = namespaceFriendlyName,
                SubTitle = "Namespace",
                Title = namespaceFriendlyName
            };
        }

        public ICodeDocEntity GetSimpleEntity(string cRef){
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetSimpleEntity(new CRefIdentifier(cRef));
        }

        public ICodeDocEntityContent GetContentEntity(string cRef) {
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            return GetContentEntity(new CRefIdentifier(cRef));
        }

        public ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocNamespace(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToSimpleEntity(memberInfo);
        }

        public ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();

            if ("N".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocNamespace(cRef);

            if ("A".Equals(cRef.TargetType, StringComparison.OrdinalIgnoreCase))
                return GetCodeDocAssembly(cRef);

            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToContentEntity(memberInfo);
        }

        protected virtual ICodeDocEntity ConvertToSimpleEntity(MemberInfo memberInfo) {
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);
            var cRef = GetCRefIdentifier(memberInfo);
            var result = new CodeDocSimpleEntity(cRef);

            ApplyStandardXmlDocs(result, cRef.FullCRef);
            if (memberInfo is Type) {
                ApplyCommonTypeAttributes(result, (Type)memberInfo);
            }
            else if (memberInfo is MethodBase){
                ApplyCommonMethodAttributes(result, (MethodBase) memberInfo);
            }
            else if (memberInfo is FieldInfo) {
                ApplyCommonFieldAttributes(result, (FieldInfo)memberInfo);
            }
            else if (memberInfo is EventInfo) {
                ApplyCommonEventAttributes(result, (EventInfo)memberInfo);
            }
            else if (memberInfo is PropertyInfo) {
                ApplyCommonPropertyAttributes(result, (PropertyInfo)memberInfo);
            }
            else{
                throw new NotSupportedException();
            }

            return result;
        }

        protected virtual ICodeDocEntityContent ConvertToContentEntity(MemberInfo memberInfo) {
            if(memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(Contract.Result<ICodeDocEntity>() != null);

            if (memberInfo is Type){
                return ConvertToTypeEntity((Type)memberInfo);
            }
            if (memberInfo is FieldInfo){
                return ConvertToFieldEntity((FieldInfo)memberInfo);
            }
            if (memberInfo is MethodBase){
                return ConvertToMethodEntity((MethodBase) memberInfo);
            }
            if (memberInfo is EventInfo){
                return ConvertToEventEntity((EventInfo) memberInfo);
            }
            if (memberInfo is PropertyInfo){
                return ConvertToPropertyEntity((PropertyInfo) memberInfo);
            }

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

        private CodeDocEvent ConvertToEventEntity(EventInfo eventInfo){
            Contract.Requires(eventInfo != null);
            Contract.Ensures(Contract.Result<CodeDocEvent>() != null);
            var cRef = GetCRefIdentifier(eventInfo);
            var model = new CodeDocEvent(cRef);
            ApplyStandardXmlDocs(model, cRef.FullCRef);
            ApplyCommonEventAttributes(model, eventInfo);
            model.DelegateType = GetSimpleEntity(eventInfo.EventHandlerType);
            Contract.Assume(eventInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(eventInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(eventInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(eventInfo.DeclaringType);
            return model;
        }

        private void ApplyCommonEventAttributes(CodeDocSimpleEntity model, EventInfo eventInfo) {
            Contract.Requires(model != null);
            Contract.Requires(eventInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(eventInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(eventInfo);
            model.Title = model.ShortName;
            Contract.Assume(eventInfo.DeclaringType != null);
            model.NamespaceName = eventInfo.DeclaringType.Namespace;
            model.SubTitle = "Event";
            model.IsStatic = eventInfo.IsStatic();
        }

        private CodeDocField ConvertToFieldEntity(FieldInfo fieldInfo) {
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(Contract.Result<CodeDocField>() != null);
            var model = new CodeDocField(GetCRefIdentifier(fieldInfo));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonFieldAttributes(model, fieldInfo);
            model.ValueType = GetSimpleEntity(fieldInfo.FieldType);
            model.IsLiteral = fieldInfo.IsLiteral;
            model.IsInitOnly = fieldInfo.IsInitOnly;

            Contract.Assume(fieldInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(fieldInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(fieldInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(fieldInfo.DeclaringType);
            return model;
        }

        private void ApplyCommonFieldAttributes(CodeDocSimpleEntity model, FieldInfo fieldInfo) {
            Contract.Requires(model != null);
            Contract.Requires(fieldInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(fieldInfo);
            model.Title = model.ShortName;
            Contract.Assume(fieldInfo.DeclaringType != null);
            model.NamespaceName = fieldInfo.DeclaringType.Namespace;
            model.IsStatic = fieldInfo.IsStatic;

            if (fieldInfo.IsLiteral)
                model.SubTitle = "Constant";
            else
                model.SubTitle = "Field";
        }

        private CodeDocProperty ConvertToPropertyEntity(PropertyInfo propertyInfo){
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(Contract.Result<CodeDocProperty>() != null);
            var model = new CodeDocProperty(GetCRefIdentifier(propertyInfo));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonPropertyAttributes(model, propertyInfo);

            var parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0){
                model.Parameters = new ReadOnlyCollection<ICodeDocParameter>(Array.ConvertAll(parameters,
                    parameter => {
                        var parameterSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.GetParameterSummary(parameter.Name)
                        : null;
                        return new CodeDocParameter(
                            parameter.Name,
                            GetSimpleEntity(parameter.ParameterType),
                            parameterSummaryElement
                        ) {
                            IsOut = parameter.IsOut,
                            IsByRef = parameter.ParameterType.IsByRef
                        };
                    }));
            }

            var getterMethodInfo = propertyInfo.GetGetMethod(true);
            if (getterMethodInfo != null && MethodBaseFilter(getterMethodInfo, true)) {
                model.Getter = ConvertToMethodEntity(getterMethodInfo);
            }

            var setterMethodInfo = propertyInfo.GetSetMethod(true);
            if (setterMethodInfo != null && MethodBaseFilter(setterMethodInfo, true)) {
                model.Setter = ConvertToMethodEntity(setterMethodInfo);
            }

            model.ValueType = GetSimpleEntity(propertyInfo.PropertyType);
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(propertyInfo.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(propertyInfo.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(propertyInfo.DeclaringType);

            return model;
        }

        private void ApplyCommonPropertyAttributes(CodeDocSimpleEntity model, PropertyInfo propertyInfo) {
            Contract.Requires(model != null);
            Contract.Requires(propertyInfo != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(propertyInfo);
            model.Title = model.ShortName;
            Contract.Assume(propertyInfo.DeclaringType != null);
            model.NamespaceName = propertyInfo.DeclaringType.Namespace;
            model.IsStatic = propertyInfo.IsStatic();

            if (propertyInfo.IsItemIndexerProperty())
                model.SubTitle = "Indexer";
            else
                model.SubTitle = "Property";

        }

        private CodeDocMethod ConvertToMethodEntity(MethodBase methodBase){
            Contract.Requires(methodBase != null);
            Contract.Ensures(Contract.Result<CodeDocMethod>() != null);
            var model = new CodeDocMethod(GetCRefIdentifier(methodBase));
            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonMethodAttributes(model, methodBase);
            var methodInfo = methodBase as MethodInfo;

            Contract.Assume(methodBase.DeclaringType != null);
            model.Namespace = GetCodeDocNamespaceByName(methodBase.DeclaringType.Namespace);
            model.Assembly = GetCodeDocAssembly(methodBase.DeclaringType.Assembly);
            model.DeclaringType = GetSimpleEntity(methodBase.DeclaringType);

            var parameterModels = methodBase
                .GetParameters()
                .Select(parameter => {
                    var parameterSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.GetParameterSummary(parameter.Name)
                        : null;
                    return new CodeDocParameter(
                        parameter.Name,
                        GetSimpleEntity(parameter.ParameterType),
                        parameterSummaryElement
                    ){
                        IsOut = parameter.IsOut,
                        IsByRef = parameter.ParameterType.IsByRef
                    };
                })
                .ToArray();
            model.Parameters = new ReadOnlyCollection<ICodeDocParameter>(parameterModels);

            if (methodInfo != null) {
                if (methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void)) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    model.Return = new CodeDocParameter(
                        String.Empty,
                        GetSimpleEntity(methodInfo.ReturnType),
                        returnSummaryElement);
                }
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
                            exceptionModel = new CodeDocException(exceptionCRef);
                            exceptionModel.Ensures = new List<XmlDocNode>();
                            exceptionModel.Conditions = new List<XmlDocNode>();
                            exceptionLookup.Add(exceptionCRef, exceptionModel);
                        }
                        var priorElement = xmlDocException.PriorElement;
                        if (
                            null != priorElement
                            && String.Equals("ensuresOnThrow",priorElement.Name, StringComparison.OrdinalIgnoreCase)
                            && priorElement.Element.GetAttribute("exception") == exceptionCRef.FullCRef
                        ) {
                            if (priorElement.HasChildren){
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
                        .OrderBy(x => x.ExceptionCRef.FullCRef)
                        .ToArray();

                    foreach (var exceptionModel in exceptionModels) {
                        // freeze collections
                        exceptionModel.Ensures = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Ensures.ToArray());
                        exceptionModel.Conditions = new ReadOnlyCollection<XmlDocNode>(exceptionModel.Conditions.ToArray());
                    }

                    model.Exceptions = new ReadOnlyCollection<ICodeDocException>(exceptionModels);
                }

                if (model.XmlDocs.HasEnsuresElements){
                    model.Ensures = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.EnsuresElements.ToArray());
                }
                if (model.XmlDocs.HasRequiresElements){
                    model.Requires = new ReadOnlyCollection<XmlDocContractElement>(model.XmlDocs.RequiresElements.ToArray());
                }
            }

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

        private void ApplyCommonMethodAttributes(CodeDocSimpleEntity model, MethodBase methodBase) {
            Contract.Requires(model != null);
            Contract.Requires(methodBase != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(methodBase);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(methodBase);
            model.Title = model.ShortName;
            Contract.Assume(methodBase.DeclaringType != null);
            model.NamespaceName = methodBase.DeclaringType.Namespace;
            model.IsStatic = methodBase.IsStatic;

            if (methodBase.IsConstructor)
                model.SubTitle = "Constructor";
            else if (methodBase.IsOperatorOverload())
                model.SubTitle = "Operator";
            else
                model.SubTitle = "Method";
        }

        private CodeDocType ConvertToTypeEntity(Type type) {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);

            var cRef = GetCRefIdentifier(type);
            var model = type.IsDelegateType()
                ? new CodeDocDelegate(cRef)
                : new CodeDocType(cRef);

            ApplyStandardXmlDocs(model, model.CRef.FullCRef);
            ApplyCommonTypeAttributes(model, type);

            model.Namespace = GetCodeDocNamespaceByName(type.Namespace);
            model.Assembly = GetCodeDocAssembly(type.Assembly);
            model.IsEnum = type.IsEnum;
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
                model.DirectInterfaces = Array.AsReadOnly(
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
            foreach (var nestedType in type.GetAllNestedTypes().Where(TypeFilter)) {
                var nestedTypeModel = ConvertToSimpleEntity(nestedType);
                if (nestedType.IsDelegateType())
                    nestedDelegateModels.Add(nestedTypeModel);
                else
                    nestedTypeModels.Add(nestedTypeModel);
            }
            model.NestedTypes = new ReadOnlyCollection<ICodeDocEntity>(nestedTypeModels);
            model.NestedDelegates = new List<ICodeDocEntity>(nestedDelegateModels);

            var methodModels = new List<ICodeDocEntity>();
            var operatorModels = new List<ICodeDocEntity>();
            foreach (var methodInfo in type.GetAllMethods().Where(MethodBaseFilter)) {
                var methodModel = ConvertToSimpleEntity(methodInfo);
                if (methodInfo.IsOperatorOverload())
                    operatorModels.Add(methodModel);
                else
                    methodModels.Add(methodModel);
            }
            model.Methods = new ReadOnlyCollection<ICodeDocEntity>(methodModels);
            model.Operators = new ReadOnlyCollection<ICodeDocEntity>(operatorModels);

            model.Constructors = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllConstructors()
                .Where(MethodBaseFilter)
                .Select(ConvertToSimpleEntity)
                .ToArray());

            model.Properties = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllProperties()
                .Where(PropertyInfoFilter)
                .Select(ConvertToSimpleEntity)
                .ToArray());

            model.Fields = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllFields()
                .Where(FieldInfoFilter)
                .Select(ConvertToSimpleEntity)
                .ToArray());

            model.Events = new ReadOnlyCollection<ICodeDocEntity>(type
                .GetAllEvents()
                .Where(EventInfoFilter)
                .Select(ConvertToSimpleEntity)
                .ToArray());

            if (model is CodeDocDelegate){
                var delegateResult = (CodeDocDelegate)model;

                var parameterModels = type
                    .GetDelegateTypeParameters()
                    .Select(parameter =>{
                                var parameterSummaryElement = model.XmlDocs != null
                                    ? model.XmlDocs.GetParameterSummary(parameter.Name)
                                    : null;
                                return new CodeDocParameter(
                                    parameter.Name,
                                    GetSimpleEntity(parameter.ParameterType),
                                    parameterSummaryElement
                                    ){
                                        IsOut = parameter.IsOut,
                                        IsByRef = parameter.ParameterType.IsByRef
                                    };
                            })
                    .ToArray();
                delegateResult.Parameters = new ReadOnlyCollection<ICodeDocParameter>(parameterModels);

                var returnType = type.GetDelegateReturnType();
                if(returnType != null && returnType != typeof(void)) {
                    var returnSummaryElement = model.XmlDocs != null
                        ? model.XmlDocs.ReturnsElement
                        : null;
                    delegateResult.Return = new CodeDocParameter(
                        String.Empty,
                        GetSimpleEntity(returnType),
                        returnSummaryElement);
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
                                exceptionModel = new CodeDocException(exceptionCRef);
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
                            .OrderBy(x => x.ExceptionCRef.FullCRef)
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

        private void ApplyCommonTypeAttributes(CodeDocSimpleEntity model, Type type){
            Contract.Requires(model != null);
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            model.ShortName = RegularTypeDisplayNameOverlay.GetDisplayName(type);
            model.FullName = FullTypeDisplayNameOverlay.GetDisplayName(type);
            model.Title = model.ShortName;
            model.NamespaceName = type.Namespace;
            model.IsStatic = type.IsStatic();

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
