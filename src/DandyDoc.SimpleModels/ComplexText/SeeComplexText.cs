using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels.ComplexText
{
	public class SeeComplexText : ComplexTextList
	{

		public enum TargetKind
		{
			None,
			CRef,
			HRef,
			LanguageWord
		}

		public SeeComplexText(string target, TargetKind kind)
			: this(target, kind, new IComplexTextNode[0]) { }

		public SeeComplexText(string target, TargetKind kind, string description)
			: this(target, kind, new IComplexTextNode[]{new StandardComplexText(description)}) { }

		public SeeComplexText(string target, TargetKind kind, IList<IComplexTextNode> children)
			: base(children)
		{
			Contract.Requires(children != null);
			Target = target;
			Kind = kind;
		}

		public string Target { get; private set; }

		public TargetKind Kind { get; private set; }

	}
}
