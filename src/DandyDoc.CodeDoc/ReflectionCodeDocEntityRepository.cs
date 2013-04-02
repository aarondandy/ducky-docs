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
        }

        public XmlAssemblyDocumentationCollection XmlDocs { get; private set; }

        public ReflectionCRefLookup CRefLookup { get; private set; }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(CRefLookup != null);
            Contract.Invariant(XmlDocs != null);
        }

        public ICodeDocEntity GetSimpleEntity(string cRef){
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToSimpleEntity(memberInfo);
        }

        public ICodeDocEntityContent GetContentEntity(string cRef) {
            if(String.IsNullOrEmpty(cRef)) throw new ArgumentException("CRef is not valid.", "cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToContentEntity(memberInfo);
        }

        public ICodeDocEntity GetSimpleEntity(CRefIdentifier cRef) {
            if (cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToSimpleEntity(memberInfo);
        }

        public ICodeDocEntityContent GetContentEntity(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.EndContractBlock();
            var memberInfo = CRefLookup.GetMember(cRef);
            if (memberInfo == null || !MemberInfoFilter(memberInfo))
                return null;
            return ConvertToContentEntity(memberInfo);
        }

        protected bool MemberInfoFilter(MemberInfo memberInfo){
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            Contract.EndContractBlock();
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
            if(type == null) throw new ArgumentNullException("type");
            Contract.EndContractBlock();
            return type.GetExternalVisibility() != ExternalVisibilityKind.Hidden;
        }

        protected virtual bool MethodBaseFilter(MethodBase methodBase) {
            if (methodBase == null) throw new ArgumentNullException("methodBase");
            Contract.EndContractBlock();
            if (methodBase.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            var name = methodBase.Name;
            if (name.Length >= 2 && (name[0] == '$' || name[name.Length - 1] == '$'))
                return false;
            if (methodBase.IsSpecialName && !methodBase.IsConstructor && !methodBase.IsOperatorOverload())
                return false;
            if (methodBase.IsFinalizer())
                return false;
            return true;
        }

        protected virtual bool FieldInfoFilter(FieldInfo fieldInfo) {
            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");
            Contract.EndContractBlock();
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
            if (eventInfo == null) throw new ArgumentNullException("eventInfo");
            Contract.EndContractBlock();
            if (eventInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (eventInfo.IsSpecialName)
                return false;
            return true;
        }

        protected virtual bool PropertyInfoFilter(PropertyInfo propertyInfo) {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            Contract.EndContractBlock();
            if (propertyInfo.GetExternalVisibility() == ExternalVisibilityKind.Hidden)
                return false;
            if (propertyInfo.IsSpecialName)
                return false;
            return true;
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

            throw new NotSupportedException();
        }

        private CodeDocType ConvertToTypeEntity(Type type){
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<CodeDocType>() != null);
            var result = new CodeDocType(GetCRefIdentifier(type));
            ApplyStandardXmlDocs(result, result.CRef.FullCRef);
            ApplyTypeAttributes(result, type);
            return result;
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
            if (fieldInfo.IsLiteral)
                model.SubTitle = "Constant";
            else
                model.SubTitle = "Field";
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

            if (propertyInfo.IsItemIndexerProperty())
                model.SubTitle = "Indexer";
            else
                model.SubTitle = "Property";
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

            if (methodBase.IsConstructor)
                model.SubTitle = "Constructor";
            else if (methodBase.IsOperatorOverload())
                model.SubTitle = "Operator";
            else
                model.SubTitle = "Method";
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

        private void ApplyTypeAttributes(CodeDocType model, Type type){
            Contract.Requires(model != null);
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(model.ShortName));
            Contract.Ensures(!String.IsNullOrEmpty(model.FullName));
            Contract.Ensures(!String.IsNullOrEmpty(model.Title));
            Contract.Ensures(model.Title == model.ShortName);
            Contract.Ensures(!String.IsNullOrEmpty(model.SubTitle));

            ApplyCommonTypeAttributes(model, type);

            model.IsEnum = type.IsEnum;

            if (!type.IsInterface) {
                var currentBase = type.BaseType;
                if (null != currentBase) {
                    model.BaseCRef = GetCRefIdentifier(currentBase);
                    var baseChain = new List<CRefIdentifier>() {
                        model.BaseCRef
                    };
                    currentBase = currentBase.BaseType;
                    while (currentBase != null) {
                        baseChain.Add(GetCRefIdentifier(currentBase));
                        currentBase = currentBase.BaseType;
                    }
                    model.BaseChainCRefs = new ReadOnlyCollection<CRefIdentifier>(baseChain);
                }
            }

            var implementedInterfaces = type.GetInterfaces();
            if (implementedInterfaces.Length > 0) {
                model.DirectInterfaceCRefs = Array.AsReadOnly(
                    Array.ConvertAll(implementedInterfaces, GetCRefIdentifier));
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
                                genericModel.TypeConstraints = Array.AsReadOnly(Array.ConvertAll(typeConstraints, GetCRefIdentifier));

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
            foreach (var nestedType in type.GetAllNestedTypes().Where(TypeFilter)){
                var nestedTypeModel = ConvertToSimpleEntity(nestedType);
                if(nestedType.IsDelegateType())
                    nestedDelegateModels.Add(nestedTypeModel);
                else
                    nestedTypeModels.Add(nestedTypeModel);
            }
            model.NestedTypes = new ReadOnlyCollection<ICodeDocEntity>(nestedTypeModels);
            model.NestedDelegates = new List<ICodeDocEntity>(nestedDelegateModels);

            var methodModels = new List<ICodeDocEntity>();
            var operatorModels = new List<ICodeDocEntity>();
            foreach (var methodInfo in type.GetAllMethods().Where(MethodBaseFilter)){
                var methodModel = ConvertToSimpleEntity(methodInfo);
                if(methodInfo.IsOperatorOverload())
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

        }

    }
}
