using System.Xml;
using DandyDoc.CRef;

namespace DandyDoc.CodeDoc
{
	public interface ICodeDocContractCondition
	{

        CRefIdentifier ExceptionCRef { get; }

		bool HasExceptionDescription { get; }

		XmlNodeList ExceptionDescription { get; }

		bool HasContractDescription { get; }

		XmlNodeList ContractDescription { get; }

	}
}
