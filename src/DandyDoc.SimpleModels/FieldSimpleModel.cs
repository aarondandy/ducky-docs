using System.Diagnostics.Contracts;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class FieldSimpleModel : DefinitionMemberSimpleModelBase<FieldDefinition>, IFieldSimpleModel
	{

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
				return new ReferenceSimpleMemberPointer(FullTypeDisplayNameOverlay.GetDisplayName(Definition.FieldType), Definition.FieldType);
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
	}
}
