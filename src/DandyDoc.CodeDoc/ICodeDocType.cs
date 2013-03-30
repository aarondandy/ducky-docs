using System.Collections.Generic;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
    public interface ICodeDocType : ICodeDocEntityContent, ICodeDocGenericDefinition
	{

		bool HasBaseChain { get; }

		IList<CRefIdentifier> BaseChainCRefs { get; }

		bool HasDirectInterfaces { get; }

        IList<CRefIdentifier> DirectInterfaceCRefs { get; }

        bool IsEnum { get; }

        bool HasNestedTypes { get; }

        IList<ICodeDocEntity> NestedTypes { get; }

        bool HasNestedDelegates { get; }

        IList<ICodeDocEntity> NestedDelegates { get; }

        bool HasConstructors { get; }

        IList<ICodeDocEntity> Constructors { get; }

        bool HasMethods { get; }

        IList<ICodeDocEntity> Methods { get; }

        bool HasOperators { get; }

        IList<ICodeDocEntity> Operators { get; }

        bool HasProperties { get; }

        IList<ICodeDocEntity> Properties { get; }

        bool HasFields { get; }

        IList<ICodeDocEntity> Fields { get; }

        bool HasEvents { get; }

        IList<ICodeDocEntity> Events { get; }

	}
}
