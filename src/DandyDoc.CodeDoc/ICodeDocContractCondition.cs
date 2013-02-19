using System.Xml;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocContractCondition
	{

		string ExceptionCRef { get; }

		bool HasExceptionDescription { get; }

		XmlNodeList ExceptionDescription { get; }

		bool HasContractDescription { get; }

		XmlNodeList ContractDescription { get; }

	}
}
