using System;
using System.Collections.Generic;

namespace DandyDoc.Utility
{
    /// <summary>
    /// A utility class which provides a mapping between C# operator
    /// symbols and their method names.
    /// </summary>
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

        /// <summary>
        /// Determines if a given method <paramref name="name"/> is an operator method.
        /// </summary>
        /// <param name="name">The method name to test.</param>
        /// <returns><c>true</c> when the method name is an operator.</returns>
        [Obsolete("This may move to a language agnostic class.")]
        public static bool IsOperatorName(string name) {
            return NameToSymbol.ContainsKey(name)
                || IsConversionOperatorMethodName(name);
        }

        /// <summary>
        /// Determines if a given method <paramref name="name"/> is an implicit or explicit conversion operator.
        /// </summary>
        /// <param name="name">The method name to test.</param>
        /// <returns><c>true</c> when the method name is a conversion.</returns>
        [Obsolete("This may move to a language agnostic class.")]
        public static bool IsConversionOperatorMethodName(string name){
            return "op_Implicit".Equals(name) || "op_Explicit".Equals(name);
        }

        /// <summary>
        /// Attempts to resolve a method name to a C# operator symbol.
        /// </summary>
        /// <param name="name">The method name to search for.</param>
        /// <returns>The related C# operator symbol or <c>null</c> if not found.</returns>
        public static string OperatorSymbol(string name) {
            string symbol;
            if (name == null)
                symbol = null;
            else
                NameToSymbol.TryGetValue(name, out symbol);
            return symbol;
        }

        /// <summary>
        /// Attempts to resolve a method name to a C# operator symbol.
        /// </summary>
        /// <param name="name">The method name to search for.</param>
        /// <param name="symbol">The related C# operator symbol if successful.</param>
        /// <returns><c>true</c> when a match is found.</returns>
        public static bool TryGetOperatorSymbol(string name, out string symbol) {
            if (name == null) {
                symbol = null;
                return false;
            }
            return NameToSymbol.TryGetValue(name, out symbol);
        }

    }
}
