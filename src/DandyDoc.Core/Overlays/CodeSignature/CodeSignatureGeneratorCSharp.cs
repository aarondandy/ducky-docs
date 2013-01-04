using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using DandyDoc.Overlays.DisplayName;
using Mono.Cecil;

namespace DandyDoc.Overlays.CodeSignature
{
	public class CodeSignatureGeneratorCSharp : CodeSignatureGeneratorBase
	{

		private static readonly DisplayNameOverlay ShortDisplayNameOverlay;

		static CodeSignatureGeneratorCSharp() {
			ShortDisplayNameOverlay = new DisplayNameOverlay() {
				IncludeParameterNames = true,
				ReplaceItemWithThis = true
			};
			ShortDisplayNameOverlay.ParameterTypeDisplayNameOverlay = ShortDisplayNameOverlay;
		}

		public CodeSignatureGeneratorCSharp() : base("csharp") { }

		private IEnumerable<string> GetModifiers(MethodDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

			var access = GetAccessModifier(definition);
			if (!String.IsNullOrEmpty(access))
				yield return access;

			if (definition.IsStatic)
				yield return "static";
			if (definition.IsSealed())
				yield return "sealed";

			if (definition.IsAbstract)
				yield return "abstract";
			else if (!definition.IsFinal && definition.IsVirtual && definition.IsNewSlot)
				yield return "virtual";
			
			if (definition.IsOverride())
				yield return "override";
			
		}

		private string GetAccessModifier(FieldDefinition definition) {
			Contract.Requires(null != definition);
			if (definition.IsPublic)
				return "public";
			if (definition.IsPrivate)
				return "private";
			if (definition.IsFamily)
				return "protected";
			if (definition.IsFamilyOrAssembly)
				return "protected internal";
			return "internal";
		}

		private IEnumerable<string> GetModifiers(FieldDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

			var accessModifier = GetAccessModifier(definition);
			if (!String.IsNullOrEmpty(accessModifier))
				yield return accessModifier;

			if (definition.HasConstant) {
				yield return "const";
			}
			else {
				if (definition.IsStatic)
					yield return "static";
				if (definition.IsInitOnly)
					yield return "readonly";
			}
		}

		private IEnumerable<string> GetModifiers(EventDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

			var method = definition.InvokeMethod ?? definition.AddMethod;
			if (null != method) {
				var accessModifier = GetAccessModifier(method);
				if (!String.IsNullOrEmpty(accessModifier))
					yield return accessModifier;

				if (method.IsStatic)
					yield return "static";
			}
		}

		private string GetAccessModifier(TypeDefinition definition) {
			Contract.Requires(definition != null);
			if (definition.IsPublic || definition.IsNestedPublic)
				return "public";
			if (definition.IsNestedPrivate)
				return "private";
			if (definition.IsNestedFamily)
				return "protected";
			if (definition.IsNestedFamilyOrAssembly)
				return "protected internal";
			return "internal";
		}

		private IEnumerable<string> GetModifiers(TypeDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

			var accessModifier = GetAccessModifier(definition);
			if(!String.IsNullOrEmpty(accessModifier))
				yield return accessModifier;

			if (!definition.IsEnum && definition.IsStatic())
				yield return "static";
			
			
			if (definition.IsDelegateType()) {
				yield return "delegate";
			}
			else if (definition.IsEnum) {
				yield return "enum";
			}
			else {
				if (!definition.IsValueType && definition.IsSealed)
					yield return "sealed";

				if (definition.IsInterface) {
					yield return "interface";
				}
				else {
					if (definition.IsAbstract)
						yield return "abstract";

					yield return definition.IsValueType ? "struct" : "class";
				}
			}
		}

		private string GetAccessModifier(MethodDefinition definition) {
			Contract.Requires(definition != null);
			Contract.Ensures(Contract.Result<string>() != null);
			if (definition.IsPublic)
				return "public";
			if (definition.IsPrivate)
				return "private";
			if (definition.IsFamily)
				return "protected";
			if (definition.IsAssembly)
				return "internal";
			if (definition.IsFamilyOrAssembly)
				return "protected internal";
			return String.Empty;
		}

		private IEnumerable<TypeReference> GetBases(TypeDefinition definition) {
			Contract.Requires(null != definition);
			Contract.Ensures(Contract.Result<IEnumerable<TypeReference>>() != null);

			if (!definition.IsValueType && null != definition.BaseType)
				yield return definition.BaseType;

			if (definition.HasInterfaces)
				foreach (var iface in definition.Interfaces)
					yield return iface;
		}

		public override CodeSignature GenerateSignature(MethodDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var modifiers = String.Join(" ", GetModifiers(definition));
			var codeBuilder = new StringBuilder(modifiers);
			if(!String.IsNullOrEmpty(modifiers))
				codeBuilder.Append(' ');

			if (!definition.IsConstructor) {
				Contract.Assume(definition.ReturnType != null);
				codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition.ReturnType));
				codeBuilder.Append(' ');
			}

			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition));

			Contract.Assume(!String.IsNullOrEmpty(codeBuilder.ToString()));
			return new CodeSignature(Language, codeBuilder.ToString());
		}

		public override CodeSignature GenerateSignature(TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var modifiers = String.Join(" ", GetModifiers(definition));
			var codeBuilder = new StringBuilder(modifiers);
			if (!String.IsNullOrEmpty(modifiers))
				codeBuilder.Append(' ');

			var isDelegate = definition.IsDelegateType();
			if (isDelegate) {
				codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition.GetDelegateReturnType()));
				codeBuilder.Append(' ');
			}

			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition));

			if (!isDelegate) {
				var inherits = String.Join(ShortDisplayNameOverlay.ListSeperator, GetBases(definition).Select(b => ShortDisplayNameOverlay.GetDisplayName(b)));
				if (!String.IsNullOrEmpty(inherits)) {
					codeBuilder.Append(" : ");
					codeBuilder.Append(inherits);
				}
			}
			Contract.Assume(!String.IsNullOrEmpty(codeBuilder.ToString()));
			return new CodeSignature(Language, codeBuilder.ToString());
		}

		private static readonly string[] OrderedAccessModifiers = new [] {
			"public",
			"protected internal",
			"protected",
			"internal",
			"private"
		};

		private string MostPublic(string a, string b) {
			var aIndex = Array.IndexOf(OrderedAccessModifiers, a);
			var bIndex = Array.IndexOf(OrderedAccessModifiers, b);
			if (aIndex >= 0) {
				if (bIndex >= 0) {
					return aIndex <= bIndex ? a : b;
				}
				return a;
			}
			if (bIndex >= 0) {
				return b;
			}
			return String.Empty;
		}

		public override CodeSignature GenerateSignature(FieldDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var modifiers = String.Join(" ", GetModifiers(definition));
			var codeBuilder = new StringBuilder(modifiers);
			if (!String.IsNullOrEmpty(modifiers))
				codeBuilder.Append(' ');

			Contract.Assume(definition.FieldType != null);
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition.FieldType));
			codeBuilder.Append(' ');
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName((IMemberDefinition)definition));

			Contract.Assume(!String.IsNullOrEmpty(codeBuilder.ToString()));
			return new CodeSignature(Language, codeBuilder.ToString());
		}

		public override CodeSignature GenerateSignature(EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var modifiers = String.Join(" ", GetModifiers(definition));
			var codeBuilder = new StringBuilder(modifiers);
			if (!String.IsNullOrEmpty(modifiers))
				codeBuilder.Append(' ');

			codeBuilder.Append("event ");

			Contract.Assume(definition.EventType != null);
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition.EventType, true));
			codeBuilder.Append(' ');
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName((IMemberDefinition)definition));

			Contract.Assume(!String.IsNullOrEmpty(codeBuilder.ToString()));
			return new CodeSignature(Language, codeBuilder.ToString());
		}

		public override CodeSignature GenerateSignature(PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();

			var getMethod = definition.GetMethod;
			var getAccess = null == getMethod ? null : GetAccessModifier(getMethod);
			var setMethod = definition.SetMethod;
			var setAccess = null == setMethod ? null : GetAccessModifier(setMethod);
			string outerAccess;

			if (String.IsNullOrEmpty(getAccess)) {
				outerAccess = String.IsNullOrEmpty(setAccess) ? null : setAccess;
			}
			else {
				outerAccess = String.IsNullOrEmpty(setAccess)
					? getAccess
					: MostPublic(getAccess, setAccess);
			}

			var modifiers = new List<String>(2);
			if (!String.IsNullOrEmpty(outerAccess))
				modifiers.Add(outerAccess);

			if (definition.IsStatic())
				modifiers.Add("static");
			if (definition.IsSealed())
				modifiers.Add("sealed");

			if (definition.IsAbstract())
				modifiers.Add("abstract");
			
			if (definition.IsOverride())
				modifiers.Add("override");
			else if (definition.IsVirtual() && definition.IsNewSlot() && !definition.IsFinal())
				modifiers.Add("virtual");

			var codeBuilder = new StringBuilder(String.Join(" ", modifiers));
			if (codeBuilder.Length != 0)
				codeBuilder.Append(' ');

			Contract.Assume(definition.PropertyType != null);
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition.PropertyType));
			codeBuilder.Append(' ');
			codeBuilder.Append(ShortDisplayNameOverlay.GetDisplayName(definition));

			var etters = new List<string>();
			if (null != getMethod) {
				var sig = "get;";
				if (!String.IsNullOrEmpty(getAccess) && getAccess != outerAccess)
					sig = String.Concat(getAccess, ' ', sig);

				etters.Add(sig);
			}
			if (null != setMethod) {
				var sig = "set;";
				if (!String.IsNullOrEmpty(setAccess) && setAccess != outerAccess)
					sig = String.Concat(setAccess, ' ', sig);

				etters.Add(sig);
			}

			if (etters.Count > 0) {
				codeBuilder.Append(" { ");
				codeBuilder.Append(String.Join(" ", etters));
				codeBuilder.Append(" }");
			}

			Contract.Assume(!String.IsNullOrEmpty(codeBuilder.ToString()));
			return new CodeSignature(Language, codeBuilder.ToString());
		}

	}
}
