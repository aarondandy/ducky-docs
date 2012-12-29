using System;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.ExternalVisibility
{
	public static class ExternalVisibilityOverlay
	{

		// ---- Extensions

		public static bool IsExternallyVisible(this IMemberDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyVisible(this MethodDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyVisible(this FieldDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyVisible(this PropertyDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyVisible(this EventDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyVisible(this TypeDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) != ExternalVisibilityKind.Hidden;
		}

		public static bool IsExternallyProtected(this IMemberDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		public static bool IsExternallyProtected(this MethodDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		public static bool IsExternallyProtected(this FieldDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		public static bool IsExternallyProtected(this PropertyDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		public static bool IsExternallyProtected(this EventDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		public static bool IsExternallyProtected(this TypeDefinition definition) {
			Contract.Requires(null != definition);
			return Get(definition) == ExternalVisibilityKind.Protected;
		}

		// ---- Core methods

		public static ExternalVisibilityKind Min(ExternalVisibilityKind a, ExternalVisibilityKind b) {
			return a <= b ? a : b;
		}

		public static ExternalVisibilityKind Max(ExternalVisibilityKind a, ExternalVisibilityKind b) {
			return a >= b ? a : b;
		}

		public static ExternalVisibilityKind Get(IMemberDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			if (definition is TypeDefinition) {
				return Get((TypeDefinition)definition);
			}
			if (definition is MethodDefinition) {
				return Get((MethodDefinition)definition);
			}
			if (definition is PropertyDefinition) {
				return Get((PropertyDefinition)definition);
			}
			if (definition is FieldDefinition) {
				return Get((FieldDefinition)definition);
			}
			if (definition is EventDefinition) {
				return Get((EventDefinition)definition);
			}
			throw new NotSupportedException();
		}

		private static ExternalVisibilityKind GetNestedVisibility(TypeDefinition definition) {
			Contract.Requires(null != definition);
			if(definition.IsNestedPublic)
				return ExternalVisibilityKind.Public;
			if (definition.IsNestedFamily || definition.IsNestedFamilyOrAssembly)
				return ExternalVisibilityKind.Protected;
			return ExternalVisibilityKind.Hidden;
		}

		public static ExternalVisibilityKind Get(TypeDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			if (definition.IsNested) {
				Contract.Assume(null != definition.DeclaringType);
				var parentVisibility = Get(definition.DeclaringType);
				if(parentVisibility == ExternalVisibilityKind.Hidden)
					return ExternalVisibilityKind.Hidden;
				var thisVisibility = GetNestedVisibility(definition);
				return Min(parentVisibility, thisVisibility);
			}
			return definition.IsPublic ? ExternalVisibilityKind.Public : ExternalVisibilityKind.Hidden;
		}

		public static ExternalVisibilityKind Get(FieldDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Contract.Assume(null != definition.DeclaringType);
			var typeVis = Get(definition.DeclaringType);
			var fieldVis =
				definition.IsPublic
				? ExternalVisibilityKind.Public
				: (definition.IsFamily || definition.IsFamilyOrAssembly)
				? ExternalVisibilityKind.Protected
				: ExternalVisibilityKind.Hidden;
			return Min(typeVis, fieldVis);
		}

		public static ExternalVisibilityKind Get(MethodDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			Contract.Assume(null != definition.DeclaringType);
			var typeVis = Get(definition.DeclaringType);
			var fieldVis =
				definition.IsPublic
				? ExternalVisibilityKind.Public
				: (definition.IsFamily || definition.IsFamilyOrAssembly)
				? ExternalVisibilityKind.Protected
				: ExternalVisibilityKind.Hidden;
			return Min(typeVis, fieldVis);
		}

		public static ExternalVisibilityKind Get(EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.InvokeMethod ?? definition.AddMethod;
			return null == method ? ExternalVisibilityKind.Hidden : Get(method);
		}

		public static ExternalVisibilityKind Get(PropertyDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var getMethod = definition.GetMethod;
			var setMethod = definition.SetMethod;
			if (null == getMethod) {
				return null == setMethod
					? ExternalVisibilityKind.Hidden
					: Get(setMethod);
			}
			return null == setMethod
				? Get(getMethod)
				: Max(Get(getMethod), Get(setMethod));
		}

	}
}
