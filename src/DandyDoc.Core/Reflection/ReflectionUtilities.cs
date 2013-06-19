using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Utility;

namespace DandyDoc.Reflection
{
    /// <summary>
    /// Various methods and extension methods related to reflection.
    /// </summary>
    public static class ReflectionUtilities
    {

        private const BindingFlags BindAllTheThings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Gets the full file path for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to locate.</param>
        /// <returns>The best file path that could be found for the assembly.</returns>
        /// <remarks>
        /// Be aware that finding an assembly on disk is not as simple as <seealso cref="System.Reflection.Assembly.Location"/>.
        /// In some situations such as automated testing the location may not be correctly reported.
        /// This method attempts multiple ways of finding the location.
        /// </remarks>
        public static string GetFilePath(this Assembly assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();

            var codeBase = assembly.CodeBase;
            Uri uri;
            if (Uri.TryCreate(codeBase, UriKind.Absolute, out uri) && "FILE".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(uri.AbsolutePath);

            return assembly.Location;
        }

        /// <summary>
        /// Determines if a property is static.
        /// </summary>
        /// <param name="propertyInfo">The property to test.</param>
        /// <returns><c>true</c> if the property is static.</returns>
        public static bool IsStatic(this PropertyInfo propertyInfo){
            if(propertyInfo == null) throw new NullReferenceException("propertyInfo is null");
            Contract.EndContractBlock();
            var method = propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true);
            return method != null && method.IsStatic;
        }

        /// <summary>
        /// Determines if an event is static.
        /// </summary>
        /// <param name="eventInfo">The event to test.</param>
        /// <returns><c>true</c> if the event is static.</returns>
        public static bool IsStatic(this EventInfo eventInfo){
            if(eventInfo == null) throw new NullReferenceException("eventInfo is null");
            Contract.EndContractBlock();
            var method = eventInfo.GetAddMethod(true) ?? eventInfo.GetRaiseMethod(true);
            return method != null && method.IsStatic;
        }

        /// <summary>
        /// Determines if a type is static.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if the type is static.</returns>
        /// <remarks>
        /// A type is static when it is both abstract and sealed.
        /// </remarks>
        public static bool IsStatic(this Type type){
            if(type == null) throw new NullReferenceException("type is null");
            Contract.EndContractBlock();
            return type.IsAbstract && type.IsSealed;
        }

        /// <summary>
        /// Determines if a member is static.
        /// </summary>
        /// <param name="memberInfo">The member to test.</param>
        /// <returns><c>true</c> if the member is static.</returns>
        public static bool IsStatic(this MemberInfo memberInfo){
            if(memberInfo == null) throw new NullReferenceException("memberInfo is null");
            Contract.EndContractBlock();
            if (memberInfo is Type)
                return ((Type)memberInfo).IsStatic();
            if (memberInfo is MethodBase)
                return ((MethodBase)memberInfo).IsStatic;
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).IsStatic;
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).IsStatic();
            if (memberInfo is EventInfo)
                return ((EventInfo)memberInfo).IsStatic();
            return false;
        }

        /// <summary>
        /// Determines if a method is an operator overload.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns><c>true</c> when the method is an operator overload.</returns>
        public static bool IsOperatorOverload(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            if (!methodBase.IsStatic)
                return false;
            return CSharpOperatorNameSymbolMap.IsOperatorName(methodBase.Name);
        }

        /// <summary>
        /// Determines if a method is a finalizer method.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns><c>true</c> when the method is a finalizer.</returns>
        public static bool IsFinalizer(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            return !methodBase.IsStatic
                && "Finalize".Equals(methodBase.Name)
                && methodBase.GetParameters().Length == 0;
        }

        /// <summary>
        /// Determines if a property is an item indexer.
        /// </summary>
        /// <param name="propertyInfo">The property to test.</param>
        /// <returns><c>true</c> when the property is an item indexer.</returns>
        public static bool IsItemIndexerProperty(this PropertyInfo propertyInfo) {
            if(propertyInfo == null) throw new NullReferenceException("propertyInfo is null");
            Contract.EndContractBlock();
            return "Item".Equals(propertyInfo.Name)
                && propertyInfo.GetIndexParameters().Length > 0;
        }

        /// <summary>
        /// Determines if a type is a delegate.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> when the type is a delegate.</returns>
        public static bool IsDelegateType(this Type type) {
            if (type == null)
                return false;

            var baseType = type.BaseType;
            if (baseType == null)
                return false;

            if (!"System.MulticastDelegate".Equals(baseType.FullName))
                return false;

            var methods = type.GetAllMethods();
            return methods.Length > 0
                && methods.Any(x => "Invoke".Equals(x.Name));
        }

        /// <summary>
        /// Extracts the parameters for a delegate type from the invoke method.
        /// </summary>
        /// <param name="type">The delegate type to extract parameters from.</param>
        /// <returns>The parameters for the delegate.</returns>
        public static ParameterInfo[] GetDelegateTypeParameters(this Type type) {
            Contract.Ensures(Contract.Result<ParameterInfo[]>() != null);

            if (!IsDelegateType(type))
                return new ParameterInfo[0];

            var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod == null)
                return new ParameterInfo[0];

            return invokeMethod.GetParameters();
        }

        /// <summary>
        /// Extracts the return parameter for a delegate type from the invoke method.
        /// </summary>
        /// <param name="type">The delegate type to extract a return parameter from.</param>
        /// <returns>The return parameter.</returns>
        public static ParameterInfo GetDelegateReturnParameter(this Type type) {
            if (!IsDelegateType(type))
                return null;

            var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod == null)
                return null;

            return invokeMethod.ReturnParameter;
        }

        /// <summary>
        /// Extracts the return type for a delegate type from the invoke method.
        /// </summary>
        /// <param name="type">The delegate type to extract a return type from.</param>
        /// <returns>The return type.</returns>
        public static Type GetDelegateReturnType(this Type type) {
            if (!IsDelegateType(type))
                return null;

            var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod == null)
                return null;

            return invokeMethod.ReturnType;
        }

        /// <summary>
        /// Gets all constructors for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        public static ConstructorInfo[] GetAllConstructors(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);
            return type.GetConstructors(BindAllTheThings);
        }

        /// <summary>
        /// Gets all methods for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        /// <remarks>
        /// This methods will not return constructors.
        /// </remarks>
        public static MethodInfo[] GetAllMethods(this Type type, bool inheritStatic = false) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<MethodInfo[]>() != null);
            var members = type.GetMethods(BindAllTheThings);
            if (!inheritStatic)
                members = type.IsStatic()
                    ? members.Where(m => m.DeclaringType == type).ToArray()
                    : members.Where(m => m.DeclaringType == type || !m.IsStatic).ToArray();
            return members;
        }

        /// <summary>
        /// Gets all properties for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        public static PropertyInfo[] GetAllProperties(this Type type, bool inheritStatic = false) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<PropertyInfo[]>() != null);
            var members = type.GetProperties(BindAllTheThings);
            if (!inheritStatic)
                members = type.IsStatic()
                    ? members.Where(m => m.DeclaringType == type).ToArray()
                    : members.Where(m => m.DeclaringType == type || !m.IsStatic()).ToArray();
            return members;
        }

        /// <summary>
        /// Gets all fields for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        public static FieldInfo[] GetAllFields(this Type type, bool inheritStatic = false) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<FieldInfo[]>() != null);
            var members = type.GetFields(BindAllTheThings);
            if (!inheritStatic)
                members = type.IsStatic()
                    ? members.Where(m => m.DeclaringType == type).ToArray()
                    : members.Where(m => m.DeclaringType == type || !m.IsStatic).ToArray();
            return members;
        }

        /// <summary>
        /// Gets all events for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        public static EventInfo[] GetAllEvents(this Type type, bool inheritStatic = false) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<EventInfo[]>() != null);
            var members = type.GetEvents(BindAllTheThings);
            if (!inheritStatic)
                members = type.IsStatic()
                    ? members.Where(m => m.DeclaringType == type).ToArray()
                    : members.Where(m => m.DeclaringType == type || !m.IsStatic()).ToArray();
            return members;
        }

        /// <summary>
        /// Gets all nested types for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to extract members from.</param>
        /// <returns>The requested members from the type.</returns>
        public static Type[] GetAllNestedTypes(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<Type[]>() != null);
            return type.GetNestedTypes(BindAllTheThings);
        }

        /// <summary>
        /// Determines if a member has an attribute of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="memberInfo">The member to search for attributes on.</param>
        /// <param name="type">The attribute type to search for.</param>
        /// <returns><c>true</c> when an attribute matching the <paramref name="type"/> is found.</returns>
        public static bool HasAttribute(this MemberInfo memberInfo, Type type) {
            if (memberInfo == null) throw new NullReferenceException("memberInfof is null");
            if (type == null) throw new ArgumentNullException("type");
            return memberInfo
                .GetCustomAttributesData()
                .Any(x => x.Constructor.DeclaringType == type);
        }

        /// <summary>
        /// Determines if any attribute on a member satisfies a predicate.
        /// </summary>
        /// <param name="memberInfo">The member to search for attributes on.</param>
        /// <param name="predicate">The test to apply to all attributes.</param>
        /// <returns><c>true</c> if any attribute satisfies the <paramref name="predicate"/>.</returns>
        public static bool HasAttribute(this MemberInfo memberInfo, Func<CustomAttributeData, bool> predicate) {
            if (memberInfo == null) throw new NullReferenceException("memberInfo is null");
            if (predicate == null) throw new ArgumentNullException("predicate");
            return memberInfo
                .GetCustomAttributesData()
                .Any(predicate);
        }

        /// <summary>
        /// Determines if any attribute on a parameter matches a predicate.
        /// </summary>
        /// <param name="parameterInfo">The parameter to search for attribbutes on.</param>
        /// <param name="predicate">The test to apply to all attributes.</param>
        /// <returns><c>true</c> if any attribute matches the <paramref name="predicate"/>.</returns>
        public static bool HasAttribute(this ParameterInfo parameterInfo, Func<CustomAttributeData, bool> predicate) {
            if (parameterInfo == null) throw new NullReferenceException("parameterInfo is null");
            if (predicate == null) throw new ArgumentNullException("predicate");
            return parameterInfo
                .GetCustomAttributesData()
                .Any(predicate);
        }

        /// <summary>
        /// Determines if a method is an extension method.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns><c>true</c> if the methods is an extension method.</returns>
        public static bool IsExtensionMethod(this MethodBase methodBase) {
            if(methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            if (methodBase.GetParameters().Length == 0)
                return false;
            return methodBase.HasAttribute(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
        }

        /// <summary>
        /// Determines if a method is an override of a base method.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns><c>true</c> if the method is an override.</returns>
        public static bool IsOverride(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            return methodBase.IsVirtual && methodBase.Attributes.HasFlag(MethodAttributes.ReuseSlot);
        }

        /// <summary>
        /// Determines if a method is sealed.
        /// </summary>
        /// <param name="methodBase">The method to test.</param>
        /// <returns><c>true</c> if the method is sealed.</returns>
        public static bool IsSealed(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            return methodBase.IsFinal && methodBase.IsOverride();
        }

        /// <summary>
        /// Determines if a type is a <see cref="T:System.Nullable`1"/> type.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if the type is nullable.</returns>
        public static bool IsNullable(this Type type) {
            Contract.Requires(type != null);
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Returns the base property of a property if found.
        /// </summary>
        /// <param name="propertyInfo">The property to find the base property of.</param>
        /// <returns>A property or <c>null</c> if a base was not found.</returns>
        public static PropertyInfo FindNextAncestor(this PropertyInfo propertyInfo) {
            Contract.Requires(propertyInfo != null);
            var propertyName = propertyInfo.Name;
            var ancestorMethods = propertyInfo
                .GetAccessors(true)
                .Select(FindNextAncestor)
                .Where(a => a != null);
            foreach (var ancestor in ancestorMethods) {
                var baseType = ancestor.DeclaringType;
                Contract.Assume(baseType != null);
                var baseProperty = baseType.GetProperty(propertyName, BindAllTheThings);
                if (Array.IndexOf(baseProperty.GetAccessors(), ancestor) >= 0)
                    return baseProperty;
            }
            return null;
        }

        /// <summary>
        /// Returns the base of an event if one is found.
        /// </summary>
        /// <param name="eventInfo">The event to find the base event of.</param>
        /// <returns>An event or <c>null</c> if a base was not found.</returns>
        public static EventInfo FindNextAncestor(this EventInfo eventInfo) {
            Contract.Requires(eventInfo != null);
            var eventName = eventInfo.Name;
            var invokeMethod = eventInfo.GetRaiseMethod(true);
            if (invokeMethod == null)
                return null; // nothing we can work with

            var ancestor = invokeMethod.FindNextAncestor();
            if (ancestor != null) {
                Contract.Assume(ancestor.DeclaringType != null);
                var baseEvent = ancestor.DeclaringType.GetEvent(eventName, BindAllTheThings);
                if (baseEvent != null && baseEvent != eventInfo && baseEvent.GetRaiseMethod(true) == ancestor)
                    return baseEvent;
            }

            return null;
        }

        public static MethodInfo FindNextAncestor(this MethodInfo methodInfo) {
            Contract.Requires(methodInfo != null);
            var methodName = methodInfo.Name;
            var methodParameterTypes = Array.ConvertAll(methodInfo.GetParameters(), p => p.ParameterType);
            Contract.Assume(methodInfo.DeclaringType != null);
            var baseType = methodInfo.ReflectedType != methodInfo.DeclaringType
                ? methodInfo.DeclaringType
                : methodInfo.DeclaringType.BaseType;

            while (baseType != null) {
                var matchingMethod = baseType.GetMethod(methodName, BindAllTheThings, null, methodParameterTypes, null);
                if (matchingMethod != null) {
                    if (matchingMethod.DeclaringType == baseType)
                        return matchingMethod;

                    // skip ahead
                    baseType = matchingMethod.DeclaringType;
                }
                else {
                    baseType = baseType.BaseType;
                }
            }

            if (methodInfo.DeclaringType.IsInterface)
                return null;
            
            foreach (var interfaceType in methodInfo.DeclaringType.GetInterfaces()) {
                var map = methodInfo.DeclaringType.GetInterfaceMap(interfaceType);
                var targetMethods = map.TargetMethods;
                for (int i = 0; i < targetMethods.Length; i++) {
                    if (targetMethods[i] == methodInfo) {
                        return map.InterfaceMethods[i];
                    }
                 }
            }

            return null;
        }

        /// <summary>
        /// Determines if a method has a non-void return type.
        /// </summary>
        /// <param name="methodInfo">The method to test.</param>
        /// <returns><c>true</c> when the method returns a non-void type.</returns>
        public static bool HasNonVoidReturn(this MethodInfo methodInfo){
            Contract.Requires(methodInfo != null);
            return methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void);
        }

    }
}
