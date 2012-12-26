using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class TypeDefinitionXmlDoc : DefinitionXmlDocBase
	{

		internal TypeDefinitionXmlDoc(TypeDefinition typeDefinition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(typeDefinition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != typeDefinition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		public ParsedXmlNodeBase DocsForTypeparam(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return ParameterizedXmlDocBase.DocsForTypeparam(name, this);
		}

		public TypeDefinition TypeDefinition { get { return (TypeDefinition)Definition; } }

		public IList<ParsedXmlContractCondition> Invariants{
			get {
				var nodes = SelectParsedXmlNodes("invariant");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlContractCondition)n);
			}
		}

	}
}
