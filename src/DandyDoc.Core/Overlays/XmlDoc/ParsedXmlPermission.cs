using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Overlays.XmlDoc
{
	/// <summary>
	/// A parsed permission node referencing a permission member or type.
	/// </summary>
	/// <seealso href="http://msdn.microsoft.com/en-us/library/h9df2kfb.aspx"/>
	[Obsolete]
	public class ParsedXmlPermission : ParsedCrefXmlElementBase
	{

		internal ParsedXmlPermission(XmlElement element, DefinitionXmlDocBase xmlDocBase)
			: base(element, xmlDocBase)
		{
			Contract.Requires(null != element);
			Contract.Requires(null != xmlDocBase);
		}

	}
}
