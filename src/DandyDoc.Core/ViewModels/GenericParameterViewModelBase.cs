using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.DisplayName;
using DandyDoc.Overlays.XmlDoc;
using Mono.Cecil;

namespace DandyDoc.ViewModels
{
	public abstract class GenericParameterViewModelBase
	{

		public interface IConstraint
		{
			string DisplayName { get; }
		}

		public class TypeConstraint : IConstraint
		{
			private static readonly DisplayNameOverlay DefaultFullDisplayNameOverlay = new DisplayNameOverlay {
				IncludeNamespaceForTypes = true,
				ShowGenericParametersOnDefinition = true
			};

			internal TypeConstraint(TypeReference reference){
				Contract.Requires(null != reference);
				Reference = reference;
			}

			public TypeReference Reference { get; private set; }

			public string DisplayName {
				get { return DefaultFullDisplayNameOverlay.GetDisplayName(Reference); }
			}

			[ContractInvariantMethod]
			private void CodeContractInvariant() {
				Contract.Invariant(null != Reference);
			}

		}

		private class DefaultConstructorConstraint : IConstraint
		{
			public string DisplayName { get { return "Default Constructor"; } }
		}

		private class StructConstraint : IConstraint
		{
			public string DisplayName { get { return "Value Type"; } }
		}

		private class ReferenceTypeContraints : IConstraint
		{
			public string DisplayName { get { return "Reference Type"; } }
		}

		private readonly Lazy<ReadOnlyCollection<IConstraint>> _contraints;

		protected GenericParameterViewModelBase(GenericParameter parameter){
			if(null == parameter) throw new ArgumentNullException("parameter");
			Contract.EndContractBlock();
			Parameter = parameter;
			_contraints = new Lazy<ReadOnlyCollection<IConstraint>>(GenerateConstraints);
		}

		private ReadOnlyCollection<IConstraint> GenerateConstraints(){
			var results = new List<IConstraint>();

			if (Parameter.HasNotNullableValueTypeConstraint)
				results.Add(new StructConstraint());
			else if(Parameter.HasReferenceTypeConstraint)
				results.Add(new ReferenceTypeContraints());

			if (Parameter.HasConstraints){
				foreach (var referenceType in Parameter.Constraints){
					if (Parameter.HasNotNullableValueTypeConstraint && referenceType.FullName == "System.ValueType")
						continue;
					
					results.Add(new TypeConstraint(referenceType));
				}
			}

			if (Parameter.HasDefaultConstructorConstraint && !Parameter.HasNotNullableValueTypeConstraint)
				results.Add(new DefaultConstructorConstraint());

			return new ReadOnlyCollection<IConstraint>(results);
		}

		public bool HasConstraints {
			get { return Parameter.HasConstraints && Constraints.Count > 0; }
		}

		public ReadOnlyCollection<IConstraint> Constraints { get { return _contraints.Value; } }

		public GenericParameter Parameter { get; private set; }

		public virtual string DisplayName {
			get {
				Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
				Contract.Assume(!String.IsNullOrEmpty(Parameter.Name));
				return Parameter.Name;
			}
		}

		public bool HasXmlDoc {
			get { return null != XmlDoc; }
		}

		public abstract ParsedXmlElementBase XmlDoc { get; }

		[ContractInvariantMethod]
		private void CodeContractInvariant() {
			Contract.Invariant(Parameter != null);
		}

	}
}
