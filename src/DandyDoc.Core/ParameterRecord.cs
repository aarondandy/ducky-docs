using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Xml;
using Mono.Cecil;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DandyDoc.Core
{
	public class ParameterRecord : IDocumentableEntity
	{

		private static readonly ReadOnlyCollection<ParsedXmlDoc> EmptyParsedXmlDocList = Array.AsReadOnly(new ParsedXmlDoc[0]);
		private static readonly ReadOnlyCollection<SeeAlsoReference> EmptySeeAlsoReferences = Array.AsReadOnly(new SeeAlsoReference[0]);

		internal ParameterRecord(MemberRecord parent, ParameterDefinition parameterInfo) {
			if(null == parent) throw new ArgumentNullException("parent");
			if(null == parameterInfo) throw new ArgumentNullException("parameterInfo");
			Contract.EndContractBlock();
			CoreParameterInfo = parameterInfo;
			ParentEntity = parent;
		}

		public ParameterDefinition CoreParameterInfo { get; private set; }

		public MemberRecord ParentEntity { get; private set; }

		public ParsedXmlDoc Summary { get { return new ParsedXmlDoc(XmlDocNode, this); } }

		public IList<ParsedXmlDoc> Remarks { get { return EmptyParsedXmlDocList; } }

		public IList<ParsedXmlDoc> Examples { get { return EmptyParsedXmlDocList; } }

		public IList<SeeAlsoReference> SeeAlso { get { return EmptySeeAlsoReferences; } }

		public string FullTypeName { get { return CoreParameterInfo.ParameterType.FullName; } }

		public string Name { get { return CoreParameterInfo.Name; } }

		public XmlNode XmlDocNode {
			get { return ParentEntity.XmlDocNode.SelectSingleNode(String.Format("param[@name=\"{0}\"]", CoreParameterInfo.Name)); }
		}

		public IDocumentableEntity ResolveCref(string crefName){
			return ParentEntity.ResolveCref(crefName);
		}

		public string Cref {
			get { return ParentEntity.Cref + ':' + Name; }
		}

	}
}
