﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;
using System.Collections.ObjectModel;

namespace DandyDoc
{
	[Obsolete("Need to move this stuff.")]
	public static class CecilExtensions
	{

		[Obsolete]
		[Pure]
		public static IList<EventDefinition> GetAllEvents(this TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(Contract.Result<IList<EventDefinition>>() != null);
			var result = new List<EventDefinition>();
			if (definition.HasEvents)
				result.AddRange(definition.Events);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseEvents(baseDef, result);
				}
			}
			return result;
		}

		[Obsolete]
		[Pure]
		private static void AppendNeededBaseEvents(TypeDefinition definition, List<EventDefinition> baseMembers) {
			var newAdditions = new List<EventDefinition>();
			foreach (var e in definition.Events) {
				if (baseMembers.Any(b => b.Name == e.Name))
					continue;

				newAdditions.Add(e);
			}

			if (newAdditions.Count > 0)
				baseMembers.AddRange(newAdditions);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseEvents(baseDef, baseMembers);
				}
			}
		}

		[Obsolete]
		[Pure]
		public static IList<PropertyDefinition> GetAllProperties(this TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(Contract.Result<IList<PropertyDefinition>>() != null);
			var result = new List<PropertyDefinition>();
			if (definition.HasProperties)
				result.AddRange(definition.Properties);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseProperties(baseDef, result);
				}
			}
			return result;
		}

		[Obsolete]
		[Pure]
		private static void AppendNeededBaseProperties(TypeDefinition definition, List<PropertyDefinition> baseMembers) {
			var newAdditions = new List<PropertyDefinition>();
			foreach (var property in definition.Properties) {
				if (baseMembers.Any(b => b.Name == property.Name))
					continue;

				newAdditions.Add(property);
			}

			if (newAdditions.Count > 0)
				baseMembers.AddRange(newAdditions);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseProperties(baseDef, baseMembers);
				}
			}
		}

		[Obsolete]
		[Pure]
		public static IList<FieldDefinition> GetAllFields(this TypeDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(Contract.Result<IList<FieldDefinition>>() != null);
			var result = new List<FieldDefinition>();
			if(definition.HasFields)
				result.AddRange(definition.Fields);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseFields(baseDef, result);
				}
			}
			return result;
		}

		[Obsolete]
		[Pure]
		private static void AppendNeededBaseFields(TypeDefinition definition, List<FieldDefinition> baseMembers) {
			var newAdditions = new List<FieldDefinition>();
			foreach (var field in definition.Fields) {
				if (baseMembers.Any(b => b.Name == field.Name))
					continue;

				newAdditions.Add(field);
			}

			if (newAdditions.Count > 0)
				baseMembers.AddRange(newAdditions);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseFields(baseDef, baseMembers);
				}
			}
		}

		[Obsolete]
		[Pure] public static IList<MethodDefinition> GetAllMethods(this TypeDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(Contract.Result<IList<MethodDefinition>>() != null);
			var result = new List<MethodDefinition>();
			if(definition.HasMethods)
				result.AddRange(definition.Methods);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseMethods(baseDef, result);
				}
			}
			return result;
		}

		[Obsolete]
		[Pure] private static void AppendNeededBaseMethods(TypeDefinition definition, List<MethodDefinition> baseMembers) {
			var newAdditions = new List<MethodDefinition>();
			foreach (var method in definition.Methods) {
				if (method.IsConstructor)
					continue;

				if (baseMembers.Any(b => SignaturesEqual(b, method)))
					continue;

				newAdditions.Add(method);
			}

			if(newAdditions.Count > 0)
				baseMembers.AddRange(newAdditions);

			var baseRef = definition.BaseType;
			if (null != baseRef) {
				var baseDef = baseRef.Resolve();
				if (null != baseDef) {
					AppendNeededBaseMethods(baseDef, baseMembers);
				}
			}
		}

		[Obsolete]
		public static bool SignaturesEqual(MethodDefinition a, MethodDefinition b) {
			if (null == a)
				return null == b;
			if (null == b)
				return false;
			if (a.Name != b.Name)
				return false;
			if (!a.HasParameters)
				return !b.HasParameters;
			if (a.Parameters.Count != b.Parameters.Count)
				return false;

			for (int i = 0; i < a.Parameters.Count; i++) {
				if (a.Parameters[i].ParameterType != b.Parameters[i].ParameterType)
					return false;
			}
			return true;
		}

		[Obsolete]
		[Pure] public static bool IsDelegateType(this TypeDefinition typeDefinition){
			if (null == typeDefinition)
				return false;

			var baseType = typeDefinition.BaseType;
			if (null == baseType)
				return false;

			if (!baseType.FullName.Equals("System.MulticastDelegate"))
				return false;

			return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
		}

		[Obsolete]
		private static readonly ReadOnlyCollection<ParameterDefinition> EmptyParameterDefinitionCollection = Array.AsReadOnly(new ParameterDefinition[0]);

		[Obsolete]
		public static IList<ParameterDefinition> GetDelegateTypeParameters(this TypeDefinition typeDefinition){
			Contract.Ensures(Contract.Result<IList<ParameterDefinition>>() != null);
			if (!IsDelegateType(typeDefinition))
				return EmptyParameterDefinitionCollection;

			Contract.Assume(typeDefinition.Methods != null);
			var method = typeDefinition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
			return null == method || !method.HasParameters
				? (IList<ParameterDefinition>)EmptyParameterDefinitionCollection
				: method.Parameters;
		}

		[Obsolete]
		public static TypeReference GetDelegateReturnType(this TypeDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			if(!definition.IsDelegateType()) throw new ArgumentException("Definition must be a delegate type.", "delegate");
			Contract.Ensures(Contract.Result<TypeReference>() != null);
			Contract.Assume(definition.Methods != null);
			var method = definition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
			if(null == method)
				throw new ArgumentException("Definition does not have an Invoke method.");

			return method.ReturnType;
		}

		[Obsolete]
		private static readonly HashSet<string> OperatorMethodNames = new HashSet<string>{
			"op_Implicit",
			"op_explicit",
			"op_Addition",
			"op_Subtraction",
			"op_Multiply",
			"op_Division",
			"op_Modulus",
			"op_ExclusiveOr",
			"op_BitwiseAnd",
			"op_BitwiseOr",
			"op_LogicalAnd",
			"op_LogicalOr",
			"op_Assign",
			"op_LeftShift",
			"op_RightShift",
			"op_SignedRightShift",
			"op_UnsignedRightShift",
			"op_Equality",
			"op_GreaterThan",
			"op_LessThan",
			"op_Inequality",
			"op_GreaterThanOrEqual",
			"op_LessThanOrEqual",
			"op_MultiplicationAssignment",
			"op_SubtractionAssignment",
			"op_ExclusiveOrAssignment",
			"op_LeftShiftAssignment",
			"op_ModulusAssignment",
			"op_AdditionAssignment",
			"op_BitwiseAndAssignment",
			"op_BitwiseOrAssignment",
			"op_Comma",
			"op_DivisionAssignment",
			"op_Decrement",
			"op_Increment",
			"op_UnaryNegation",
			"op_UnaryPlus",
			"op_OnesComplement"
		};

		[Obsolete]
		public static bool IsOperatorOverload(this MethodDefinition methodDefinition){
			if(null == methodDefinition) throw new ArgumentNullException("methodDefinition");
			Contract.EndContractBlock();
			if (!methodDefinition.IsStatic)
				return false;
			Contract.Assume(!String.IsNullOrEmpty(methodDefinition.Name));
			return OperatorMethodNames.Contains(methodDefinition.Name);
		}

		[Obsolete]
		public static bool IsItemIndexerProperty(this PropertyDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return definition.HasParameters && "Item".Equals(definition.Name);
		}

		[Obsolete]
		public static bool IsFinalizer(this MethodDefinition methodDefinition){
			if (null == methodDefinition) throw new ArgumentNullException("methodDefinition");
			Contract.EndContractBlock();
			return !methodDefinition.IsStatic
				&& !methodDefinition.HasParameters
				&& "Finalize".Equals(methodDefinition.Name);
		}

		[Obsolete]
		public static bool IsStatic(this PropertyDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.GetMethod ?? definition.SetMethod;
			return null != method && method.IsStatic;
		}

		[Obsolete]
		public static bool IsStatic(this EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.AddMethod ?? definition.InvokeMethod;
			return null != method && method.IsStatic;
		}

		[Obsolete]
		public static bool IsStatic(this TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return definition.IsAbstract && definition.IsSealed;
		}

		[Obsolete]
		public static bool IsStatic(this IMemberDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (definition is MethodDefinition)
				return ((MethodDefinition)definition).IsStatic;
			if (definition is FieldDefinition)
				return ((FieldDefinition)definition).IsStatic;
			if (definition is TypeDefinition)
				return IsStatic((TypeDefinition)definition);
			if (definition is PropertyDefinition)
				return IsStatic((PropertyDefinition)definition);
			if (definition is EventDefinition)
				return IsStatic((EventDefinition)definition);
			throw new NotSupportedException();
		}

		[Obsolete]
		public static bool IsFinal(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsFinal)
				|| (null != definition.SetMethod && definition.SetMethod.IsFinal);
		}

		[Obsolete]
		public static bool IsAbstract(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsAbstract)
				|| (null != definition.SetMethod && definition.SetMethod.IsAbstract);
		}

		[Obsolete]
		public static bool IsVirtual(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsVirtual)
				|| (null != definition.SetMethod && definition.SetMethod.IsVirtual);
		}

		[Obsolete]
		public static bool IsOverride(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsOverride())
				|| (null != definition.SetMethod && definition.SetMethod.IsOverride());
		}

		[Obsolete]
		public static bool IsSealed(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsSealed())
				|| (null != definition.SetMethod && definition.SetMethod.IsSealed());
		}

		[Obsolete]
		public static bool IsNewSlot(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return (null != definition.GetMethod && definition.GetMethod.IsNewSlot)
				|| (null != definition.SetMethod && definition.SetMethod.IsNewSlot);
		}

		[Obsolete]
		public static bool IsOverride(this MethodDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return definition.IsVirtual && definition.IsReuseSlot;
		}

		[Obsolete]
		public static bool IsSealed(this MethodDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return definition.IsFinal && definition.IsOverride();
		}

		[Obsolete]
		public static bool IsExtensionMethod(this MethodDefinition definition){
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (!definition.HasParameters || definition.Parameters.Count == 0 || !definition.HasCustomAttributes)
				return false;

			return definition.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute");
		}

		[Obsolete]
		public static bool HasPureAttribute(this ICustomAttributeProvider definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return HasAttributeMatchingShortName(definition, "PureAttribute");
		}

		[Obsolete]
		public static bool HasFlagsAttribute(this ICustomAttributeProvider definition){
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return HasAttributeMatchingFullName(definition, "System.FlagsAttribute");
		}

		[Obsolete]
		public static bool HasObsoleteAttribute(this ICustomAttributeProvider definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			return HasAttributeMatchingFullName(definition, "System.ObsoleteAttribute");
		}

		[Obsolete]
		public static bool HasAttributeMatchingName(this ICustomAttributeProvider definition, string name) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Contract.Assume(null != definition.CustomAttributes);
			return definition.HasCustomAttributes
				&& definition.CustomAttributes.Select(a => a.AttributeType).Any(t => t.FullName == name || t.Name == name);
		}

		[Obsolete]
		public static bool HasAttributeMatchingShortName(this ICustomAttributeProvider definition, string name){
			if (null == definition) throw new ArgumentNullException("definition");
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Valid name is required.", name);
			Contract.EndContractBlock();
			Contract.Assume(null != definition.CustomAttributes);
			return definition.HasCustomAttributes
				&& definition.CustomAttributes.Any(a => a.AttributeType.Name == name);
		}

		[Obsolete]
		public static bool HasAttributeMatchingFullName(this ICustomAttributeProvider definition, string name) {
			if (null == definition) throw new ArgumentNullException("definition");
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Valid name is required.", name);
			Contract.EndContractBlock();
			Contract.Assume(null != definition.CustomAttributes);
			return definition.HasCustomAttributes
				&& definition.CustomAttributes.Any(a => a.AttributeType.FullName == name);
		}

	}
}
