using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using Mono.Cecil;

namespace DandyDoc.Core.Overlays.XmlDoc
{
	public class ParsedXmlParamrefElement : ParsedXmlElementBase
	{

		internal ParsedXmlParamrefElement(XmlElement element, DefinitionXmlDocBase docBase)
			: base(element, docBase)
		{
			Contract.Requires(element != null);
			Contract.Requires(docBase != null);
		}

		public string ParameterName {
			get{
				var nameAttribute = Node.Attributes["name"];
				return null == nameAttribute ? null : nameAttribute.Value;
			}
		}

		public ParameterDefinition Target {
			get {
				var definition = DocBase.Definition;
				var parameterName = ParameterName;
				if (string.IsNullOrEmpty(parameterName))
					return null;

				var methodDefinition = definition as MethodDefinition;
				if (null != methodDefinition){
					if (methodDefinition.HasParameters)
						return methodDefinition.Parameters.FirstOrDefault(p => p.Name == parameterName);
					return null;
				}

				var propertyDefinition = definition as PropertyDefinition;
				if (null != propertyDefinition){
					if (propertyDefinition.HasParameters)
						return propertyDefinition.Parameters.FirstOrDefault(p => p.Name == parameterName);
					return null;
				}

				var typeDefinition = definition as TypeDefinition;
				if (null != typeDefinition){
					if (typeDefinition.IsDelegateType())
						return typeDefinition.GetDelegateTypeParameters().FirstOrDefault(p =>p.Name == parameterName);
					return null;
				}

				return null;
			}
		}

	}
}
