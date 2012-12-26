using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Overlays.XmlDoc;
using Mono.Cecil;

namespace Mvc4WebDirectDocSample.Models
{
	public class MethodSummaryTable
	{

		public enum Kind
		{
			Method = 0,
			Constructor,
			Operator
		}

		public IList<MethodDefinition> Methods { get; set; }

		public Kind MethodTableKind { get; set; }

		public XmlDocOverlay XmlDocOverlay { get; set; }

		public CrefOverlay CrefOverlay { get; set; }

	}
}