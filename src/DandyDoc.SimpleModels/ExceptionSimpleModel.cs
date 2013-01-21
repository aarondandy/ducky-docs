using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using DandyDoc.Overlays.XmlDoc;
using DandyDoc.SimpleModels.ComplexText;
using DandyDoc.SimpleModels.Contracts;
using System.Collections.ObjectModel;

namespace DandyDoc.SimpleModels
{
	public class ExceptionSimpleModel : IExceptionSimpleModel
	{

		private static readonly Regex StringNullOrEmptyRegex = new Regex(@"^[Ss]tring.IsNullOrEmpty[(](.+)[)]$");

		public static IComplexTextNode ConvertToComplexTextNode(ParsedXmlException exceptionItem) {
			var node = ParsedXmlDocComplexTextNode.ConvertToSingleComplexNode(exceptionItem.Children);
			var textNode = node as StandardComplexText;
			if (null != textNode){
				return AdjustExceptionTextNode(textNode);
			}
			return node;
		}

		private static string RemoveRedundantNegations(string text){
			Contract.Requires(!String.IsNullOrEmpty(text));
			Contract.Ensures(Contract.Result<string>() != null);
			while (text.StartsWith("!(!(") && text.EndsWith("))")){
				// NOTE: this will cause an issue with something like: !(!(a)) == ((b)), so don't do that!
				text = text.Substring(4, text.Length - 6);
			}
			return text;
		}

		public static IComplexTextNode AdjustExceptionTextNode(StandardComplexText textNode){
			if(textNode == null) throw new ArgumentNullException("textNode");
			Contract.Ensures(Contract.Result<IComplexTextNode>() != null);
			var text = textNode.Text;

			if (!String.IsNullOrEmpty(text))
				text = RemoveRedundantNegations(text);

			var stringNullOrEmptyMatch = StringNullOrEmptyRegex.Match(text);
			if (stringNullOrEmptyMatch.Success){
				return new ComplexTextList(new IComplexTextNode[]{
					new CodeComplexText(true,String.Empty,stringNullOrEmptyMatch.Groups[1].Value),
					new StandardComplexText(" is "),
					new SeeComplexText("null", SeeComplexText.TargetKind.LanguageWord), 
					new StandardComplexText(" or "),
					new SeeComplexText("System.String.Empty", SeeComplexText.TargetKind.CRef, "empty"), 
					new StandardComplexText(".")
				});
			}

			// only create a new instance if the text changed
			return textNode.Text == text
				? textNode
				: new StandardComplexText(text);
		}

		public static ExceptionSimpleModel Create(string exceptionCRef, IEnumerable<ParsedXmlException> xmlExceptions, Converter<ParsedXmlException,IComplexTextNode> converter = null){
			Contract.Requires(xmlExceptions != null);
			Contract.Ensures(Contract.Result<ExceptionSimpleModel>() != null);
			return Create(new CrefSimpleMemberPointer(exceptionCRef), xmlExceptions, converter);
		}

		public static ExceptionSimpleModel Create(ISimpleMemberPointerModel exception, IEnumerable<ParsedXmlException> xmlExceptions, Converter<ParsedXmlException,IComplexTextNode> converter = null) {
			if(null == exception) throw new ArgumentNullException("exception");
			if(null == xmlExceptions) throw new ArgumentNullException("xmlExceptions");
			Contract.Ensures(Contract.Result<ExceptionSimpleModel>() != null);
			if (converter == null)
				converter = ConvertToComplexTextNode;

			var conditions = new List<IComplexTextNode>();
			var ensures = new List<IComplexTextNode>();
			foreach (var exceptionItem in xmlExceptions.Where(ex => ex.Children.Count > 0)) {
				var summary = converter(exceptionItem);
				if (null != summary) {
					(exceptionItem.HasRelatedEnsures ? ensures : conditions).Add(summary);
				}
			}
			return new ExceptionSimpleModel(exception, conditions, ensures);
		}

		public ExceptionSimpleModel(ISimpleMemberPointerModel exceptionType, IList<IComplexTextNode> conditions = null, IList<IComplexTextNode> ensures = null){
			if(null == exceptionType) throw new ArgumentNullException("exceptionType");
			Contract.EndContractBlock();
			ExceptionType = exceptionType;
			Conditions = new ReadOnlyCollection<IComplexTextNode>(conditions ?? new IComplexTextNode[0]);
			Ensures = new ReadOnlyCollection<IComplexTextNode>(ensures ?? new IComplexTextNode[0]);
		}

		public ISimpleMemberPointerModel ExceptionType { get; private set; }

		public bool HasConditions { get { return Conditions.Count > 0; } }

		public IList<IComplexTextNode> Conditions { get; private set; }

		public bool HasEnsures { get { return Ensures.Count > 0; } }

		public IList<IComplexTextNode> Ensures { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(ExceptionType != null);
			Contract.Invariant(Conditions != null);
			Contract.Invariant(Ensures != null);
		}

	}
}
