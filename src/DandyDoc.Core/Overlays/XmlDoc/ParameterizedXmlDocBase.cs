using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Overlays.Cref;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParameterizedXmlDocBase : DefinitionXmlDocBase
	{

		internal static ParsedXmlElementBase DocsForParameter(string name, DefinitionXmlDocBase xmlDoc) {
			Contract.Requires(!String.IsNullOrEmpty(name));
			Contract.Requires(null != xmlDoc);
			var query = String.Format("param[@name=\"{0}\"]", name);
			Contract.Assume(!String.IsNullOrEmpty(query));
			return xmlDoc.SelectParsedXmlNode(query) as ParsedXmlElementBase;
		}

		internal static ParsedXmlElementBase DocsForTypeparam(string name, DefinitionXmlDocBase xmlDoc) {
			Contract.Requires(!String.IsNullOrEmpty(name));
			Contract.Requires(null != xmlDoc);
			var query = String.Format("typeparam[@name=\"{0}\"]", name);
			Contract.Assume(!String.IsNullOrEmpty(query));
			return xmlDoc.SelectParsedXmlNode(query) as ParsedXmlElementBase;
		}

		internal ParameterizedXmlDocBase(IMemberDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		public ParsedXmlElementBase DocsForParameter(string name) {
			if(String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return DocsForParameter(name, this);
		}

		public ParsedXmlElementBase DocsForTypeparam(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return DocsForTypeparam(name, this);
		}

		public ParsedXmlElementBase Returns { get { return SelectParsedXmlNode("returns") as ParsedXmlElementBase; } }

		public IList<ParsedXmlException> Exceptions{
			get{
				var nodes = SelectParsedXmlNodes("exception");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlException)n);
			}
		}

	}
}
