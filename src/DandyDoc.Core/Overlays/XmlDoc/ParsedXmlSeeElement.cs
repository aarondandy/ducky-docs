using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	/// <summary>
	/// A parsed cref hyperlink element used to refer a documentation reader to another type or member.
	/// </summary>
	public class ParsedXmlSeeElement : ParsedCrefXmlElementBase
	{

		internal ParsedXmlSeeElement(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

		/// <summary>
		/// Get a language word that the see element is referring to.
		/// </summary>
		public string LanguageWord{
			get {
				var langwordAttribute = Element.Attributes["langword"];
				return null == langwordAttribute ? null : langwordAttribute.Value;
			}
		}

	}
}
