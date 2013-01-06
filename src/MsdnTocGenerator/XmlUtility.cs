using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MsdnTocGenerator
{
	internal static class XmlUtility
	{

		public static string GetValueOrDefault(this XmlAttributeCollection attributes, string key) {
			Contract.Requires(null != attributes);
			Contract.Requires(!String.IsNullOrEmpty(key));
			var attribute = attributes[key];
			return null == attribute ? null : attribute.Value;
		}

	}
}
