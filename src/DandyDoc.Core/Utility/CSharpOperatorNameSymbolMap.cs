using System.Collections.Generic;

namespace DandyDoc.Utility
{
	public static class CSharpOperatorNameSymbolMap
	{

		private static readonly Dictionary<string, string> NameToSymbol = new Dictionary<string, string>{
			{"op_Addition","+"},
			{"op_Subtraction","-"},
			{"op_Multiply","*"},
			{"op_Division","/"},
			{"op_Modulus","%"},
			{"op_ExclusiveOr","^"},
			{"op_BitwiseAnd","&"},
			{"op_BitwiseOr","|"},
			{"op_LogicalAnd","&&"},
			{"op_LogicalOr","||"},
			{"op_Assign","="},
			{"op_LeftShift","<<"},
			{"op_RightShift",">>"},
			{"op_Equality","=="},
			{"op_GreaterThan",">"},
			{"op_LessThan","<"},
			{"op_Inequality","!="},
			{"op_GreaterThanOrEqual",">="},
			{"op_LessThanOrEqual","<="},
			{"op_MultiplicationAssignment","*="},
			{"op_SubtractionAssignment","-="},
			{"op_ExclusiveOrAssignment","^="},
			{"op_LeftShiftAssignment","<<="},
			{"op_ModulusAssignment","%="},
			{"op_AdditionAssignment","+="},
			{"op_BitwiseAndAssignment","&="},
			{"op_BitwiseOrAssignment","|="},
			{"op_Comma",","},
			{"op_DivisionAssignment","/="},
			{"op_Decrement","--"},
			{"op_Increment","++"},
			{"op_UnaryNegation","-"},
			{"op_UnaryPlus","+"},
			{"op_OnesComplement","~"}
		};

		public static bool IsOperatorName(string name) {
			return NameToSymbol.ContainsKey(name);
		}

		public static string OperatorSymbol(string name) {
			string symbol;
			if(name == null)
				symbol = null;
			else
				NameToSymbol.TryGetValue(name, out symbol);
			return symbol;
		}

		public static bool TryGetOperatorSymbol(string name, out string symbol) {
			if (name == null) {
				symbol = null;
				return false;
			}
			return NameToSymbol.TryGetValue(name, out symbol);
		}

	}
}
