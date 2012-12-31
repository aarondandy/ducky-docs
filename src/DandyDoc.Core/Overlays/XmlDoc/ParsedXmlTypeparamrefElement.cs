using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{
	public class ParsedXmlTypeparamrefElement : ParsedXmlElementBase
	{

		internal ParsedXmlTypeparamrefElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(element != null);
			Contract.Requires(docBase != null);
		}

		public string TypeparamName {
			get{
				var nameAttribute = Node.Attributes["name"];
				return null == nameAttribute ? null : nameAttribute.Value;
			}
		}

		public GenericParameter Target {
			get {
				var definition = DocBase.Definition;
				var typeparamName = TypeparamName;
				if (string.IsNullOrEmpty(typeparamName))
					return null;

				var methodDefinition = definition as MethodDefinition;
				TypeDefinition typeDefinition;
				if (null != methodDefinition){
					if (methodDefinition.HasGenericParameters){
						var result = methodDefinition.GenericParameters.FirstOrDefault(p => typeparamName.Equals(p.Name));
						if (null != result)
							return result;
					}
					typeDefinition = methodDefinition.DeclaringType;
				}
				else {
					typeDefinition = definition as TypeDefinition;
				}

				return null != typeDefinition && typeDefinition.HasGenericParameters
					? typeDefinition.GenericParameters.FirstOrDefault(p => typeparamName.Equals(p.Name))
					: null;
			}
		}

		private static GenericParameter GetParameterByName(TypeDefinition definition, string name){
			Contract.Requires(!String.IsNullOrEmpty(name));
			Contract.Requires(null != definition);
			return definition.HasGenericParameters ? definition.GenericParameters.FirstOrDefault(p => name.Equals(p.Name)) : null;
		}

	}
}
