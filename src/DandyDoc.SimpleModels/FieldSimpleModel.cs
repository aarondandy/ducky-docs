using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class FieldSimpleModel : DefinitionMemberSimpleModelBase<FieldDefinition>, IFieldSimpleModel
	{
		protected static readonly IFlairTag DefaultConstantTag = new SimpleFlairTag("constant", "Value", "This field is a constant.");
		protected static readonly IFlairTag DefaultReadOnlyTag = new SimpleFlairTag("readonly", "Value", "This field is only assignable during instantiation.");

		public FieldSimpleModel(FieldDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
		}

		public override string SubTitle {
			get {
				if (Definition.HasConstant)
					return "Constant";
				return "Field";
			}
		}

		protected FieldDefinitionXmlDoc FieldXmlDocs { get { return DefinitionXmlDocs as FieldDefinitionXmlDoc; } }

		public ISimpleMemberPointerModel FieldType {
			get {
				Contract.Assume(Definition.FieldType != null);
				return new ReferenceSimpleMemberPointer(Definition.FieldType, FullTypeDisplayNameOverlay.GetDisplayName(Definition.FieldType));
			}
		}

		public bool HasValueDescription {
			get { return ValueDescription != null; }
		}

		public IComplexTextNode ValueDescription {
			get { var xmlDoc = FieldXmlDocs;
				if (xmlDoc == null)
					return null;
				var valueDoc = xmlDoc.ValueDoc;
				if (null == valueDoc)
					return null;
				return ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(valueDoc.Children);
			}
		}

		public override IList<IFlairTag> FlairTags {
			get {
				var tags = base.FlairTags;

				if(Definition.HasConstant)
					tags.Add(DefaultConstantTag);

				else if(Definition.IsInitOnly)
					tags.Add(DefaultReadOnlyTag);

				return tags;
			}
		}

	}
}
