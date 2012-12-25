using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Core.Overlays.Cref;
using DandyDoc.Core.Utility;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParameterizedXmlDocBase : DefinitionXmlDocBase
	{

		internal static ParsedXmlNodeBase DocsForParameter(string name, DefinitionXmlDocBase xmlDoc) {
			Contract.Requires(!String.IsNullOrEmpty(name));
			Contract.Requires(null != xmlDoc);
			var query = String.Format("param[@name=\"{0}\"]", name);
			Contract.Assume(!String.IsNullOrEmpty(query));
			return xmlDoc.SelectParsedXmlNode(query);
		}

		internal static ParsedXmlNodeBase DocsForTypeparam(string name, DefinitionXmlDocBase xmlDoc){
			Contract.Requires(!String.IsNullOrEmpty(name));
			Contract.Requires(null != xmlDoc);
			var query = String.Format("typeparam[@name=\"{0}\"]", name);
			Contract.Assume(!String.IsNullOrEmpty(query));
			return xmlDoc.SelectParsedXmlNode(query);
		}

		internal ParameterizedXmlDocBase(IMemberDefinition definition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(definition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != definition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		public ParsedXmlNodeBase DocsForParameter(string name) {
			if(String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return DocsForParameter(name, this);
		}

		public ParsedXmlNodeBase DocsForTypeparam(string name){
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return DocsForTypeparam(name, this);
		}

		public ParsedXmlNodeBase Returns { get { return SelectParsedXmlNode("returns"); } }

		public IList<ParsedXmlException> Exceptions{
			get{
				var nodes = SelectParsedXmlNodes("exception");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlException)n);
			}
		}

	}
}
