using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mono.Cecil;

namespace DandyDoc.Core
{
	public class GenericTypeRecord : IDocumentableEntity
	{

		private static readonly ReadOnlyCollection<ParsedXmlDoc> EmptyDocList = Array.AsReadOnly(new ParsedXmlDoc[0]);
		private static readonly ReadOnlyCollection<SeeAlsoReference> EmptySeeAlso = Array.AsReadOnly(new SeeAlsoReference[0]);

		public GenericTypeRecord(GenericParameter parameter) {
			CoreParameter = parameter;
		}

		public GenericParameter CoreParameter { get; private set; }

		public string Name { get { return CoreParameter.Name; } }

		public ParsedXmlDoc Summary {
			get {
				throw new NotImplementedException();
			}
		}

		public IList<ParsedXmlDoc> Remarks { get { return EmptyDocList; } }

		public IList<ParsedXmlDoc> Examples { get { return EmptyDocList; } }

		public IList<SeeAlsoReference> SeeAlso { get { return EmptySeeAlso; } }

		public XmlNode XmlDocNode {
			get { throw new NotImplementedException(); }
		}

		public IDocumentableEntity ResolveCref(string cref) {
			throw new NotImplementedException();
		}

		public string Cref {
			get { throw new NotImplementedException(); }
		}
	}
}
