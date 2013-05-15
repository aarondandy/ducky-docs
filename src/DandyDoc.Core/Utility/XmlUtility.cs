using System;
using System.Diagnostics.Contracts;
using System.Xml;

namespace DandyDoc.Utility
{
    internal static class XmlUtility
    {

        public static string GetValueOrDefault(this XmlAttributeCollection attributes, string key) {
            Contract.Requires(null != attributes);
            Contract.Requires(!String.IsNullOrEmpty(key));
            var attribute = attributes[key];
            return null == attribute ? null : attribute.Value;
        }

        public static XmlElement FindPreviousSiblingElement(this XmlElement element) {
            XmlNode node = element.PreviousSibling;
            while (null != node) {
                var result = node as XmlElement;
                if (null != result)
                    return result;
                node = node.PreviousSibling;
            }
            return null;
        }

    }
}
