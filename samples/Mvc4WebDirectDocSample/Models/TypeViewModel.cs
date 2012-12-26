using System.Collections.Generic;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;
using DandyDoc.Core.Overlays.Cref;

namespace Mvc4WebDirectDocSample.Models
{
	public class TypeViewModel
	{

		public TypeDefinition Definition { get; set; }
		public CrefOverlay CrefOverlay { get; set; }
		public XmlDocOverlay XmlDocOverlay { get; set; }
		public DefinitionXmlDocBase XmlDoc { get; set; }

		public MethodSummaryTable CreateMethodSummaryTable(IList<MethodDefinition> methods, MethodSummaryTable.Kind kind){
			return new MethodSummaryTable{
				CrefOverlay = CrefOverlay,
				XmlDocOverlay = XmlDocOverlay,
				Methods = methods,
				MethodTableKind = kind
			};
		}

		public FieldSummaryTable CreateFieldSummaryTable(IList<FieldDefinition> fields){
			return new FieldSummaryTable{
				CrefOverlay = CrefOverlay,
				XmlDocOverlay = XmlDocOverlay,
				Fields = fields
			};
		}

	}
}