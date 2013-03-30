using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public class CodeDocType : CodeDocEntityContentBase, ICodeDocType
	{

		public CodeDocType(CRefIdentifier cRef) : base(cRef){
			Contract.Requires(cRef != null);
		}

        public CRefIdentifier BaseCRef { get; set; }

		public bool HasBaseChain { get { return BaseChainCRefs != null && BaseChainCRefs.Count > 0; } }

        public IList<CRefIdentifier> BaseChainCRefs { get; set; }

        public bool HasDirectInterfaces { get { return DirectInterfaceCRefs != null && DirectInterfaceCRefs.Count > 0; } }

        public IList<CRefIdentifier> DirectInterfaceCRefs { get; set; }

        public bool HasGenericParameters { get { return GenericParameters != null && GenericParameters.Count > 0; } }

        public IList<ICodeDocGenericParameter> GenericParameters { get; set; }

        public bool IsEnum { get; set; }

        public bool HasNestedTypes { get { return NestedTypes != null && NestedTypes.Count > 0; } }

        public IList<ICodeDocEntity> NestedTypes { get; set; }

        public bool HasNestedDelegates { get { return NestedDelegates != null && NestedDelegates.Count > 0; } }

        public IList<ICodeDocEntity> NestedDelegates { get; set; }

        public bool HasConstructors { get { return Constructors != null && Constructors.Count > 0; } }

        public IList<ICodeDocEntity> Constructors { get; set; }

        public bool HasMethods { get { return Methods != null && Methods.Count > 0; } }

        public IList<ICodeDocEntity> Methods { get; set; }

        public bool HasOperators { get { return Operators != null && Operators.Count > 0; } }

        public IList<ICodeDocEntity> Operators { get; set; }

        public bool HasProperties { get { return Properties != null && Properties.Count > 0; } }

        public IList<ICodeDocEntity> Properties { get; set; }

        public bool HasFields { get { return Fields != null && Fields.Count > 0; } }

        public IList<ICodeDocEntity> Fields { get; set; }

        public bool HasEvents { get { return Events != null && Events.Count > 0; } }

        public IList<ICodeDocEntity> Events { get; set; }

    }
}
